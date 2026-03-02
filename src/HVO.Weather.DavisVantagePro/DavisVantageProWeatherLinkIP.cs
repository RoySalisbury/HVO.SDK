using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace HVO.Weather.DavisVantagePro
{
    /// <summary>
    /// Communicates with a Davis Vantage Pro weather station over a TCP connection
    /// via a WeatherLink IP data logger.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The WeatherLink IP data logger exposes a TCP socket (default port 22222) that
    /// accepts the same serial commands as the console's serial port. This class sends
    /// LOOP commands to retrieve real-time weather data packets every ~2 seconds.
    /// </para>
    /// <para>
    /// Use <see cref="StartMonitorAsync"/> for production workloads. The synchronous
    /// <see cref="StartMonitor"/> method is retained for backward compatibility but
    /// uses a recursive restart pattern that should be avoided in new code.
    /// </para>
    /// </remarks>
    public sealed class DavisVantageProWeatherLinkIP : IDisposable
    {
        private bool _disposed;
        private readonly IPEndPoint _ipEndPoint;
        private readonly object _syncLock = new object();

        /// <summary>
        /// Occurs when a valid console data record (LOOP packet) is received.
        /// </summary>
        public event EventHandler<DavisVantageProConsoleRecordReceivedEventArgs>? ConsoleRecordReceived;

        /// <summary>
        /// Initializes a new instance of the <see cref="DavisVantageProWeatherLinkIP"/> class.
        /// </summary>
        /// <param name="ipAddress">The IP address of the WeatherLink IP device.</param>
        /// <param name="port">The TCP port number (default: 22222).</param>
        /// <exception cref="ArgumentNullException"><paramref name="ipAddress"/> is <c>null</c>.</exception>
        public DavisVantageProWeatherLinkIP(IPAddress ipAddress, int port = 22222)
        {
            if (ipAddress == null) throw new ArgumentNullException(nameof(ipAddress));
            _ipEndPoint = new IPEndPoint(ipAddress, port);
        }

        /// <summary>
        /// Starts monitoring the weather station synchronously using a background task.
        /// </summary>
        /// <remarks>
        /// This method uses a recursive restart pattern via <see cref="Task"/>.<c>ContinueWith</c>.
        /// Prefer <see cref="StartMonitorAsync"/> for new code.
        /// </remarks>
        public void StartMonitor()
        {
            var task = Task.Factory.StartNew(() =>
            {
                try
                {
                    using (var tcpClient = new TcpClient())
                    {
                        tcpClient.Connect(_ipEndPoint.Address, _ipEndPoint.Port);

                        var networkStream = tcpClient.GetStream();
                        networkStream.ReadTimeout = 5000;
                        networkStream.WriteTimeout = 5000;

                        using (var streamReader = new StreamReader(networkStream))
                        using (var streamWriter = new StreamWriter(networkStream) { AutoFlush = true })
                        {
                            if (SendConsoleWakeupCommand(streamWriter, streamReader))
                            {
                                if (GetConsoleDataRecord(networkStream, streamWriter, out var latestConsoleRecord))
                                {
                                    ConsoleRecordReceived?.Invoke(this, new DavisVantageProConsoleRecordReceivedEventArgs(latestConsoleRecord.Key, latestConsoleRecord.Value));
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // Connection failure — the ContinueWith below will restart.
                }
            }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);

            task.ContinueWith(t =>
            {
                Thread.Sleep(10000);
                StartMonitor();
            }, TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        /// <summary>
        /// Starts monitoring the weather station asynchronously with cancellation support.
        /// </summary>
        /// <param name="stoppingToken">A token to signal when monitoring should stop.</param>
        /// <returns>A task that completes when monitoring is cancelled.</returns>
        public async Task StartMonitorAsync(CancellationToken stoppingToken)
        {
            using (var tcpClient = new TcpClient { ReceiveTimeout = 1500, SendTimeout = 1500 })
            {
                await tcpClient.ConnectAsync(_ipEndPoint.Address, _ipEndPoint.Port).ConfigureAwait(false);
                var networkStream = tcpClient.GetStream();

                using (var streamReader = new StreamReader(networkStream))
                using (var streamWriter = new StreamWriter(networkStream) { AutoFlush = true })
                {
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        if (await SendConsoleWakeupCommandAsync(streamReader, streamWriter, retryAttempts: 3).ConfigureAwait(false))
                        {
                            while (!stoppingToken.IsCancellationRequested)
                            {
                                Action<DateTimeOffset, byte[]> callback = (recordDateTime, data) =>
                                {
                                    ConsoleRecordReceived?.Invoke(this, new DavisVantageProConsoleRecordReceivedEventArgs(recordDateTime, data));
                                };

                                if (!await GetConsoleDataRecordAsync(streamReader, streamWriter, callback, stoppingToken).ConfigureAwait(false))
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        private static async Task<bool> SendConsoleWakeupCommandAsync(StreamReader streamReader, StreamWriter streamWriter, int retryAttempts = 3)
        {
            int attempt = 0;
            while (retryAttempts > attempt++)
            {
                await streamWriter.WriteAsync('\n').ConfigureAwait(false);
                var result = await streamReader.ReadLineAsync().ConfigureAwait(false);

                if (result != null)
                {
                    streamReader.DiscardBufferedData();
                    return true;
                }

                await Task.Delay(1200).ConfigureAwait(false);
            }
            return false;
        }

        private static async Task<bool> GetConsoleDataRecordAsync(StreamReader streamReader, StreamWriter streamWriter, Action<DateTimeOffset, byte[]> action, CancellationToken cancellationToken, byte packetsRequested = byte.MaxValue)
        {
            await streamWriter.WriteAsync($"LOOP {packetsRequested}\n").ConfigureAwait(false);

            int ackRetryCount = 0;
            do
            {
                int byteRead;
                try
                {
                    byteRead = streamReader.BaseStream.ReadByte();
                }
                catch (IOException ex) when (ex.InnerException is SocketException socketException && socketException.SocketErrorCode == SocketError.TimedOut)
                {
                    continue;
                }

                if (byteRead == 0x06) // ACK
                {
                    for (int i = 0; i < packetsRequested; i++)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            return false;
                        }

                        var commandResponse = new byte[99];
                        int bytesRead = 0;
                        int retryCount = 0;

                        while (bytesRead < commandResponse.Length && !cancellationToken.IsCancellationRequested)
                        {
                            try
                            {
                                bytesRead += streamReader.BaseStream.Read(commandResponse, bytesRead, commandResponse.Length - bytesRead);
                            }
                            catch (IOException ex) when (ex.InnerException is SocketException socketException && socketException.SocketErrorCode == SocketError.TimedOut)
                            {
                                if (retryCount++ > 3)
                                {
                                    return false;
                                }
                            }
                        }

                        // The first record may be stale; discard it when requesting multiple packets.
                        if (i == 0 && packetsRequested > 1)
                        {
                            continue;
                        }

                        if (DavisVantageProConsoleRecord.ValidatePacketCrc(commandResponse))
                        {
                            var record = DavisVantageProConsoleRecord.Create(commandResponse, DateTimeOffset.Now, false);
                            action?.Invoke(record.RecordDateTime, record.RawDataRecord);
                        }
                    }

                    return true;
                }

                if (byteRead == 0x21) // NAK
                {
                    return false;
                }
            } while (ackRetryCount++ < 3);

            return false;
        }

        private static bool SendConsoleWakeupCommand(StreamWriter streamWriter, StreamReader streamReader)
        {
            try
            {
                streamWriter.Write("\n");
                streamReader.ReadLine();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static bool GetConsoleDataRecord(NetworkStream networkStream, StreamWriter streamWriter, out System.Collections.Generic.KeyValuePair<DateTimeOffset, byte[]> latestConsoleRecord)
        {
            int retryCount = 0;
            do
            {
                try
                {
                    streamWriter.Write(string.Format("LOOP 1{0}", '\n'));

                    int ackRetry = 0;
                    while (ackRetry < 3)
                    {
                        if (networkStream.ReadByte() == 6)
                        {
                            var commandResponse = new byte[99];
                            int totalReadCount = 0;

                            while (totalReadCount < commandResponse.Length)
                            {
                                int currentReadCount = networkStream.Read(commandResponse, totalReadCount, commandResponse.Length - totalReadCount);
                                totalReadCount += currentReadCount;

                                if (totalReadCount >= commandResponse.Length)
                                {
                                    if (DavisVantageProConsoleRecord.ValidatePacketCrc(commandResponse))
                                    {
                                        latestConsoleRecord = new System.Collections.Generic.KeyValuePair<DateTimeOffset, byte[]>(DateTimeOffset.Now, commandResponse);
                                        return true;
                                    }

                                    if (++retryCount >= 3)
                                    {
                                        latestConsoleRecord = default;
                                        return false;
                                    }
                                }
                            }
                            break;
                        }

                        ackRetry++;
                    }
                }
                catch (Exception)
                {
                    latestConsoleRecord = default;
                    return false;
                }
            } while (true);
        }

        #region IDisposable

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // No managed resources to dispose currently.
                    // Future: dispose TcpClient if held as a field.
                }
                _disposed = true;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Finalizer releases unmanaged resources.
        /// </summary>
        ~DavisVantageProWeatherLinkIP()
        {
            Dispose(false);
        }

        #endregion
    }
}
