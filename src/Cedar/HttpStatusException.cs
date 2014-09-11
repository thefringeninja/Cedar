namespace Cedar
{
    using System;
    using System.Net;

    internal class HttpStatusException : Exception
    {
        private readonly HttpStatusCode _statusCode;

        public HttpStatusException(string message, int statusCode, Exception innerException = null)
            : this(message, (HttpStatusCode)statusCode, innerException)
        {

        }

        public HttpStatusException(string message, HttpStatusCode statusCode, Exception innerException = null)
            : base(message, innerException)
        {
            _statusCode = statusCode;
        }

        public HttpStatusCode StatusCode
        {
            get { return _statusCode; }
        }
    }
}