namespace Cedar.Testing
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;

    static partial class Scenario
    {
        internal class HttpRequest
        {
            private readonly HttpRequestMessage _request;

            private readonly byte[] _body;

            internal HttpRequest(HttpRequestMessage request)
            {
                _request = request;

                if(request.Content == null)
                {
                    return;
                }

                using(var stream = new MemoryStream())
                {
                    request.Content.CopyToAsync(stream).Wait();

                    _body = stream.ToArray();
                }
            }

            public HeaderCollection Headers
            {
                get { return new HeaderCollection(_request); }
            }

            public byte[] Body()
            {
                return _body;
            }

            public static implicit operator HttpRequestMessage(HttpRequest request)
            {
                return request == null ? null : request._request;
            }

            public static implicit operator HttpRequest(HttpRequestMessage request)
            {
                return new HttpRequest(request);
            }

            public override string ToString()
            {
                var flattenedHeaders =
                    (from headers in
                        _request.Headers.Union((_request.Content ?? new StringContent(String.Empty)).Headers)
                        let values = headers.Value
                        from value in values
                        select new { key = headers.Key, value });

                var builder = new StringBuilder();

                builder.Append(_request.Method.ToString().ToUpper()).Append(' ')
                    .Append(_request.RequestUri.PathAndQuery).Append(' ')
                    .Append("HTTP/").Append(_request.Version.ToString(2))
                    .AppendLine();

                flattenedHeaders.Aggregate(builder,
                    (sb, header) => sb.Append(header.key).Append(':').Append(' ').Append(header.value).AppendLine());

                if (_body != null)
                {
                    builder.AppendLine().AppendLine();
                    builder.AppendLine(Encoding.UTF8.GetString(_body));
                }

                builder.AppendLine().AppendLine();

                return builder.ToString();
            }

            public class HeaderCollection
            {
                private readonly HttpContentHeaders _contentHeaders;
                private readonly HttpRequestHeaders _headers;

                public HeaderCollection(HttpRequestMessage request)
                {
                    _headers = request.Headers;
                    _contentHeaders = (request.Content ?? new StringContent(String.Empty)).Headers;
                }

                public HttpHeaderValueCollection<WarningHeaderValue> Warning
                {
                    get { return _headers.Warning; }
                }

                public HttpHeaderValueCollection<ViaHeaderValue> Via
                {
                    get { return _headers.Via; }
                }

                public HttpHeaderValueCollection<ProductHeaderValue> Upgrade
                {
                    get { return _headers.Upgrade; }
                }

                public bool? TransferEncodingChunked
                {
                    get { return _headers.TransferEncodingChunked; }
                }

                public HttpHeaderValueCollection<TransferCodingHeaderValue> TransferEncoding
                {
                    get { return _headers.TransferEncoding; }
                }

                public HttpHeaderValueCollection<string> Trailer
                {
                    get { return _headers.Trailer; }
                }

                public HttpHeaderValueCollection<NameValueHeaderValue> Pragma
                {
                    get { return _headers.Pragma; }
                }

                public DateTimeOffset? Date
                {
                    get { return _headers.Date; }
                }

                public bool? ConnectionClose
                {
                    get { return _headers.ConnectionClose; }
                }

                public HttpHeaderValueCollection<string> Connection
                {
                    get { return _headers.Connection; }
                }

                public CacheControlHeaderValue CacheControl
                {
                    get { return _headers.CacheControl; }
                }

                public HttpHeaderValueCollection<ProductInfoHeaderValue> UserAgent
                {
                    get { return _headers.UserAgent; }
                }

                public HttpHeaderValueCollection<TransferCodingWithQualityHeaderValue> TE
                {
                    get { return _headers.TE; }
                }

                public Uri Referrer
                {
                    get { return _headers.Referrer; }
                }

                public RangeHeaderValue Range
                {
                    get { return _headers.Range; }
                }

                public AuthenticationHeaderValue ProxyAuthorization
                {
                    get { return _headers.ProxyAuthorization; }
                }

                public int? MaxForwards
                {
                    get { return _headers.MaxForwards; }
                }

                public DateTimeOffset? IfUnmodifiedSince
                {
                    get { return _headers.IfUnmodifiedSince; }
                }

                public RangeConditionHeaderValue IfRange
                {
                    get { return _headers.IfRange; }
                }

                public HttpHeaderValueCollection<EntityTagHeaderValue> IfNoneMatch
                {
                    get { return _headers.IfNoneMatch; }
                }

                public DateTimeOffset? IfModifiedSince
                {
                    get { return _headers.IfModifiedSince; }
                }

                public HttpHeaderValueCollection<EntityTagHeaderValue> IfMatch
                {
                    get { return _headers.IfMatch; }
                }

                public string Host
                {
                    get { return _headers.Host; }
                }

                public string From
                {
                    get { return _headers.From; }
                }

                public bool? ExpectContinue
                {
                    get { return _headers.ExpectContinue; }
                }

                public HttpHeaderValueCollection<NameValueWithParametersHeaderValue> Expect
                {
                    get { return _headers.Expect; }
                }

                public AuthenticationHeaderValue Authorization
                {
                    get { return _headers.Authorization; }
                }

                public HttpHeaderValueCollection<StringWithQualityHeaderValue> AcceptLanguage
                {
                    get { return _headers.AcceptLanguage; }
                }

                public HttpHeaderValueCollection<StringWithQualityHeaderValue> AcceptEncoding
                {
                    get { return _headers.AcceptEncoding; }
                }

                public HttpHeaderValueCollection<StringWithQualityHeaderValue> AcceptCharset
                {
                    get { return _headers.AcceptCharset; }
                }

                public HttpHeaderValueCollection<MediaTypeWithQualityHeaderValue> Accept
                {
                    get { return _headers.Accept; }
                }

                public DateTimeOffset? LastModified
                {
                    get { return _contentHeaders.LastModified; }
                }

                public DateTimeOffset? Expires
                {
                    get { return _contentHeaders.Expires; }
                }

                public MediaTypeHeaderValue ContentType
                {
                    get { return _contentHeaders.ContentType; }
                }

                public ContentRangeHeaderValue ContentRange
                {
                    get { return _contentHeaders.ContentRange; }
                }

                public byte[] ContentMD5
                {
                    get { return _contentHeaders.ContentMD5; }
                }

                public Uri ContentLocation
                {
                    get { return _contentHeaders.ContentLocation; }
                }

                public long? ContentLength
                {
                    get { return _contentHeaders.ContentLength; }
                }

                public ICollection<string> ContentLanguage
                {
                    get { return _contentHeaders.ContentLanguage; }
                }

                public ICollection<string> ContentEncoding
                {
                    get { return _contentHeaders.ContentEncoding; }
                }

                public ContentDispositionHeaderValue ContentDisposition
                {
                    get { return _contentHeaders.ContentDisposition; }
                }

                public ICollection<string> Allow
                {
                    get { return _contentHeaders.Allow; }
                }
            }
        }

        public class HttpResponse
        {
            private readonly HttpResponseMessage _response;

            private readonly byte[] _body;

            internal HttpResponse(HttpResponseMessage response)
            {
                _response = response;

                if(response.Content == null)
                {
                    return;
                }

                using(var stream = new MemoryStream())
                {
                    response.Content.CopyToAsync(stream).Wait();

                    _body = stream.ToArray();
                }
            }

            public HeaderCollection Headers
            {
                get { return new HeaderCollection(_response); }
            }

            public HttpStatusCode StatusCode
            {
                get { return _response.StatusCode;  }
            }

            public byte[] Body()
            {
                return _body;
            }

            public static implicit operator HttpResponseMessage(HttpResponse response)
            {
                return response == null ? null : response._response;
            }

            public static implicit operator HttpResponse(HttpResponseMessage response)
            {
                return new HttpResponse(response);
            }

            public override string ToString()
            {
                var flattenedHeaders =
                    (from headers in
                        _response.Headers.Union((_response.Content ?? new StringContent(String.Empty)).Headers)
                        let values = headers.Value
                        from value in values
                        select new { key = headers.Key, value });

                var builder = new StringBuilder();

                builder
                    .Append("HTTP/").Append(_response.Version.ToString(2)).Append(' ')
                    .Append((int) _response.StatusCode).Append(' ')
                    .Append(_response.ReasonPhrase)
                    .AppendLine();

                flattenedHeaders.Aggregate(builder,
                    (sb, header) => sb.Append(header.key).Append(':').Append(' ').Append(header.value).AppendLine());

                if (_body != null)
                {
                    builder.AppendLine().AppendLine();
                    builder.AppendLine(Encoding.UTF8.GetString(_body));
                }

                builder.AppendLine().AppendLine();
                
                return builder.ToString();
            }

            public class HeaderCollection
            {
                private readonly HttpContentHeaders _contentHeaders;
                private readonly HttpResponseHeaders _headers;

                public HeaderCollection(HttpResponseMessage response)
                {
                    _headers = response.Headers;
                    _contentHeaders = (response.Content ?? new StringContent(String.Empty)).Headers;
                }

                public HttpHeaderValueCollection<string> AcceptRanges
                {
                    get { return _headers.AcceptRanges; }
                }

                public TimeSpan? Age
                {
                    get { return _headers.Age; }
                }

                public EntityTagHeaderValue ETag
                {
                    get { return _headers.ETag; }
                }

                public Uri Location
                {
                    get { return _headers.Location; }
                }

                public HttpHeaderValueCollection<AuthenticationHeaderValue> ProxyAuthenticate
                {
                    get { return _headers.ProxyAuthenticate; }
                }

                public RetryConditionHeaderValue RetryAfter
                {
                    get { return _headers.RetryAfter; }
                }

                public HttpHeaderValueCollection<ProductInfoHeaderValue> Server
                {
                    get { return _headers.Server; }
                }

                public HttpHeaderValueCollection<string> Vary
                {
                    get { return _headers.Vary; }
                }

                public HttpHeaderValueCollection<AuthenticationHeaderValue> WwwAuthenticate
                {
                    get { return _headers.WwwAuthenticate; }
                }

                public CacheControlHeaderValue CacheControl
                {
                    get { return _headers.CacheControl; }
                }

                public HttpHeaderValueCollection<string> Connection
                {
                    get { return _headers.Connection; }
                }

                public bool? ConnectionClose
                {
                    get { return _headers.ConnectionClose; }
                }

                public DateTimeOffset? Date
                {
                    get { return _headers.Date; }
                }

                public HttpHeaderValueCollection<NameValueHeaderValue> Pragma
                {
                    get { return _headers.Pragma; }
                }

                public HttpHeaderValueCollection<string> Trailer
                {
                    get { return _headers.Trailer; }
                }

                public HttpHeaderValueCollection<TransferCodingHeaderValue> TransferEncoding
                {
                    get { return _headers.TransferEncoding; }
                }

                public bool? TransferEncodingChunked
                {
                    get { return _headers.TransferEncodingChunked; }
                }

                public HttpHeaderValueCollection<ProductHeaderValue> Upgrade
                {
                    get { return _headers.Upgrade; }
                }

                public HttpHeaderValueCollection<ViaHeaderValue> Via
                {
                    get { return _headers.Via; }
                }

                public HttpHeaderValueCollection<WarningHeaderValue> Warning
                {
                    get { return _headers.Warning; }
                }

                public ICollection<string> Allow
                {
                    get { return _contentHeaders.Allow; }
                }

                public ContentDispositionHeaderValue ContentDisposition
                {
                    get { return _contentHeaders.ContentDisposition; }
                }

                public ICollection<string> ContentEncoding
                {
                    get { return _contentHeaders.ContentEncoding; }
                }

                public ICollection<string> ContentLanguage
                {
                    get { return _contentHeaders.ContentLanguage; }
                }

                public long? ContentLength
                {
                    get { return _contentHeaders.ContentLength; }
                }

                public Uri ContentLocation
                {
                    get { return _contentHeaders.ContentLocation; }
                }

                public byte[] ContentMD5
                {
                    get { return _contentHeaders.ContentMD5; }
                }

                public ContentRangeHeaderValue ContentRange
                {
                    get { return _contentHeaders.ContentRange; }
                }

                public MediaTypeHeaderValue ContentType
                {
                    get { return _contentHeaders.ContentType; }
                }

                public DateTimeOffset? Expires
                {
                    get { return _contentHeaders.Expires; }
                }

                public DateTimeOffset? LastModified
                {
                    get { return _contentHeaders.LastModified; }
                }
            }
        }
    }
}