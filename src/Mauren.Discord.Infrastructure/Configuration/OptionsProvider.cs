using Mauren.Discord.Application.Abstractions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Mauren.Discord.Infrastructure.Configuration
{
    /// <inheritdoc/>
    /// 
    /// <remarks>
    /// <para>
    /// This implementation synchronizes concurrent updates using a 
    /// <see cref="SemaphoreSlim"/> to ensure thread-safe operations
    /// when reading or writing options.
    /// </para>
    /// 
    /// <para>
    /// Persistence is handled via JSON serialization directly to disk.
    /// File writes utilize a temporary file exchange pattern to prevent 
    /// configuration file corruption in the event of an unhandled exception
    /// or abrupt shutdown.
    /// </para>
    /// </remarks>
    /// 
    /// <typeparam name="TOptions">
    /// <inheritdoc/>
    /// </typeparam>
    internal class OptionsProvider<TOptions> : IOptionsProvider<TOptions>
    {
        /// <summary>
        /// The <see cref="ILogger{TCategoryName}"/> used to record operation logs.
        /// </summary>
        private readonly ILogger<OptionsProvider<TOptions>> _logger;

        /// <summary>
        /// The <see cref="IHostEnvironment"/> providing file provider and root path
        /// context.
        /// </summary>
        private readonly IHostEnvironment _hostEnvironment;

        /// <summary>
        /// The <see cref="IOptionsMonitor{TOptions}"/> used to observe current
        /// option states.
        /// </summary>
        private readonly IOptionsMonitor<TOptions> _optionsMonitor;

        /// <summary>
        /// A <see cref="JsonSerializerOptions"/> instance providing options
        /// for serializing the underlying JSON file.
        /// </summary>
        private readonly JsonSerializerOptions? _serializerOptions;

        /// <summary>
        /// A <see cref="SemaphoreSlim"/> for synchronizing file writes and
        /// state updates.
        /// </summary>
        private readonly SemaphoreSlim _semaphore;

        /// <summary>
        /// Gets the file path of the underlying JSON file where options 
        /// are persisted.
        /// </summary>
        public String Path { get; private set; }

        /// <inheritdoc/>
        TOptions IOptionsProvider<TOptions>.Current => _optionsMonitor.CurrentValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionsProvider{TOptions}"/>
        /// <see langword="class"/>.
        /// </summary>
        /// 
        /// <param name="logger">
        /// The <see cref="ILogger{TCategoryName}"/> used to record operation logs.
        /// </param>
        /// 
        /// <param name="hostEnvironment">
        /// The <see cref="IHostEnvironment"/> providing file provider and root path
        /// context.
        /// </param>
        /// 
        /// <param name="optionsMonitor">
        /// The <see cref="IOptionsMonitor{TOptions}"/> used to observe current
        /// option states.
        /// </param>
        /// 
        /// <param name="path">
        /// The file path to the JSON storage file.
        /// </param>
        public OptionsProvider(ILogger<OptionsProvider<TOptions>> logger, IHostEnvironment hostEnvironment,
            IOptionsMonitor<TOptions> optionsMonitor, String path)
        {
            _logger = logger;
            _hostEnvironment = hostEnvironment;
            _optionsMonitor = optionsMonitor;
            _semaphore = new SemaphoreSlim(1, 1);

            Path = path;
        }

        /// <inheritdoc/>
        async Task IOptionsProvider<TOptions>.UpdateAsync(Action<TOptions> configureOptions, CancellationToken cancellationToken)
        {
            // Wait for the semaphore
            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

            // Try to update the options instance and save the modified data
            try
            {
                // Get the current value
                TOptions current = _optionsMonitor.CurrentValue;
                // Apply the action to the current value
                configureOptions.Invoke(current);

                // Save the modified instance to disk
                await SaveChangesInternalAsync(current, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                // Release the semaphore
                _semaphore.Release();
            }
        }

        /// <inheritdoc/>
        async Task IOptionsProvider<TOptions>.SaveChangesAsync(CancellationToken cancellationToken)
        {
            // Wait for the semaphore
            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

            // Try to save the options instance
            try
            {
                // Save the current value to disk
                await SaveChangesInternalAsync(_optionsMonitor.CurrentValue, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                // Release the semaphore
                _semaphore.Release();
            }
        }

        /// <summary>
        /// Saves the <paramref name="value"/> to the underlying storage provider.
        /// </summary>
        /// 
        /// <param name="value">
        /// The value to persist to disk.
        /// </param>
        /// 
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> to observe while waiting for the task to complete.
        /// </param>
        /// 
        /// <returns>
        /// A <see cref="Task"/> that represents the asynchronous save operation.
        /// /// </returns>
        /// 
        /// <exception cref="InvalidOperationException"></exception>
        async Task SaveChangesInternalAsync(TOptions value, CancellationToken cancellationToken)
        {
            // Get the content root file provider
            IFileProvider storageProvider = _hostEnvironment.ContentRootFileProvider;

            // Get the configuration file at the path
            IFileInfo fileInfo = storageProvider.GetFileInfo(Path);

            // If the configuration file does not exist
            if (fileInfo.Exists is false)
            {

            }

            // Get the physical path of the configuration file
            String physicalPath = fileInfo.PhysicalPath switch
            {
                // If the physical path is not null
                String path => path,
                // If the physical path is null, throw InvalidOperationException
                null => throw new InvalidOperationException("The configuration file does not have a physical path on disk. It may be embedded, virtual, or missing."),
            };

            // Get a path to a temporary file
            String temporaryPath = String.Join('.', physicalPath, "tmp");

            // Open a file stream for the configuration file
            using (FileStream stream = new(temporaryPath, FileMode.Create, FileAccess.Write))
            {
                // Serialize the current value
                await JsonSerializer.SerializeAsync<TOptions>(stream, value, _serializerOptions, cancellationToken).ConfigureAwait(false);

                // Flush the file stream
                await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
            }

            // Overwrite the physical path with the temporary path atomically
            File.Move(temporaryPath, physicalPath, overwrite: true);
        }

        /// <inheritdoc/>
        IDisposable IObservable<TOptions>.Subscribe(IObserver<TOptions> observer)
        {
            // Subscribe to monitor changes and push to observer
            IDisposable? disposable = _optionsMonitor.OnChange((TOptions newValue) =>
            {
                try
                {
                    observer.OnNext(newValue);
                }
                catch (Exception exception)
                {
                    observer.OnError(exception);
                }
            });

            // Return the disposable
            return disposable switch
            {
                IDisposable value => value,
                _ => throw new InvalidOperationException("Could not get disposable"),
            }; 
        }
    }
}