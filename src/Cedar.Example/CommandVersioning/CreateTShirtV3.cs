namespace Cedar.Example.CommandVersioning
{
    using System;
    using System.Linq;

    public class CreateTShirtV3
    {
        public String Name { get; set; }

        public string[] Sizes { get; set; }

        public string[] Colors { get; set; }

        public override string ToString()
        {
            return string.Format(
                "Creating a T Shirt {0} in sizes {1} and colors {2}",
                Name,
                String.Join(", ", Sizes ?? Enumerable.Empty<string>()),
                String.Join(", ", Colors ?? Enumerable.Empty<string>()));
        }
    }
}