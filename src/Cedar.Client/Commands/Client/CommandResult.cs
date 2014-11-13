namespace Cedar.Commands.Client
{
    using System;

    public class CommandResult
    {
        public Guid CommandId { get; set; }
        public bool HandlersCompleted { get; set; }
    }
}