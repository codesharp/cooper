//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Cooper.Model.Accounts;
using Cooper.Model.Tasks;

//提供一些辅助
internal static class Extensions
{

}
//扩展断言
internal class Assert : NUnit.Framework.Assert
{
    /// <summary>断言是否空白字符串
    /// </summary>
    /// <param name="input"></param>
    public static void IsNotNullOrWhiteSpace(string input)
    {
        Assert.IsNotNullOrEmpty(input);
        Assert.IsNotNullOrEmpty(input.Trim());
        //Assert.IsFalse(string.IsNullOrWhiteSpace(input));
    }
    /// <summary>断言账号是否有效
    /// </summary>
    /// <param name="account"></param>
    public static void IsValid(Account account)
    {
        Assert.IsNotNull(account);
        Assert.Greater(account.ID, 0);
    }
    /// <summary>断言任务是否有效
    /// </summary>
    /// <param name="task"></param>
    public static void IsValid(Task task)
    {
        Assert.IsNotNull(task);
        Assert.Greater(task.ID, 0);
    }
    /// <summary>断言任务表是否有效
    /// </summary>
    /// <param name="tasklist"></param>
    public static void IsValid(Tasklist tasklist)
    {
        Assert.IsNotNull(tasklist);
        Assert.Greater(tasklist.ID, 0);
    }
}
