using System;
using CooperDemo.Infrastructure;

namespace CooperDemo
{
    [Component(LifeStyle=LifeStyle.Singleton)]
    public class AndroidLoggerFactory : ILoggerFactory
    {
        ILogger ILoggerFactory.Create(string name)
        {
            return new AndroidLogger(name);
        }
        ILogger ILoggerFactory.Create(Type type)
        {
            return new AndroidLogger(type.Name);
        }
    }
}

