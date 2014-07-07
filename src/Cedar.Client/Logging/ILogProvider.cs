// https://github.com/damianh/DH.Logging

namespace Cedar.Logging
{
    public interface ILogProvider
    {
        ILog GetLogger(string name);
    }
}