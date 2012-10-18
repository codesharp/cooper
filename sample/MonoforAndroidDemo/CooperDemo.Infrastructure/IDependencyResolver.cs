using System;
using System.Reflection;

namespace CooperDemo.Infrastructure
{
    /// <summary>
    /// 对象依赖解释器接口定义
    /// </summary>
    public interface IDependencyResolver
    {
        /// <summary>
        /// 注册指定类型
        /// <remarks>
        /// 将类型以及该类型的所有接口注册到容器
        /// </remarks>
        /// </summary>
        void RegisterType(Type type);
        /// <summary>
        /// 将指定程序集中符合条件的类型以及类型的所有接口注册到容器
        /// </summary>
        /// <param name="typePredicate"></param>
        /// <param name="assemblies"></param>
        void RegisterTypes(Func<Type, bool> typePredicate, params Assembly[] assemblies);
        /// <summary>
        /// 注册指定接口的实例
        /// </summary>
        void Register<T>(T instance, LifeStyle life = LifeStyle.Singleton) where T : class;
        /// <summary>
        /// 返回给定类型是否已在容器中注册
        /// </summary>
        bool IsTypeRegistered(Type type);
        /// <summary>
        /// 解释指定类型
        /// </summary>
        T Resolve<T>() where T : class;
        /// <summary>
        /// 解释指定类型
        /// </summary>
        object Resolve(Type type);
    }
}

