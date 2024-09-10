using System;

namespace nmocker
{
    public class UnexpectedCallException : Exception
    {
        public UnexpectedCallException(string message) : base(message)
        {
        }
    }
}
