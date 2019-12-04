using System;
using System.Runtime.Serialization;

namespace VirtualSuspect.Exception
{
    [Serializable]
    public class MessageFieldException : System.Exception
    {
        public MessageFieldException() {
        }

        public MessageFieldException(string message) : base(message) {
        }

        public MessageFieldException(string message, System.Exception innerException) : base(message, innerException) {
        }

        protected MessageFieldException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }
    }
}