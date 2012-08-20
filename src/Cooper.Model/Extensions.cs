//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Cooper.Model.Accounts;
using Cooper.Model.Tasks;
using Cooper.Model.Teams;

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
        Assert.IsNotNullOrWhiteSpace(team.Name);
    }
    /// <summary>断言团队项目是否有效
    /// </summary>
    /// <param name="project"></param>
    public static void IsValid(Project project)
    {
        Assert.IsNotNull(project);
        Assert.Greater(project.ID, 0);
        Assert.IsNotNullOrWhiteSpace(project.Name);
        Assert.Greater(project.TeamId, 0);
    }
    /// <summary>断言团队成员是否有效
    /// </summary>
    /// <param name="teamMember"></param>
    public static void IsValid(TeamMember teamMember)
    {
        Assert.IsNotNull(teamMember);
        Assert.Greater(teamMember.ID, 0);
        Assert.IsNotNullOrWhiteSpace(teamMember.Name);
        Assert.IsNotNullOrWhiteSpace(teamMember.Email);
        Assert.Greater(teamMember.TeamId, 0);
    }
}
