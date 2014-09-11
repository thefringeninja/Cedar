namespace Cedar.TypeResolution
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http.Headers;

    public static class RequestTypeMappingStrategies
    {
        public static VersionedName FromTypeName(Type type)
        {
            var version = 1;
            var typeName = type.Name;
            var pieces =
                new string(typeName.Reverse().ToArray()).Split(new[] { '_' }, 2)
                    .Select(s => new string(s.Reverse().ToArray()))
                    .Reverse()
                    .ToArray();

            var name = pieces[0];

            if (pieces.Length > 1)
            {
                version = Int32.Parse(pieces[1].Replace("v", String.Empty));
            }

            return new VersionedName(name, version);
        }

        public static IEnumerable<string> FromContentType(this IRequest request)
        {
            return request.Headers["Content-Type"];
        }

        public static IEnumerable<string> FromPath(this IRequest request)
        {
            var result = request.Uri.AbsolutePath.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
            if (false == result.Any())
                yield break;
            yield return result.Last();
        }

        public static IEnumerable<string> FromAcceptHeader(this IRequest request)
        {
            return from value in request.Headers["Accept"]
                   let range = MediaTypeWithQualityHeaderValue.Parse(value)
                   orderby range.Quality descending
                   select range.MediaType;
        }
    }
}