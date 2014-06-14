namespace Cedar.Example.CommandVersioning
{
    using System;
    using System.Linq;

    public class CreateTShirtV2
    {
        public String Name { get; set; }

        public string[] Sizes { get; set; }

        public override string ToString()
        {
            return string.Format(
                "Creating a T Shirt {0} in sizes {1}",
                Name,
                String.Join(", ", Sizes ?? Enumerable.Empty<string>()));
        }
    }
}