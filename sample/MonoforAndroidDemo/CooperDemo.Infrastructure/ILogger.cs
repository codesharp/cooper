using System;

namespace CooperDemo.Infrastructure
{
    /// <summary>
    /// 日志记录器接口定义
    /// </summary>
    public interface ILogger
    {
        bool IsDebugEnabled { get; }
        void Debug(object message);
        void DebugFormat(string format, params object[] args);

        void Info(object message);
        void InfoFormat(string format, params object[] args);

        void Warn(object message);
        void WarnFormat(string format, params object[] args);

        void Error(object message);
        void ErrorFormat(string format, params object[] args);

        void Fatal(object message);
        void FatalFormat(string format, params object[] args);
    }
}

