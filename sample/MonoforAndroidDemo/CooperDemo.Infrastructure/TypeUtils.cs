using System;
using System.Linq;

namespace CooperDemo.Infrastructure
{
    /// <summary>
    /// �����࣬�ṩ���ֹ��߷���
    /// </summary>
    public sealed class TypeUtils
    {
        /// <summary>�ж��Ƿ���Repository
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
        /// <summary>�ж��Ƿ���Service
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
        /// <summary>�ж��Ƿ���ComponentAttribute����
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

