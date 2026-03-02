using System;

namespace HVO.Power.OutbackMate
{
    /// <summary>
    /// Represents a parsed record from an Outback Mate controller.
    /// </summary>
    public interface IOutbackMateRecord
    {
        /// <summary>
        /// Gets the type of Outback device that produced this record.
        /// </summary>
        OutbackMateRecordType RecordType { get; }

        /// <summary>
        /// Gets the date and time the record was received.
        /// </summary>
        DateTimeOffset RecordDateTime { get; }

        /// <summary>
        /// Gets the hub port number (0-based) identifying which device on the hub produced this record.
        /// </summary>
        byte HubPort { get; }

        /// <summary>
        /// Gets the original raw CSV data from which this record was parsed.
        /// </summary>
        string RawData { get; }
    }
}
