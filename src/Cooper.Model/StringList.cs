//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Collections.Generic;
using System.Linq;

namespace Cooper.Model
{
    /// <summary>一个组件类，提供将String Item List与其拼接字符串进行转换的功能
    /// <remarks>
    /// 拼接分隔符为$
    /// </remarks>
    /// </summary>
    public class StringList
    {
        private IList<string> _items;
        private string _seperator = "$";

        public StringList(string initialStringValue)
        {
            this._items = Deserialize(initialStringValue);
        }

        /// <summary>添加一个字符串item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public StringList Add(string item)
        {
            Assert.IsNotNullOrWhiteSpace(item);
            if (this._items == null)
                this._items = new List<string>();
            this._items.Add(item);
            return this;
        }
        /// <summary>移除一个字符串item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public StringList Remove(string item)
        {
            Assert.IsNotNullOrWhiteSpace(item);
            if (this._items == null)
                this._items = new List<string>();
            this._items = this._items.Where(x => string.Compare(x, item, true) != 0).ToList();
            return this;
        }

        /// <summary>获取所有List中的所有Item
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetAllItems()
        {
            if (this._items == null)
                this._items = new List<string>();
            return this._items;
        }

        /// <summary>返回将List中的StringItem进行拼接后的字符串，拼接分隔符为$
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Serialize(this._items);
        }

        private IList<string> Deserialize(string serializedValue)
        {
            return string.IsNullOrWhiteSpace(serializedValue)
                ? new List<string>()
                : serializedValue.Split(new string[] { this._seperator }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }
        private string Serialize(IList<string> items)
        {
            var value = string.Join(this._seperator, items.Where(item => !string.IsNullOrWhiteSpace(item)).ToArray());
            if (!string.IsNullOrEmpty(value))
            {
                return string.Format("${0}$", value);
            }
            return value;
        }
    }
}
