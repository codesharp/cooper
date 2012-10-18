using System;
using System.Linq;

namespace CooperDemo.Infrastructure
{
    /// <summary>
    /// 工具类，提供各种工具方法
    /// </summary>
    public sealed class TypeUtils
    {
        /// <summary>判断是否是Repository
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsRepository(Type type)
        {
            return type != null
                 && type.Name.EndsWith("Repository", StringComparison.InvariantCultureIgnoreCase)
                 && !type.IsAbstract
                 && !type.IsInterface;
        }
        /// <summary>判断是否是Service
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsService(Type type)
        {
            return type != null
                 && type.Name.EndsWith("Service", StringComparison.InvariantCultureIgnoreCase)
                 && !type.IsAbstract
                 && !type.IsInterface;
        }
        /// <summary>判断是否有ComponentAttribute属性
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsComponent(Type type)
        {
            return type != null
                 && type.GetCustomAttributes(typeof(ComponentAttribute), false).Count() > 0
                 && !type.IsAbstract
                 && !type.IsInterface;
        }
    }
}

