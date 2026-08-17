using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.LibDave;
using Discord.LibDave.Binding;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mauren.Discord.Infrastructure
{
    internal class LoggingBinderService : BackgroundService
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly InteractionService _interactions;

        private readonly ILogger _clientLogger;
        private readonly ILogger _commandLogger;
        private readonly ILogger _interactionsLogger;
        private readonly ILogger _daveLogger;

        private readonly TaskCompletionSource<Object> _taskCompletionSource;

        public LoggingBinderService(ILoggerFactory loggerFactory, DiscordSocketClient client,
            CommandService commands, InteractionService interactions)
        {
            _client = client;
            _clientLogger = loggerFactory.CreateLogger(client.GetType());

            _commands = commands;
            _commandLogger = loggerFactory.CreateLogger(commands.GetType());

            _interactions = interactions;
            _interactionsLogger = loggerFactory.CreateLogger(interactions.GetType());

            _taskCompletionSource = new();

            _daveLogger = loggerFactory.CreateLogger(typeof(Dave).FullName ?? "DAVE");
            Dave.SetLogSink((LoggingSeverity severity, String file, Int32 line, String message) =>
            {
                LogLevel loglevel = severity switch
                {
                    LoggingSeverity.Error => LogLevel.Error,
                    LoggingSeverity.Warning => LogLevel.Warning,
                    LoggingSeverity.Info => LogLevel.Information,
                    LoggingSeverity.Verbose => LogLevel.Trace,
                    LoggingSeverity.None => LogLevel.None,

                    _ => LogLevel.None,
                };

                _daveLogger.Log(loglevel, "{message}", message);
            });
        }

        /// <inheritdoc/>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Subscribe to events
            _client.Log += OnClientLog;
            _commands.Log += OnCommandLog;
            _interactions.Log += OnInteractionLog;

            // Register a cleanup callback for on cancellation
            using CancellationTokenRegistration register = stoppingToken.Register(() => _taskCompletionSource.TrySetResult(true));

            // Await the task completion source
            await _taskCompletionSource.Task;

            // Unsubscribe to events
            _client.Log -= OnClientLog;
            _commands.Log -= OnCommandLog;
            _interactions.Log -= OnInteractionLog;
        }

        private Task OnClientLog(LogMessage value)
        {
            // Convert log level
            LogLevel logLevel = value.Severity.Convert();

            // If log level is disabled
            if (_clientLogger.IsEnabled(logLevel) is false)
                // Short circuit
                return Task.CompletedTask;

            if (value.Exception is Exception exception)
                // Log message
                _clientLogger.Log(logLevel, exception, "{message}", value.Message);

            else
                // Log message
                _clientLogger.Log(logLevel, "{message}", value.Message);

            // Return completed task
            return Task.CompletedTask;
        }

        private Task OnCommandLog(LogMessage value)
        {
            // Convert log level
            LogLevel logLevel = value.Severity.Convert();

            // If log level is disabled
            if (_commandLogger.IsEnabled(logLevel) is false)
                // Short circuit
                return Task.CompletedTask;

            if (value.Exception is Exception exception)
                // Log message
                _commandLogger.Log(logLevel, exception, "{message}", value.Message);

            else
                // Log message
                _commandLogger.Log(logLevel, "{message}", value.Message);

            // Return completed task
            return Task.CompletedTask;
        }

        private Task OnInteractionLog(LogMessage value)
        {
            // Convert log level
            LogLevel logLevel = value.Severity.Convert();

            // If log level is disabled
            if (_interactionsLogger.IsEnabled(logLevel) is false)
                // Short circuit
                return Task.CompletedTask;

            if (value.Exception is Exception exception)
                // Log message
                _interactionsLogger.Log(logLevel, exception, "{message}", value.Message);

            else
                // Log message
                _interactionsLogger.Log(logLevel, "{message}", value.Message);

            // Return completed task
            return Task.CompletedTask;
        }
    }
}

namespace Discord
{
    public static class LogMessageExtensions
    {
        public static LogLevel Convert(this LogSeverity severity) => severity switch
        {
            Discord.LogSeverity.Critical => LogLevel.Critical,
            Discord.LogSeverity.Error => LogLevel.Error,
            Discord.LogSeverity.Warning => LogLevel.Warning,
            Discord.LogSeverity.Info => LogLevel.Information,
            Discord.LogSeverity.Debug => LogLevel.Debug,
            Discord.LogSeverity.Verbose => LogLevel.Trace,

            _ => throw new NotImplementedException(),
        };
    }
}
