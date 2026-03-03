using System;

namespace HVO.Astronomy.TheSkyX
{
    public class PeriodicFocuserStatusEventArgs : EventArgs
    {
        public PeriodicFocuserStatusEventArgs(DateTimeOffset eventDateTime, bool isConnected, HardwareInstanceType hardwareInstanceType)
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

        public int? Position
        {
            get; set;
        } = null;

        public double? TemperatureC
        {
            get; set;
        } = null;
    }
}