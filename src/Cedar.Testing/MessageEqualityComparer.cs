namespace Cedar.Testing
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using KellermanSoftware.CompareNetObjects;

    public class Any
    {
        public static readonly DateTime Date = DateTime.MaxValue.AddSeconds(-60);
        public static readonly Guid Guid = Guid.NewGuid();
    }

    public class MessageEqualityComparer : IEqualityComparer<object>
    {

        public static readonly MessageEqualityComparer Instance = new MessageEqualityComparer();
        private CompareLogic _compareLogic;

        public MessageEqualityComparer()
        {
            _compareLogic = new CompareLogic();
            _compareLogic.Config.TreatStringEmptyAndNullTheSame = true;
            _compareLogic.Config.MaxDifferences = 50;
        }

        public bool Equals(object x, object y)
        {

            var result = _compareLogic.Compare(x, y);

            if(result.ExceededDifferences)
            {
                var type = x == null ? null : x.GetType();
                Debug.WriteLine("Warning while comparing objects of type {1}exceeded maximum number of {0}", _compareLogic.Config.MaxDifferences, "ARG1");
            }

            FilterResults(result);
            if(!result.AreEqual)
            {

                var type = x == null ? null:x.GetType();
                Debug.WriteLine("Found differences while comparing objects of type: {0} " + result.DifferencesString, type);
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