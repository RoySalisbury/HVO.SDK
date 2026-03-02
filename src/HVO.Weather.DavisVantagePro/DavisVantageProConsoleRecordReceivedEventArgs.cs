using System;

namespace HVO.Weather.DavisVantagePro
{
    /// <summary>
    /// Provides data for the <see cref="DavisVantageProWeatherLinkIP.ConsoleRecordReceived"/> event.
    /// </summary>
    public sealed class DavisVantageProConsoleRecordReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DavisVantageProConsoleRecordReceivedEventArgs"/> class.
        /// </summary>
        /// <param name="recordDateTime">The date and time the record was received.</param>
        /// <param name="consoleRecord">The raw 99-byte LOOP packet data.</param>
        public DavisVantageProConsoleRecordReceivedEventArgs(DateTimeOffset recordDateTime, byte[] consoleRecord)
        {
            RecordDateTime = recordDateTime;
            ConsoleRecord = (byte[])consoleRecord.Clone();
        }

        /// <summary>Gets the date and time the console record was received.</summary>
        public DateTimeOffset RecordDateTime { get; }

        /// <summary>Gets the raw 99-byte LOOP packet data.</summary>
        public byte[] ConsoleRecord { get; }
    }
}
