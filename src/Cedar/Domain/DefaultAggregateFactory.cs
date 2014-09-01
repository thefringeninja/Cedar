namespace Cedar.Domain
{
    using System;
    using System.Reflection;
    using Cedar.Domain.Persistence;

    /// <summary>
    /// Can construct aggregates that have a public constructor that take the aggregate Id as a string.
    /// </summary>
    public class DefaultAggregateFactory : IAggregateFactory
    {
        public IAggregate Build(Type type, string id)
        {
            ConstructorInfo constructor = type.GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Instance, null, new[] {typeof (string) }, null);

            return constructor.Invoke(new object[] {id}) as IAggregate;
        }
    }
}