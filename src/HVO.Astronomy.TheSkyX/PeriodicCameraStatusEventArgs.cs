using System;

namespace HVO.Astronomy.TheSkyX
{
    public class PeriodicCameraStatusEventArgs : EventArgs
    {
        public PeriodicCameraStatusEventArgs(DateTimeOffset eventDateTime, bool isConnected, HardwareInstanceType hardwareInstanceType)
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

        public string? Status
        {
            get; set;
        }

        public int? State
        {
            get; set;
        } = null;

        public int? BinX
        {
            get; set;
        } = null;

        public int? BinY
        {
            get; set;
        } = null;

        public _TEC? TEC
        {
            get; set;
        }

        public class _TEC
        {
            internal _TEC(bool isAvailable, bool isRegulated, double temperature, double setpoint, int power)
            {
                this.IsAvailable = isAvailable;
                this.IsRegulated = isRegulated;
                this.Temperature = temperature;
                this.Setpoint = setpoint;
                this.Power = power;
            }

            public bool IsAvailable
            {
                get;
            }

            public bool IsRegulated
            {
                get;
            }

            public double Temperature
            {
                get;
            }

            public double Setpoint
            {
                get;
            }

            public int Power
            {
                get;
            }
        }
    }
}
