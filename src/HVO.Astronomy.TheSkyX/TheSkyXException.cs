using System;
using System.Runtime.Serialization;

namespace HVO.Astronomy.TheSkyX
{
    [Serializable]
    public class TheSkyXException : Exception
    {
        public TheSkyXException(string message) : base(message)
        {
        }

        public TheSkyXException(string message, int errorCode) : this(message)
        {
            this.ErrorCode = errorCode;
        }

        public int ErrorCode
        {
            get; private set;
        } = -1;

        public TheSkyXException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected TheSkyXException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}