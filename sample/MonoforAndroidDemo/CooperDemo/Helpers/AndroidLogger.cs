using Android.Util;
using CooperDemo.Infrastructure;

namespace CooperDemo
{
    public class AndroidLogger : ILogger
    {
        private string _tag;

        /// <summary>
        /// tag的长度不能超过23,否则会抛出异常, Android的Log不允许tag的长度超过23
        /// </summary>
        /// <param name="tag"></param>
        public AndroidLogger(string tag)
        {
            if (string.IsNullOrEmpty(tag))
            {
                _tag = "default";
            }
            else
            {
                //如果长度超过23，则强行截取前23个字符
                if (tag.Length > 23)
                {
                    _tag = tag.Substring(0, 23);
                }
                _tag = tag;
            }
        }

        bool ILogger.IsDebugEnabled
        {
            get { return Log.IsLoggable(_tag, LogPriority.Debug); }
        }
        void ILogger.Debug(object message)
        {
            if (!Log.IsLoggable(_tag, LogPriority.Debug))
            {
                return;
            }
            Log.Debug(_tag, message != null ? message.ToString() : null);
        }
        void ILogger.DebugFormat(string format, params object[] args)
        {
            if (!Log.IsLoggable(_tag, LogPriority.Debug))
            {
                return;
            }
            Log.Debug(_tag, format, args);
        }

        void ILogger.Info(object message)
        {
            if (!Log.IsLoggable(_tag, LogPriority.Info))
            {
                return;
            }
            Log.Info(_tag, message != null ? message.ToString() : null);
        }
        void ILogger.InfoFormat(string format, params object[] args)
        {
            if (!Log.IsLoggable(_tag, LogPriority.Info))
            {
                return;
            }
            Log.Info(_tag, format, args);
        }

        void ILogger.Warn(object message)
        {
            if (!Log.IsLoggable(_tag, LogPriority.Warn))
            {
                return;
            }
            Log.Warn(_tag, message != null ? message.ToString() : null);
        }
        void ILogger.WarnFormat(string format, params object[] args)
        {
            if (!Log.IsLoggable(_tag, LogPriority.Warn))
            {
                return;
            }
            Log.Warn(_tag, format, args);
        }

        void ILogger.Error(object message)
        {
            if (!Log.IsLoggable(_tag, LogPriority.Error))
            {
                return;
            }
            Log.Error(_tag, message != null ? message.ToString() : null);
        }
        void ILogger.ErrorFormat(string format, params object[] args)
        {
            if (!Log.IsLoggable(_tag, LogPriority.Error))
            {
                return;
            }
            Log.Error(_tag, format, args);
        }

        void ILogger.Fatal(object message)
        {
            Log.Wtf(_tag, message != null ? message.ToString() : null);
        }
        void ILogger.FatalFormat(string format, params object[] args)
        {
            Log.Wtf(_tag, format, args);
        }
    }
}

