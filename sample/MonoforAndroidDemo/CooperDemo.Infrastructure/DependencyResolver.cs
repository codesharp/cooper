using System;
using System.Reflection;

namespace CooperDemo.Infrastructure
{
    /// <summary>
    /// ����ڲ�ʹ�õ�����������
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
        /// ע��ָ������
        /// <remarks>
        /// �������Լ������͵����нӿ�ע�ᵽ����
        /// </remarks>
        /// </summary>
        public static void RegisterType(Type type)
        {
            _resolver.RegisterType(type);
        }
        /// <summary>
        /// ��ָ�������з��������������Լ����͵����нӿ�ע�ᵽ����
        /// </summary>
        /// <param name="typePredicate"></param>
        /// <param name="assemblies"></param>
        public static void RegisterTypes(Func<Type, bool> typePredicate, params Assembly[] assemblies)
        {
            _resolver.RegisterTypes(typePredicate, assemblies);
        }
        /// <summary>
        /// ע��ָ���ӿڵ�ʵ��
        /// </summary>
        public static void Register<T>(T instance, LifeStyle life = LifeStyle.Singleton) where T : class
        {
            _resolver.Register<T>(instance, life);
        }
        /// <summary>
        /// ���ظ��������Ƿ�����������ע��
        /// </summary>
        public static bool IsTypeRegistered(Type type)
        {
            return _resolver.IsTypeRegistered(type);
        }
        /// <summary>
        /// ����ָ������
        /// </summary>
        public static T Resolve<T>()
        {
            return (T)_resolver.Resolve(typeof(T));
        }
        /// <summary>
        /// ����ָ������
        /// </summary>
        public static T Resolve<T>(Type type)
        {
            return (T)_resolver.Resolve(type);
        }
        /// <summary>
        /// ����ָ������
        /// </summary>
        public static object Resolve(Type type)
        {
            return _resolver.Resolve(type);
        }
    }
}

