//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System.Globalization;

namespace Cooper.Model
{
    //字符串操作辅助类
    public static class StringHelper
    {
        /// <summary>比较两个字符串，忽略大小写，忽略全半角
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static int CompareStringIgnoreCaseAndWidth(string x, string y)
        {
            return CultureInfo.InvariantCulture.CompareInfo.Compare(x, y, CompareOptions.IgnoreCase | CompareOptions.IgnoreWidth);
        }
    }
}
