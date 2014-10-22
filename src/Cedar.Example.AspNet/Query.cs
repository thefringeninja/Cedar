namespace Cedar.Example.AspNet
{
    using System;

    public class Query
    {
        public class Response
        {
            public Response()
            {
                Message = DateTime.Now.ToLongTimeString();
            }

            public string Message { get; set; }
        }
    }
}