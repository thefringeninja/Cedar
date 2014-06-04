// https://github.com/damianh/DH.Logging

namespace CED.Logging
{
    public interface ILogProvider
    {
        ILog GetLogger(string name);
    }
}