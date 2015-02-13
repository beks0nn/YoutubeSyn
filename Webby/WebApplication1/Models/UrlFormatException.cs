using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class UrlFormatException : Exception
    {
        public UrlFormatException(){}
        public UrlFormatException(string message): base(message){}
    }
}