using Discord;
using Discord.WebSocket;
using Mauren.Discord.Application.Abstractions;
using Mauren.Discord.Application.Features.Configuration;
using Mauren.Discord.Infrastructure.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Mauren.Discord.Infrastructure.Hosting
{
    internal class BotWorker : BackgroundService
    {
        /// <summary>
        /// The <see cref="ILogger{TCategoryName}"/> used to record operation logs.
        /// </summary>
        private readonly ILogger<BotWorker> _logger;

        /// <summary>
        /// The <see cref="IOptionsProvider{TOptions}"/> for the bot options.
        /// </summary>
        private readonly IOptionsProvider<BotOptions> _optionsProvider;

        /// <summary>
        /// 
        /// </summary>
        private readonly DiscordSocketClient _client;

        /// <summary>
        /// A <see cref="Channel{T}"/> for reading and writing token values
        /// on change.
        /// </summary>
        private readonly Channel<String> _tokenChannel;

        /// <summary>
        /// 
        /// </summary>
        private readonly TokenObserver _tokenObserver;

        /// <summary>
        /// 
        /// </summary>
        private readonly BotWorkerController<IBotWorker> _controller;

        public BotWorker(ILogger<BotWorker> logger, BotWorkerController<IBotWorker> controller, IOptionsProvider<BotOptions> optionsProvider, DiscordSocketClient client)
        {
            _logger = logger;
            _optionsProvider = optionsProvider;
            _client = client;
            _controller = controller;
            
            _tokenChannel = Channel.CreateUnbounded<String>(new UnboundedChannelOptions
            {
                SingleWriter = false,
                SingleReader = true,
            });

            _tokenObserver = new(_logger, _tokenChannel.Writer);

            _client.LoggedIn += OnLoggedIn;
            _client.LoggedOut += OnLoggedOut;
        }

        private Task OnLoggedIn()
        {
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.Log(LogLevel.Information, "Logged in to Discord");

            return Task.CompletedTask;
        }

        private Task OnLoggedOut()
        {
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.Log(LogLevel.Information, "Logged out from Discord");

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            CancellationTokenSource? currentCancellationTokenSource = null;
            Task? currentTask = null;

            // Start the bot worker at startup
            await _controller.StartAsync(stoppingToken);

            // Wait for commands from the controller
            await foreach (SetBotWorkerStatusCommand command in _controller.ReadAllAsync(stoppingToken))
            {
                // If a resume command was received
                if (command == SetBotWorkerStatusCommand.Resume && currentTask is null)
                {
                    if (_logger.IsEnabled(LogLevel.Information))
                        _logger.Log(LogLevel.Information, "Resuming {service}", this.GetType().Name);

                    // Set the current cancellation token source to a source linked with the main cancellation token
                    currentCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                    // Run the worker loop with the linked token source and assign the task
                    currentTask = RunAsync(currentCancellationTokenSource.Token);

                    // Report state back to the controller
                    _controller.Update(true);
                }
                else if (command == SetBotWorkerStatusCommand.Pause && currentTask is not null)
                {
                    if (_logger.IsEnabled(LogLevel.Information))
                        _logger.Log(LogLevel.Information, "Pausing {service}", this.GetType().Name);

                    // Cancel the current cancellation token source
                    currentCancellationTokenSource?.Cancel();

                    try
                    {
                        // Wait for the current task to complete shutdown
                        await currentTask;
                    }
                    // Catch operation canceled exception as it is expected
                    catch (OperationCanceledException) { }

                    // Dispose the cancellation token source
                    currentCancellationTokenSource?.Dispose();
                    // Set the current task to null
                    currentTask = null;

                    // Report state back to the controller
                    _controller.Update(false);
                }
            }
        }

        /// <inheritdoc/>
        protected async Task RunAsync(CancellationToken stoppingToken)
        {
            try
            {
                if (_logger.IsEnabled(LogLevel.Information))
                    _logger.Log(LogLevel.Information, "{service} started", this.GetType().Name);

                // Subscribe the token observer to receive token changes from the options provider
                using IDisposable subscription = _optionsProvider.Subscribe(_tokenObserver);

                // If the current token value is not null/whitespace
                if (String.IsNullOrWhiteSpace(_optionsProvider.Current?.Token) is false)
                {
                    // Write the current token value to the channel
                    await _tokenChannel.Writer.WriteAsync(_optionsProvider.Current.Token, stoppingToken).ConfigureAwait(false);
                }

                // Initialize the current token variable
                String? currentToken = null;

                // Idle until new token values arrive or shutdown is requested
                while (await _tokenChannel.Reader.WaitToReadAsync(stoppingToken).ConfigureAwait(false))
                {
                    // Initialize the updated token variable
                    String? updatedToken = null;

                    // Synchronously drain all available token values from the channel queue
                    while (_tokenChannel.Reader.TryRead(out String? readValue))
                    {
                        // If the token value is not null/whitespace
                        if (String.IsNullOrWhiteSpace(readValue) is false)
                        {
                            // Set the updated token to the read token
                            updatedToken = readValue;
                        }
                    }

                    // If the updated token value is the same as the current token value, continue with next loop
                    if (updatedToken == currentToken) continue;

                    // Set the current token value as the updated token value
                    currentToken = updatedToken;

                    // If the current token value is null/whitespace
                    if (String.IsNullOrWhiteSpace(currentToken))
                    {
                        if (_logger.IsEnabled(LogLevel.Warning))
                            _logger.Log(LogLevel.Warning, "Provided token value was null");

                        // Continue with next loop
                        continue;
                    }

                    if (_logger.IsEnabled(LogLevel.Information))
                        _logger.Log(LogLevel.Information, "Token value change detected");

                    // Restart the client
                    await StartClientAsync(currentToken, stoppingToken);
                }
            }
            finally
            {
                // Stop the client
                await StopClientAsync(stoppingToken);
            }
        }

        private async Task StartClientAsync(String token, CancellationToken cancellationToken = default)
        {
            // Log the client in to a new session
            await _client.LoginAsync(TokenType.Bot, token, validateToken: true)
                .ConfigureAwait(false);

            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.Log(LogLevel.Debug, "Starting client");

            // Start the client
            await _client.StartAsync()
                .ConfigureAwait(false);

            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.Log(LogLevel.Debug, "Started client");
        }

        private async Task StopClientAsync(CancellationToken cancellationToken = default)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.Log(LogLevel.Debug, "Stopping client");

            // Stop the client
            await _client.StopAsync()
                .ConfigureAwait(false);

            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.Log(LogLevel.Debug, "Stopped client");

            // Log the client out of the current session
            await _client.LogoutAsync()
                .ConfigureAwait(false);
        }
    }

    internal sealed class TokenObserver : IObserver<BotOptions>
    {
        /// <summary>
        /// The <see cref="ILogger"/> used to record operation logs.
        /// </summary>
        private readonly ILogger _logger;
        /// <summary>
        /// A <see cref="ChannelWriter{T}"/> for writing observed
        /// token changes to a <see cref="Channel{T}"/>.
        /// </summary>
        private readonly ChannelWriter<String> _channelWriter;

        public TokenObserver(ILogger logger, ChannelWriter<String> channelWriter)
        {
            _logger = logger;
            _channelWriter = channelWriter;
        }

        /// <inheritdoc/>
        void IObserver<BotOptions>.OnNext(BotOptions value)
        {
            // If the observed options has a null or empty token value
            if (String.IsNullOrWhiteSpace(value.Token)) { return; }

            _logger.Log(LogLevel.Debug, "Token observer detected a configuration change.");

            if (_channelWriter.TryWrite(value.Token) is false)
            {
                _logger.Log(LogLevel.Information, "Failed to write new token");
            }
        }

        /// <inheritdoc/>
        void IObserver<BotOptions>.OnError(Exception error)
        {
            _logger.Log(LogLevel.Warning, error, "Token observer received an error.");
        }

        /// <inheritdoc/>
        void IObserver<BotOptions>.OnCompleted()
        {
            _logger.Log(LogLevel.Information, "Token observer received a completion.");
        }
    }
}
