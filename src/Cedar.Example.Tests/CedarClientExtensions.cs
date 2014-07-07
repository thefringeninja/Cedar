// ReSharper disable once CheckNamespace
namespace Cedar.Client
{
    using System;
    using System.Threading.Tasks;

    public static class CedarClientExtensions
    {
        public static Task ExecuteCommand(this CedarClient client, object command, Guid commandId)
        {
            return client.ExecuteCommand("cedar", command, commandId);
        }
    }
}