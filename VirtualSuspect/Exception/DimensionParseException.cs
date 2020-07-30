using System;
using System.Runtime.Serialization;

namespace VirtualSuspect.Exception
{
    [Serializable]
    internal class DimensionParseException : System.Exception
    {
        public DimensionParseException()
        {
        }

        public DimensionParseException(string message) : base(message)
        {
        }

        public DimensionParseException(string message, System.Exception innerException) : base(message, innerException)
        {
        }

        protected DimensionParseException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}