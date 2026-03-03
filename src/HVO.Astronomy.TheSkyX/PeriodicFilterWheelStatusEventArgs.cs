using System;

namespace HVO.Astronomy.TheSkyX
{
    public class PeriodicFilterWheelStatusEventArgs : EventArgs
    {
        public PeriodicFilterWheelStatusEventArgs(DateTimeOffset eventDateTime, bool isConnected, int currentIndex, HardwareInstanceType hardwareInstanceType)
        {
            this.EventDateTime = eventDateTime;
            this.IsConnected = isConnected;
            this.HardwareInstanceType = hardwareInstanceType;
            this.CurrentIndex = currentIndex;
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

        public int CurrentIndex
        {
            get; private set;
        }
    }
}