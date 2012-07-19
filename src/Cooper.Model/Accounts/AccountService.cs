//Copyright (c) CodeSharp.  All rights reserved. - http://www.codesharp.cn/

using CodeSharp.Core.RepositoryFramework;
using Castle.Services.Transaction;
using CodeSharp.Core;
using CodeSharp.Core.Services;

namespace Cooper.Model.Accounts
{
    /// <summary>账号服务定义
    /// </summary>
    public interface IAccountService
    {
        /// <summary>创建账号
        /// </summary>
        /// <param name="account"></param>
        void Create(Account account);
        /// <summary>更新账号
        /// </summary>
        /// <param name="account"></param>
        void Update(Account account);
        /// <summary>根据标识获取账号
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Account GetAccount(int id);
        /// <summary>根据账号名获取账号
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        Account GetAccount(string name);
    }
    /// <summary>账号DomainService
    /// </summary>
    [Transactional]
    public class AccountService : IAccountService
    {
        private static IAccountRepository _repository;
        static AccountService()
        {
            _repository = RepositoryFactory.GetRepository<IAccountRepository, int, Account>();
        }
        private ILog _log;
        private ILockHelper _locker;
        public AccountService(ILoggerFactory factory, ILockHelper locker)
        {
            this._log = factory.Create(typeof(AccountService));
            this._locker = locker;
        }

        #region IAccountService Members
        [Transaction(TransactionMode.Requires)]
        void IAccountService.Create(Account account)
        {
            this._locker.Require<Account>();
            Assert.IsNull((this as IAccountService).GetAccount(account.Name));
            _repository.Add(account);

            if (this._log.IsInfoEnabled)
                this._log.InfoFormat("创建账号#{0}|{1}", account.ID, account.Name);
        }
        [Transaction(TransactionMode.Requires)]
        void IAccountService.Update(Account account)
        {
            Assert.Greater(account.ID, 0);
            //修改账号
            if (!string.IsNullOrEmpty(account._oldName) && account._oldName != account.Name)
            {
                this._locker.Require<Account>();
                Assert.IsNull((this as IAccountService).GetAccount(account.Name));
                if (this._log.IsInfoEnabled)
                    this._log.InfoFormat("账号#{0}的账号名变更：{1}调整为{2}", account.ID, account._oldName, account.Name);
            }
            _repository.Update(account);
        }
        Account IAccountService.GetAccount(int id)
        {
            return id <= 0 ? null : _repository.FindBy(id);
        }
        Account IAccountService.GetAccount(string name)
        {
            return string.IsNullOrWhiteSpace(name) ? null : _repository.FindBy(name);
        }
        #endregion
    }
}