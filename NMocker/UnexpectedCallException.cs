using System;

namespace NMocker
{
    public class UnexpectedCallException : Exception
    {
        public UnexpectedCallException(string message) : base(message)
        {
        }
    }
}
