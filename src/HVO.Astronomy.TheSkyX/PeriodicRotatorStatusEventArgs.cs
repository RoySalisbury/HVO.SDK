using System;

namespace HVO.Astronomy.TheSkyX
{
    public class PeriodicRotatorStatusEventArgs : EventArgs
    {
        public PeriodicRotatorStatusEventArgs(DateTimeOffset eventDateTime, bool isConnected, HardwareInstanceType hardwareInstanceType)
        {
            this.EventDateTime = eventDateTime;
            this.IsConnected = isConnected;
            this.HardwareInstanceType = hardwareInstanceType;
        }

        public DateTimeOffset EventDateTime
        {
            get; private set;
        }

        public bool IsConnected
        {
            get; private set;
        }

        public HardwareInstanceType HardwareInstanceType
        {
            get; private set;
        }

        public bool? IsRotating
        {
            get; set;
        } = null;

        public double? PositionAngle
        {
            get; set;
        } = null;
    }
}