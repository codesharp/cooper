//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Cooper.Model.Accounts;
using Cooper.Model.Tasks;
using Cooper.Model.Teams;
using NUnit.Framework;

//提供一些辅助
internal static class Extensions
{
    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        Assert.IsNotNull(source);
        foreach (var item in source)
        {
            action(item);
        }
    }
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
    /// <summary>IsNotNullOrWhiteSpace, LessOrEqual 255
    /// </summary>
    /// <param name="input"></param>
    public static void IsValidKey(string input)
    {
        Assert.IsNotNullOrWhiteSpace(input);
        Assert.LessOrEqual(input.Length, 255);
    }
    public static void IsValidEmail(string input)
    {
        var regex = new Regex(@"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*");
        Assert.IsTrue(regex.IsMatch(input));
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
    public static void IsValid(Cooper.Model.Tasks.Task task)
    {
        Assert.IsNotNull(task);
        Assert.Greater(task.ID, 0);
    }
    /// <summary>断言任务表是否有效
    /// </summary>
    /// <param name="folder"></param>
    public static void IsValid(TaskFolder folder)
    {
        Assert.IsNotNull(folder);
        Assert.Greater(folder.ID, 0);
    }
    /// <summary>断言团队任务是否有效
    /// </summary>
    /// <param name="task"></param>
    public static void IsValid(Cooper.Model.Teams.Task task)
    {
        Assert.IsNotNull(task);
        Assert.Greater(task.ID, 0);
        Assert.Greater(task.TeamId, 0);
    }
    /// <summary>断言团队是否有效
    /// </summary>
    /// <param name="team"></param>
    public static void IsValid(Team team)
    {
        Assert.IsNotNull(team);
        Assert.Greater(team.ID, 0);
        Assert.IsValidKey(team.Name);
    }
    /// <summary>断言团队项目是否有效
    /// </summary>
    /// <param name="project"></param>
    public static void IsValid(Project project)
    {
        Assert.IsNotNull(project);
        Assert.Greater(project.ID, 0);
        Assert.IsValidKey(project.Name);
        Assert.Greater(project.TeamId, 0);
    }
    /// <summary>断言团队成员是否有效
    /// </summary>
    /// <param name="member"></param>
    public static void IsValid(Member member)
    {
        Assert.IsNotNull(member);
        Assert.Greater(member.ID, 0);
        Assert.IsValidKey(member.Name);
        Assert.IsValidKey(member.Email);
        Assert.Greater(member.TeamId, 0);
    }
    /// <summary>断言评论是否有效
    /// </summary>
    /// <param name="comment"></param>
    public static void IsValid(Comment comment)
    {
        Assert.IsNotNull(comment);
        Assert.Greater(comment.ID, 0);
        Assert.IsNotNullOrWhiteSpace(comment.Body);
    }
}
