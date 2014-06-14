namespace Cedar.Example.CommandVersioning
{
    using System;
    using System.Linq;

    public class CreateTShirtV4
    {
        public String Name { get; set; }

        public string[] Sizes { get; set; }

        public string[] Colors { get; set; }

        public string BlankType { get; set; }

        public override string ToString()
        {
            return string.Format(
                "Creating a T Shirt {0} of type {1} in sizes {2} and colors {3}",
                Name,
                BlankType,
                String.Join(", ", Sizes ?? Enumerable.Empty<string>()),
                String.Join(", ", Colors ?? Enumerable.Empty<string>()));
        }
    }
}