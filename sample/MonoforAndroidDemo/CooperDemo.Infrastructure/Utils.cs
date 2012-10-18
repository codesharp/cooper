using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace CooperDemo.Infrastructure
{
    public class Utils
    {
        /// <summary>
        /// �ж��Ƿ�������
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsNumber(string input)
        {
            long result;
            if (long.TryParse(input, out result))
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// ��һ�������Ķ���ת��Ϊָ�����͵Ķ���
        /// </summary>
        /// <typeparam name="T">ת���������</typeparam>
        /// <param name="value">Դ����</param>
        /// <returns>ת����Ķ���</returns>
        public static T ConvertType<T>(object value)
        {
            if (value == null)
            {
                return default(T);
            }
            TypeConverter typeConverter1 = TypeDescriptor.GetConverter(typeof(T));
            TypeConverter typeConverter2 = TypeDescriptor.GetConverter(value.GetType());
            if (typeConverter1.CanConvertFrom(value.GetType()))
            {
                return (T)typeConverter1.ConvertFrom(value);
            }
            else if (typeConverter2.CanConvertTo(typeof(T)))
            {
                return (T)typeConverter2.ConvertTo(value, typeof(T));
            }
            else
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
        }
        /// <summary>
        /// ��һ�������Ķ���ת��Ϊָ�����͵Ķ���
        /// </summary>
        /// <param name="value">Դ����</param>
        /// <typeparam name="targetType">ת���������</typeparam>
        /// <returns>ת�����ͺ�Ķ���</returns>
        public static object ConvertType(object value, Type targetType)
        {
            if (value == null)
            {
                return null;
            }

            TypeConverter typeConverter1 = TypeDescriptor.GetConverter(targetType);
            TypeConverter typeConverter2 = TypeDescriptor.GetConverter(value.GetType());

            if (typeConverter1.CanConvertFrom(value.GetType()))
            {
                return typeConverter1.ConvertFrom(value);
            }
            else if (typeConverter2.CanConvertTo(targetType))
            {
                return typeConverter2.ConvertTo(value, targetType);
            }
            else
            {
                return Convert.ChangeType(value, targetType);
            }
        }
        /// <summary>
        /// ����a=b&c=d&e=f�������ַ���Ϊ�ֵ�
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public IDictionary<string, string> Deserialize(string input)
        {
            var items = input.Split(new char[] { '&' });
            Dictionary<string, string> dic = new Dictionary<string, string>();
            foreach (var item in items)
            {
                var index = item.IndexOf('=');
                dic.Add(item.Substring(0, index), item.Substring(index + 1));
            }
            return dic;
        }
    }
}

