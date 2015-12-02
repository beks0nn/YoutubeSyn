using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class InputFormatException : Exception
    {
        public InputFormatException(){}
        public InputFormatException(string message) : base(message) { }
    }
}