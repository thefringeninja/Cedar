namespace Cedar.Testing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using KellermanSoftware.CompareNetObjects;

    public class MessageEqualityComparer : IEqualityComparer<object>
    {

        public static readonly MessageEqualityComparer Instance = new MessageEqualityComparer();
        private readonly CompareLogic _compareLogic;

        public MessageEqualityComparer()
        {
            _compareLogic = new CompareLogic {Config = {TreatStringEmptyAndNullTheSame = true, MaxDifferences = 50}};
        }

        new public bool Equals(object x, object y)
        {

            var result = _compareLogic.Compare(x, y);

            if(result.ExceededDifferences)
            {
                var type = x == null ? null : x.GetType();
                Console.WriteLine("Warning while comparing objects of type {1} exceeded maximum number of {0}", _compareLogic.Config.MaxDifferences, type);
            }

            FilterResults(result);
            if(!result.AreEqual)
            {

                var type = x == null ? null:x.GetType();
                Console.WriteLine("Found differences while comparing objects of type: \r\n\t\t - {0} " + string.Join("\r\n\t\t - ", result.Differences.Select(dif => dif.ToString())), type);
            }

            return result.AreEqual;
        }

        private void FilterResults(ComparisonResult result)
        {
            foreach(var difference in result.Differences.ToList())
            {
                if (difference.Object1TypeName == difference.Object2TypeName && difference.Object1TypeName == typeof(DateTime).Name)
                {
                    if (difference.Object1Value == Any.Date.ToString() || difference.Object2Value == Any.Date.ToString())
                    {
                        result.Differences.Remove(difference);
                    }
                }
                if (difference.Object1TypeName == difference.Object2TypeName && difference.Object1TypeName == typeof(Guid).Name)
                {
                    if (difference.Object2Value == Any.Guid.ToString() || difference.Object2Value == Any.Guid.ToString())
                    {
                        result.Differences.Remove(difference);
                    }
                }
            }
        }

        public int GetHashCode(object obj)
        {
            return 0;
        }
    }
}