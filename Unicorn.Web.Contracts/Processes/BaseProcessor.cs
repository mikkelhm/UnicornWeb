using System;
using System.Threading;
using System.Threading.Tasks;

namespace Unicorn.Web.Contracts.Processes
{
    public abstract class BaseProcessor : IProcessor, IDisposable
    {
        private const int StateNotStarted = 0;
        private const int StateStarting = 1;
        private const int StateStarted = 2;
        private const int StateStoppingOrStopped = 3;

        private readonly CancellationTokenSource _shutdownTokenSource;
        private readonly CancellationTokenSource _stoppingTokenSource;

        private int _state;
        private Task _stopTask;
        private object _stopTaskLock = new object();
        private bool _disposed;

        protected BaseProcessor()
        {
            _shutdownTokenSource = new CancellationTokenSource();
            _stoppingTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_shutdownTokenSource.Token);
        }

        public void Start()
        {
            StartAsync().GetAwaiter().GetResult();
        }

        public void Stop()
        {
            StopAsync().GetAwaiter().GetResult();
        }

        public Task StartAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfDisposed();

            if (Interlocked.CompareExchange(ref _state, StateStarting, StateNotStarted) != StateNotStarted)
            {
                throw new InvalidOperationException("Start has already been called.");
            }
            return StartAsyncCore(cancellationToken);
        }

        public abstract Task StartAsyncCore(CancellationToken cancellationToken);

        public Task StopAsync()
        {
            ThrowIfDisposed();

            Interlocked.CompareExchange(ref _state, StateStoppingOrStopped, StateStarted);

            if (_state != StateStoppingOrStopped)
            {
                throw new InvalidOperationException("The process has not yet started.");
            }

            // Multiple threads may call StopAsync concurrently. Both need to return the same task instance.
            lock (_stopTaskLock)
            {
                if (_stopTask == null)
                {
                    _stoppingTokenSource.Cancel();
                    _stopTask = StopAsyncCore(CancellationToken.None);
                }
            }

            return _stopTask;
        }

        public abstract Task StopAsyncCore(CancellationToken cancellationToken);

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                _shutdownTokenSource.Cancel();
                _stoppingTokenSource.Dispose();

                DisposeTheRest();

                _disposed = true;
            }
        }

        public virtual void DisposeTheRest()
        { }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(null);
            }
        }
    }
}
