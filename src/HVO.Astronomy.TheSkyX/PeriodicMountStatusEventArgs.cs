using HVO.Astronomy;
using System;

namespace HVO.Astronomy.TheSkyX
{
    public class PeriodicMountStatusEventArgs : EventArgs
    {
        public PeriodicMountStatusEventArgs(DateTimeOffset eventDateTime, bool isConnected)
        {
            this.EventDateTime = eventDateTime;
            this.IsConnected = isConnected;
        }

        public DateTimeOffset EventDateTime
        {
            get; private set;
        }

        public bool IsConnected
        {
            get; private set;
        }

        public bool? IsParked
        {
            get; set;
        } = null;

        public bool? IsTracking
        {
            get; set;
        } = null;

        public bool? IsSlewComplete
        {
            get; set;
        } = null;

        public RightAscension? RightAscension
        {
            get; set;
        }

        public double? Declination
        {
            get; set;
        } = null;

        public double? Altitude
        {
            get; set;
        } = null;

        public double? Azimuth
        {
            get; set;
        } = null;
    }
}