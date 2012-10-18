using System;
using System.Reflection;

namespace CooperDemo.Infrastructure
{
    /// <summary>
    /// 框架内部使用的依赖解析器
    /// </summary>
    public static class DependencyResolver
    {
        private static IDependencyResolver _resolver;

        public static IDependencyResolver Resolver { get { return _resolver; } }

        internal static void SetResolver(IDependencyResolver resolver)
        {
            _resolver = resolver;
        }

        /// <summary>
        /// 注册指定类型
        /// <remarks>
        /// 将类型以及该类型的所有接口注册到容器
        /// </remarks>
        /// </summary>
        public static void RegisterType(Type type)
        {
            _resolver.RegisterType(type);
        }
        /// <summary>
        /// 将指定程序集中符合条件的类型以及类型的所有接口注册到容器
        /// </summary>
        /// <param name="typePredicate"></param>
        /// <param name="assemblies"></param>
        public static void RegisterTypes(Func<Type, bool> typePredicate, params Assembly[] assemblies)
        {
            _resolver.RegisterTypes(typePredicate, assemblies);
        }
        /// <summary>
        /// 注册指定接口的实例
        /// </summary>
        public static void Register<T>(T instance, LifeStyle life = LifeStyle.Singleton) where T : class
        {
            _resolver.Register<T>(instance, life);
        }
        /// <summary>
        /// 返回给定类型是否已在容器中注册
        /// </summary>
        public static bool IsTypeRegistered(Type type)
        {
            return _resolver.IsTypeRegistered(type);
        }
        /// <summary>
        /// 解释指定类型
        /// </summary>
        public static T Resolve<T>()
        {
            return (T)_resolver.Resolve(typeof(T));
        }
        /// <summary>
        /// 解释指定类型
        /// </summary>
        public static T Resolve<T>(Type type)
        {
            return (T)_resolver.Resolve(type);
        }
        /// <summary>
        /// 解释指定类型
        /// </summary>
        public static object Resolve(Type type)
        {
            return _resolver.Resolve(type);
        }
    }
}

