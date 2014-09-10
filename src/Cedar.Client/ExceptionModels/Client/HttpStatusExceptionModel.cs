namespace Cedar.ExceptionModels.Client
{
    using System.Net;

    public class HttpStatusExceptionModel : ExceptionModel
    {
        public HttpStatusCode StatusCode { get; set; }
    }
}