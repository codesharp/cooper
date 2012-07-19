//Copyright (c) CodeSharp.  All rights reserved. - http://www.codesharp.cn/

using System;
using System.Collections.Generic;
using System.Linq;
using CodeSharp.Core.RepositoryFramework;
using Castle.Services.Transaction;
using CodeSharp.Core;
using CodeSharp.Core.Services;

namespace Cooper.Model.Accounts
{
    /// <summary>外部账号连接服务定义
    /// </summary>
    public interface IAccountConnectionService
    {
        /// <summary>创建外部账号连接
        /// </summary>
        /// <param name="connection"></param>
        void Create(AccountConnection connection);
        /// <summary>更新外部账号连接
        /// </summary>
        /// <param name="connection"></param>
        void Update(AccountConnection connection);
        /// <summary>删除账号连接
        /// </summary>
        /// <param name="connection"></param>
        void Delete(AccountConnection connection);
        /// <summary>获取外部账号连接
        /// </summary>
        /// <typeparam name="T">连接类型</typeparam>
        /// <param name="name">外部账号标识</param>
        /// <returns></returns>
        AccountConnection GetConnection<T>(string name) where T : AccountConnection;
        /// <summary>获取外部账号连接
        /// </summary>
        /// <param name="type">连接类型</param>
        /// <param name="name">外部账号标识</param>
        /// <returns></returns>
        AccountConnection GetConnection(Type type, string name);
        /// <summary>根据标识获取连接
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        AccountConnection GetConnection(int id);
        /// <summary>根据账号获取外部连接列表
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        IEnumerable<AccountConnection> GetConnections(Account account);
        /// <summary>获取所有外部账号连接
        /// </summary>
        /// <returns></returns>
        IEnumerable<AccountConnection> GetConnections();
    }
    /// <summary>外部账号连接DomainService
    /// </summary>
    [Transactional]
    public class AccountConnectionService : IAccountConnectionService
    {
        private static IAccountConnectionRepository _repository;
        static AccountConnectionService()
        {
            _repository = RepositoryFactory.GetRepository<IAccountConnectionRepository, int, AccountConnection>();
        }
        private ILog _log;
        private ILockHelper _locker;
        public AccountConnectionService(ILoggerFactory factory, ILockHelper locker)
        {
            this._log = factory.Create(typeof(AccountConnectionService));
            this._locker = locker;
        }

        #region IAccountConnectionService Members
        [Transaction(TransactionMode.Requires, IsolationMode.Serializable)]
        void IAccountConnectionService.Create(AccountConnection connection)
        {
            var t = connection.GetType();
            this._locker.Require(t);
            Assert.IsNull((this as IAccountConnectionService).GetConnection(t, connection.Name));
            _repository.Add(connection);
            
            if (this._log.IsInfoEnabled)
                this._log.InfoFormat("为账号#{0}创建了外部连接{1}#{2}"
                    , connection.AccountId
                    , connection
                    , connection.Name);
        }
        [Transaction(TransactionMode.Requires)]
        void IAccountConnectionService.Update(AccountConnection connection)
        {
            Assert.Greater(connection.ID, 0);
            Assert.Greater(connection.AccountId, 0);
            _repository.Update(connection);
        }
        [Transaction(TransactionMode.Requires)]
        void IAccountConnectionService.Delete(AccountConnection connection)
        {
            _repository.Remove(connection);

            if (this._log.IsInfoEnabled)
                this._log.InfoFormat("删除连接{0}#{1}|{2}"
                    , connection
                    , connection.ID
                    , connection.Name);
        }
        AccountConnection IAccountConnectionService.GetConnection<T>(string name)
        {
            return (this as IAccountConnectionService).GetConnection(typeof(T), name);
        }
        AccountConnection IAccountConnectionService.GetConnection(Type type, string name)
        {
            return _repository.FindBy(type, name);
        }
        AccountConnection IAccountConnectionService.GetConnection(int id)
        {
            return id <= 0 ? null : _repository.FindBy(id);
        }
        IEnumerable<AccountConnection> IAccountConnectionService.GetConnections(Account account)
        {
            return _repository.FindBy(account);
        }
        IEnumerable<AccountConnection> IAccountConnectionService.GetConnections()
        {
            return _repository.FindAll();
        }
        #endregion
    }
}