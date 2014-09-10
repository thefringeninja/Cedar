namespace Cedar.ContentNegotiation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http.Headers;
    using Microsoft.Owin;

    internal static class ContentNegotiationExtensions
    {
        internal static IEnumerable<MediaTypeWithQualityHeaderValue> ParseAcceptHeader(this IOwinRequest request)
        {
            return from value in request.Accept.Split(',')
                let range = MediaTypeWithQualityHeaderValue.Parse(value)
                orderby range.Quality descending
                select range;
        }

        public static Type FindBest(this IContentTypeMapper typeMapper, IOwinContext context)
        {
            return context.Request.ParseAcceptHeader()
                .Select(contentType => typeMapper.GetFromContentType(contentType.MediaType))
                .FirstOrDefault(type => type != null);
        }
    }
}