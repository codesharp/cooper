//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System.Collections.Generic;
using Castle.Services.Transaction;
using CodeSharp.Core;
using CodeSharp.Core.RepositoryFramework;
using CodeSharp.Core.Services;
using Cooper.Model.Accounts;

namespace Cooper.Model.Contacts
{
    /// <summary>通讯簿领域服务定义
    /// </summary>
    public interface IAddressBookService
    {
        /// <summary>创建通讯簿
        /// </summary>
        void Create(AddressBook addressBook);
        /// <summary>更新通讯簿
        /// </summary>
        void Update(AddressBook addressBook);
        /// <summary>删除通讯簿
        /// </summary>
        void Delete(AddressBook addressBook);
        /// <summary>根据唯一标识获取通讯簿
        /// </summary>
        AddressBook GetAddressBook(int id);
        /// <summary>获取指定账号的所有私人通讯簿
        /// </summary>
        IEnumerable<PersonalAddressBook> GetAddressBooks(Account owner);
        /// <summary>获取所有系统通讯簿
        /// </summary>
        IEnumerable<SystemAddressBook> GetAllSystemAddressBooks();
    }
    /// <summary>通讯簿领域服务实现
    /// </summary>
    [Transactional]
    public class AddressBookService : IAddressBookService
    {
        private static IAddressBookRepository _repository;
        private ILog _log;

        static AddressBookService()
        {
            _repository = RepositoryFactory.GetRepository<IAddressBookRepository, int, AddressBook>();
        }

        public AddressBookService(ILoggerFactory factory)
        {
            this._log = factory.Create(typeof(AddressBookService));
        }

        [Transaction(TransactionMode.Requires)]
        void IAddressBookService.Create(AddressBook addressBook)
        {
            _repository.Add(addressBook);
            if (this._log.IsInfoEnabled)
            {
                var personalAddressBook = addressBook as PersonalAddressBook;
                if (personalAddressBook != null)
                {
                    this._log.InfoFormat("新增私人通讯簿#{0}|{1}|{2}", addressBook.ID, addressBook.Name, personalAddressBook.OwnerAccountId);
                }
                else
                {
                    this._log.InfoFormat("新增通讯簿#{0}|{1}", addressBook.ID, addressBook.Name);
                }
            }
        }
        [Transaction(TransactionMode.Requires)]
        void IAddressBookService.Update(AddressBook addressBook)
        {
            _repository.Update(addressBook);
        }
        [Transaction(TransactionMode.Requires)]
        void IAddressBookService.Delete(AddressBook addressBook)
        {
            _repository.Remove(addressBook);
            if (this._log.IsInfoEnabled)
            {
                this._log.InfoFormat("删除通讯簿#{0}", addressBook.ID);
            }
        }
        AddressBook IAddressBookService.GetAddressBook(int id)
        {
            return _repository.FindBy(id);
        }
        IEnumerable<PersonalAddressBook> IAddressBookService.GetAddressBooks(Account owner)
        {
            return _repository.FindBy(owner);
        }
        IEnumerable<SystemAddressBook> IAddressBookService.GetAllSystemAddressBooks()
        {
            return _repository.FindAllSystemAddressBooks();
        }
    }
}