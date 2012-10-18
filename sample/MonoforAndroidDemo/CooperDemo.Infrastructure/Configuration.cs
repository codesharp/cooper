using System;
using System.Reflection;

namespace CooperDemo.Infrastructure
{
    public class Configuration
    {
        private static Configuration _instance;

        /// <summary>
        /// ���ص�ǰ�������ʵ��
        /// </summary>
        public static Configuration Instance
        {
            get
            {
                if (_instance == null)
                {
                    throw new NullReferenceException("δ��ʼ���������");
                }
                return _instance;
            }
        }

        private Configuration() { }

        /// <summary>
        /// ��ʼ���������
        /// </summary>
        public static Configuration Config()
        {
            if (_instance != null)
            {
                throw new InvalidOperationException("�����ظ���ʼ���������");
            }
            _instance = new Configuration();
            return _instance;
        }

        /// <summary>
        /// ���ÿ������Ҫ��IOC����
        /// </summary>
        /// <param name="resolver"></param>
        public Configuration SetResolver(IDependencyResolver resolver)
        {
            DependencyResolver.SetResolver(resolver);
            return this;
        }
        /// <summary>
        /// �Զ�ע��Ĭ�ϵ������Service, Repository, Component
        /// </summary>
        public void RegisterDefaultComponents(params Assembly[] assemblies)
        {
            DependencyResolver.RegisterTypes(TypeUtils.IsComponent, assemblies);
            DependencyResolver.RegisterTypes(TypeUtils.IsRepository, assemblies);
            DependencyResolver.RegisterTypes(TypeUtils.IsService, assemblies);
        }
    }
}

