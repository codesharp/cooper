using System;
using System.Reflection;

namespace CooperDemo.Infrastructure
{
    public class Configuration
    {
        private static Configuration _instance;

        /// <summary>
        /// 返回当前框架配置实例
        /// </summary>
        public static Configuration Instance
        {
            get
            {
                if (_instance == null)
                {
                    throw new NullReferenceException("未初始化框架配置");
                }
                return _instance;
            }
        }

        private Configuration() { }

        /// <summary>
        /// 初始化框架配置
        /// </summary>
        public static Configuration Config()
        {
            if (_instance != null)
            {
                throw new InvalidOperationException("不可重复初始化框架配置");
            }
            _instance = new Configuration();
            return _instance;
        }

        /// <summary>
        /// 设置框架所需要的IOC容器
        /// </summary>
        /// <param name="resolver"></param>
        public Configuration SetResolver(IDependencyResolver resolver)
        {
            DependencyResolver.SetResolver(resolver);
            return this;
        }
        /// <summary>
        /// 自动注册默认的组件：Service, Repository, Component
        /// </summary>
        public void RegisterDefaultComponents(params Assembly[] assemblies)
        {
            DependencyResolver.RegisterTypes(TypeUtils.IsComponent, assemblies);
            DependencyResolver.RegisterTypes(TypeUtils.IsRepository, assemblies);
            DependencyResolver.RegisterTypes(TypeUtils.IsService, assemblies);
        }
    }
}

