namespace Cedar.Testing
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class MessageEqualityComparer : IEqualityComparer<object>
    {
        private static bool ReflectionEquals(object x, object y)
        {
            if (ReferenceEquals(x, y))
                return true;

            if (ReferenceEquals(x, null))
                return false;

            if (ReferenceEquals(y, null))
                return false;

            var type = x.GetType();

            if (type != y.GetType())
                return false;

            if (x == y)
                return true;

            if (type.IsValueType)
                return x.Equals(y);
            
            if(type == typeof(string))
            {
                return x.Equals(y);
            }

            if(typeof(IEnumerable).IsAssignableFrom(type))
            {
                return ((IEnumerable) x).OfType<object>()
                    .SequenceEqual(((IEnumerable) y).OfType<object>(), Instance);
            }

            var fieldValues = from field in type.GetFields()
                select new
                {
                    member = (MemberInfo)field,
                    x = field.GetValue(x),
                    y = field.GetValue(y)
                };

            var propertyValues = from property in type.GetProperties()
                select new
                {
                    member = (MemberInfo)property,
                    x = property.GetValue(x),
                    y = property.GetValue(y)
                };

            var values = fieldValues.Concat(propertyValues);

            var differences = (from value in values
                where false == ReflectionEquals(value.x, value.y)
                select value).ToList();

            return false == differences.Any();
        }

        new public bool Equals(Object x, Object y)
        {
            return ReflectionEquals(x, y);
        }

        public int GetHashCode(Object obj)
        {
            return 0;
        }

        public static readonly MessageEqualityComparer Instance = new MessageEqualityComparer();
    }
}