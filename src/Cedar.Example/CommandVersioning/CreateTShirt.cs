namespace Cedar.Example.CommandVersioning
{
    using System;

    public class CreateTShirt
    {
        public String Name { get; set; }

        public string Size { get; set; }

        public override string ToString()
        {
            return string.Format("Creating a T Shirt {0} in size {1}", Name, Size);
        }
    }
}
