namespace Cedar.Domain
{
    using System;
    using System.Reflection;
    using Cedar.Domain.Persistence;

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