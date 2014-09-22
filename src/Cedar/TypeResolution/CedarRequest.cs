namespace Cedar.TypeResolution
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Cedar.Commands;
    using Microsoft.Owin;

    internal class CedarRequest : IRequest
    {
        private readonly Uri _uri;
        private readonly ILookup<string, string> _headers;
        private Stream _body;

        public CedarRequest(IDictionary<string, object> env)
            :this(new OwinContext(env))
        {
            
        }
        public CedarRequest(IOwinContext context)
        {
            Guard.EnsureNotNull(context, "context");

            _uri = context.Request.Uri;
            _headers = (from pair in context.Request.Headers
                from value in pair.Value
                select new {header = pair.Key, value}).ToLookup(x => x.header, x => x.value, StringComparer.InvariantCultureIgnoreCase);
            _body = context.Request.Body;
        }

        public Uri Uri
        {
            get { return _uri; }
        }

        public ILookup<string, string> Headers
        {
            get { return _headers; }
        }

        public Stream Body
        {
            get { return _body; }
        }
    }
}