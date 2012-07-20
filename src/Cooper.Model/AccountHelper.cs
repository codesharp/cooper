//Copyright (c) CodeSharp.  All rights reserved. - http://www.codesharp.cn/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.Services.Transaction;
using CodeSharp.Core;
using CodeSharp.Core.Services;

namespace Cooper.Model.Accounts
{
    /// <summary>账号业务辅助
    /// <remarks>用于完成一些难以在具体聚合中表述的逻辑或部分控制逻辑</remarks>
    /// </summary>
    public interface IAccountHelper
    {
        /// <summary>根据外部账号名创建系统内账号
        /// </summary>
        /// <typeparam name="T">连接类型</typeparam>
        /// <param name="connectionAccountName"></param>
        /// <param name="token">连接该账号的令牌信息</param>
        /// <returns></returns>
        Account CreateBy<T>(string connectionAccountName, string token) where T : AccountConnection;
        /// <summary>根据外部账号名创建系统内账号
        /// </summary>
        /// <param name="connectionType">连接类型</param>
        /// <param name="connectionAccountName"></param>
        /// <param name="token">连接该账号的令牌信息</param>
        /// <returns></returns>
        Account CreateBy(Type connectionType, string connectionAccountName, string token);
    }
    [CodeSharp.Core.Component]
    [Transactional]
    public class AccountHelper : IAccountHelper
    {
        static readonly Random _rd = new Random();
        private ILog _log;
        private IAccountService _accountService;
        private IAccountConnectionService _connectionService;
        public AccountHelper(ILoggerFactory factory
            , IAccountService accountService
            , IAccountConnectionService connectionService)
        {
            this._log = factory.Create(typeof(AccountHelper));
            this._accountService = accountService;
            this._connectionService = connectionService;
        }

        #region IAccountHelper Members
        [Transaction(TransactionMode.Requires)]
        Account IAccountHelper.CreateBy<T>(string connectionAccountName, string token)
        {
            return (this as IAccountHelper).CreateBy(typeof(T), connectionAccountName, token);
        }
        [Transaction(TransactionMode.Requires)]
        Account IAccountHelper.CreateBy(Type connectionType, string connectionAccountName, string token)
        {
            //创建随机账号
            var account = new Account("a"
                + DateTime.Now.ToString("yyyyMMddHHmmss")
                + _rd.Next(10, 100));//TODO:随机数方式修改或改为guid
            this._accountService.Create(account);

            //创建连接
            AccountConnection connection = null;
            if (connectionType == typeof(GoogleConnection))
                connection = new GoogleConnection(connectionAccountName, token, account);
            else if (connectionType == typeof(GitHubConnection))
                connection = new GitHubConnection(connectionAccountName, token, account);
            else
                connection = this.GenerateConnection(account, connectionType, connectionAccountName, token);
            //指定令牌
            connection.SetToken(token);
            this._connectionService.Create(connection);

            if (this._log.IsInfoEnabled)
                this._log.InfoFormat("根据连接{0}|{1}创建账号#{2}"
                    , connectionType
                    , connectionAccountName
                    , account.ID);

            return account;
        }
        #endregion

        protected virtual AccountConnection GenerateConnection(Account account, Type connectionType, string connectionAccountName, string token)
        {
            throw new NotSupportedException();
        }
    }
}