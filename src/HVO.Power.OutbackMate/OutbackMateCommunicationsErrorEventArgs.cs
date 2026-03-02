using System;

namespace HVO.Power.OutbackMate
{
    /// <summary>
    /// Provides data for the <see cref="OutbackMateSerialPort.CommunicationsError"/> event.
    /// </summary>
    public sealed class OutbackMateCommunicationsErrorEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OutbackMateCommunicationsErrorEventArgs"/> class.
        /// </summary>
        /// <param name="exception">The exception that caused the communications error.</param>
        public OutbackMateCommunicationsErrorEventArgs(Exception exception)
        {
            Exception = exception;
        }

        /// <summary>Gets the exception that caused the communications error.</summary>
        public Exception Exception { get; }
    }
}
