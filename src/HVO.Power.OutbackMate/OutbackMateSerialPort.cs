using System;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;

namespace HVO.Power.OutbackMate
{
    /// <summary>
    /// Reads CSV records from an Outback Mate controller via a serial port.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The Mate controller outputs one CSV record per line at 19200 baud, 8N1.
    /// Use <see cref="StartReadingAsync"/> to begin reading records from the serial port.
    /// Each line is raised via the <see cref="RecordReceived"/> event.
    /// </para>
    /// <para>
    /// Call <see cref="StopReadingAsync"/> or dispose the instance to stop reading.
    /// </para>
    /// </remarks>
    public sealed class OutbackMateSerialPort : IDisposable
    {
        private readonly string _portName;
        private SerialPort? _serialPort;
        private CancellationTokenSource? _readCts;
        private Task? _readTask;
        private bool _disposed;

        /// <summary>
        /// Occurs when a valid CSV record line is received from the serial port.
        /// </summary>
        public event EventHandler<OutbackMateRecordReceivedEventArgs>? RecordReceived;

        /// <summary>
        /// Occurs when a communications error is encountered.
        /// </summary>
        public event EventHandler<OutbackMateCommunicationsErrorEventArgs>? CommunicationsError;

        /// <summary>
        /// Initializes a new instance of the <see cref="OutbackMateSerialPort"/> class.
        /// </summary>
        /// <param name="portName">
        /// The name of the serial port (e.g., "COM1" on Windows, "/dev/ttyUSB0" on Linux).
        /// </param>
        /// <exception cref="ArgumentNullException"><paramref name="portName"/> is <c>null</c> or empty.</exception>
        public OutbackMateSerialPort(string portName)
        {
            if (string.IsNullOrWhiteSpace(portName))
            {
                throw new ArgumentNullException(nameof(portName));
            }

            _portName = portName;
        }

        /// <summary>
        /// Gets a value indicating whether the serial port is currently open and reading.
        /// </summary>
        public bool IsOpen
        {
            get
            {
                ThrowIfDisposed();
                return _serialPort != null && _serialPort.IsOpen;
            }
        }

        /// <summary>
        /// Opens the serial port and starts reading records asynchronously.
        /// </summary>
        /// <returns>A task that completes once the read loop has started.</returns>
        /// <exception cref="ObjectDisposedException">The instance has been disposed.</exception>
        /// <exception cref="InvalidOperationException">The serial port is already open.</exception>
        public Task StartReadingAsync()
        {
            ThrowIfDisposed();

            if (IsOpen)
            {
                throw new InvalidOperationException("The serial port is already open.");
            }

            _serialPort = new SerialPort(_portName, 19200, Parity.None, 8, StopBits.One)
            {
                DtrEnable = true,
                RtsEnable = false,
                ReadTimeout = 2000
            };

            _serialPort.Open();

            _readCts = new CancellationTokenSource();
            _readTask = Task.Run(() => ReadLoopAsync(_readCts.Token));

            return Task.CompletedTask;
        }

        /// <summary>
        /// Signals the read loop to stop and waits for it to exit.
        /// </summary>
        /// <param name="timeoutMs">
        /// Maximum time in milliseconds to wait for the read loop to exit (default: 2000).
        /// </param>
        /// <returns>A task that completes when the read loop has exited.</returns>
        public async Task StopReadingAsync(int timeoutMs = 2000)
        {
            if (_readCts != null)
            {
                _readCts.Cancel();

                // Close the serial port first so any blocking ReadLine() call
                // is unblocked immediately, rather than waiting for the timeout.
                CloseSerialPort();

                if (_readTask != null)
                {
                    // Wait for the read loop to exit, with a timeout.
                    await Task.WhenAny(_readTask, Task.Delay(timeoutMs)).ConfigureAwait(false);
                }

                _readCts.Dispose();
                _readCts = null;
                _readTask = null;
            }
            else
            {
                CloseSerialPort();
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;

                _readCts?.Cancel();
                CloseSerialPort();

                _readCts?.Dispose();
                _readCts = null;
                _readTask = null;
            }
        }

        private async Task ReadLoopAsync(CancellationToken cancellationToken)
        {
            bool fatalError = false;

            while (!cancellationToken.IsCancellationRequested && !fatalError)
            {
                try
                {
                    // SerialPort.ReadLine() is a blocking call. Running on a thread pool
                    // thread via Task.Run ensures we don't block the caller's context.
                    string? rawRecord = _serialPort?.ReadLine();
                    if (!string.IsNullOrWhiteSpace(rawRecord))
                    {
                        RecordReceived?.Invoke(this,
                            new OutbackMateRecordReceivedEventArgs(DateTimeOffset.Now, rawRecord!));
                    }
                }
                catch (IOException ex)
                {
                    // Usually caused because the port was closed externally.
                    // If cancellation or disposal is in progress, treat this as normal shutdown.
                    if (cancellationToken.IsCancellationRequested || _disposed || _serialPort == null || !_serialPort.IsOpen)
                    {
                        break;
                    }

                    OnCommunicationsError(ex);
                    fatalError = true;
                }
                catch (TimeoutException)
                {
                    // Read timed out due to ReadTimeout — check cancellation and continue.
                }
                catch (OperationCanceledException)
                {
                    // Cancellation requested — exit gracefully.
                    break;
                }
                catch (Exception ex)
                {
                    OnCommunicationsError(ex);
                    fatalError = true;
                }
            }

            // Only yield once for cooperative cancellation awareness.
            await Task.CompletedTask.ConfigureAwait(false);
        }

        private void OnCommunicationsError(Exception exception)
        {
            CommunicationsError?.Invoke(this,
                new OutbackMateCommunicationsErrorEventArgs(exception));
        }

        private void CloseSerialPort()
        {
            if (_serialPort != null)
            {
                try
                {
                    if (_serialPort.IsOpen)
                    {
                        _serialPort.Close();
                    }

                    _serialPort.Dispose();
                }
                finally
                {
                    _serialPort = null;
                }
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }
    }
}
