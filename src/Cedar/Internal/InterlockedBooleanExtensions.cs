 // ReSharper disable once CheckNamespace
namespace System.Threading
{
    public static class InterlockedBooleanExtensions
    {
        public static bool EnsureCalledOnce(this InterlockedBoolean interlockedBoolean)
        {
            return interlockedBoolean.CompareExchange(true, false);
        }
    }
}