namespace Cedar.Commands
{
    using System;
    using System.Net;

    /// <summary>
    ///     An exception that represents a Problem Detail for HTTP APIs
    ///     https://datatracker.ietf.org/doc/draft-ietf-appsawg-http-problem/
    /// </summary>
    public class HttpProblemDetailsException : Exception
    {
        
        private readonly HttpProblemDetails _problemDetails;

        /// <summary>
        ///     Initializes a new instance of the <see cref="HttpProblemDetailsException"/> class.
        /// </summary>
        /// <param name="status">
        ///     The HttpStatusCode. You can set more values via the ProblemDetails property.
        /// </param>
        public HttpProblemDetailsException(HttpStatusCode status)
            : this(new HttpProblemDetails(status))
        {}


        /// <summary>
        ///     Initializes a new instance of the <see cref="HttpProblemDetailsException"/> class.
        /// </summary>
        /// <param name="problemDetails">
        ///     An instance of <see cref="ProblemDetails"/>
        /// </param>
        public HttpProblemDetailsException(HttpProblemDetails problemDetails)
            : base("An exception occured invoking the HTTP API. See ProblemDetails for more information")
        {
            if(problemDetails == null)
            {
                throw new ArgumentNullException("problemDetails");
            }
            _problemDetails = problemDetails;
        }

        public HttpProblemDetails ProblemDetails
        {
            get { return _problemDetails; }
        }
    }
}