using System;

namespace HVO.Power.OutbackMate
{
    /// <summary>
    /// Provides data for the <see cref="OutbackMateSerialPort.RecordReceived"/> event.
    /// </summary>
    public sealed class OutbackMateRecordReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OutbackMateRecordReceivedEventArgs"/> class.
        /// </summary>
        /// <param name="recordDateTime">The date and time the record was received.</param>
        /// <param name="dataRecord">The raw CSV data received from the Mate controller.</param>
        public OutbackMateRecordReceivedEventArgs(DateTimeOffset recordDateTime, string dataRecord)
        {
            RecordDateTime = recordDateTime;
            DataRecord = dataRecord;
        }

        /// <summary>Gets the date and time the record was received.</summary>
        public DateTimeOffset RecordDateTime { get; }

        /// <summary>Gets the raw CSV data received from the Mate controller.</summary>
        public string DataRecord { get; }
    }
}
