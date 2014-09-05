namespace Cedar.Testing.Printing
{
    using System;

    internal class DisposableAction : IDisposable
    {
        private readonly Action _action;
        private bool _isDisposed;

        public DisposableAction(Action action)
        {
            _action = action;
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }
            _isDisposed = true;
            _action();
        }
    }
}