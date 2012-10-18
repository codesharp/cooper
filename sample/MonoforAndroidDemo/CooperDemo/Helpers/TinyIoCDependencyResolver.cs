using System;
using System.Linq;
using System.Reflection;
using CooperDemo.Infrastructure;
using TinyIoC;

namespace CooperDemo
{
    public class TinyIoCDependencyResolver : IDependencyResolver
    {
        private TinyIoCContainer _container;

        public TinyIoCDependencyResolver()
        {
            _container = TinyIoC.TinyIoCContainer.Current;
        }

        //public void RegisterAllServices()
        //{
        //    _container.Register<ITaskFolderRepository, TaskFolderRepository>().AsMultiInstance();
        //    _container.Register<ITaskFolderService, TaskFolderService>().AsMultiInstance();
        //}
    
        void IDependencyResolver.RegisterType(Type type)
        {
            //生命周期
            var life = ParseLife(type);

            //实现注册
            if (!_container.CanResolve(type))
            {
                _container.Register(type).Life(life);
            }

            //接口注册
            foreach (var interfaceType in type.GetInterfaces())
            {
                if (!_container.CanResolve(interfaceType))
                {
                    _container.Register(interfaceType, type).Life(life);
                }
            }
        }
        void IDependencyResolver.RegisterTypes(Func<Type,bool> typePredicate, params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetExportedTypes().Where(x => typePredicate(x)))
                {
                    ((IDependencyResolver)this).RegisterType(type);
                }
            }
        }
        void IDependencyResolver.Register<T>(T instance, LifeStyle life)
        {
            _container.Register<T>(instance).Life(life);
        }
        bool IDependencyResolver.IsTypeRegistered(Type type)
        {
            return _container.CanResolve(type);
        }
        T IDependencyResolver.Resolve<T>()
        {
            return _container.Resolve<T>();
        }
        object IDependencyResolver.Resolve(Type type)
        {
            return _container.Resolve(type);
        }

        private LifeStyle ParseLife(Type type)
        {
            var componentAttributes = type.GetCustomAttributes(typeof(ComponentAttribute), false);
            return componentAttributes.Count() <= 0 ? LifeStyle.Transient : (componentAttributes[0] as ComponentAttribute).LifeStyle;
        }
    }

    public static class TinyIoCContainerExtensions
    {
        public static TinyIoCContainer.RegisterOptions Life(this TinyIoCContainer.RegisterOptions registration, LifeStyle life)
        {
            if (life == LifeStyle.Singleton)
            {
                return registration.AsSingleton();
            }
            return registration.AsMultiInstance();
        }
    }
}

