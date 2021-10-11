using System;
using System.Collections.Generic;
using System.Text;

namespace CyDrive
{
    public class InvalidParameterException : Exception
    {
        public InvalidParameterException(string message) : base(message) { }
    }

    public class AuthException : Exception
    {
        public AuthException(string message) : base(message) { }
    }

    public class NeedParameterException : Exception
    {
        public NeedParameterException(string message) : base(message) { }
    }

    public class FileTooLargeException : Exception
    {
        public FileTooLargeException(string message) : base(message) { }
    }


}
