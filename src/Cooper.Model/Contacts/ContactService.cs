//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Collections.Generic;
using Castle.Services.Transaction;
using CodeSharp.Core;
using CodeSharp.Core.RepositoryFramework;
using CodeSharp.Core.Services;
using Cooper.Model.Contacts;

namespace Cooper.Model.Contacts
{
    /// <summary>联系人领域服务定义
    /// </summary>
    public interface IContactService
    {
        /// <summary>创建联系人
        /// </summary>
        void Create(Contact contact);
        /// <summary>更新联系人
        /// </summary>
        void Update(Contact contact);
        /// <summary>删除联系人
        /// </summary>
        void Delete(Contact contact);
        /// <summary>根据唯一标识获取联系人
        /// </summary>
        Contact GetContact(Guid id);
        /// <summary>获取指定通讯簿的所有联系人
        /// </summary>
        IEnumerable<Contact> GetContacts(AddressBook addressBook);
    }
    /// <summary>联系人领域服务实现
    /// </summary>
    [Transactional]
    public class ContactService : IContactService
    {
        private static IContactRepository _repository;
        private ILog _log;

        static ContactService()
        {
            _repository = RepositoryFactory.GetRepository<IContactRepository, Guid, Contact>();
        }

        public ContactService(ILoggerFactory factory)
        {
            this._log = factory.Create(typeof(ContactService));
        }

        [Transaction(TransactionMode.Requires)]
        void IContactService.Create(Contact contact)
        {
            _repository.Add(contact);
            if (this._log.IsInfoEnabled)
            {
                this._log.InfoFormat("新增联系人#{0}|{1}|{2}", contact.ID, contact.FullName, contact.Email);
            }
        }
        [Transaction(TransactionMode.Requires)]
        void IContactService.Update(Contact contact)
        {
            _repository.Update(contact);
        }
        [Transaction(TransactionMode.Requires)]
        void IContactService.Delete(Contact contact)
        {
            _repository.Remove(contact);
            if (this._log.IsInfoEnabled)
            {
                this._log.InfoFormat("删除联系人#{0}", contact.ID);
            }
        }
        Contact IContactService.GetContact(Guid id)
        {
            return _repository.FindBy(id);
        }
        IEnumerable<Contact> IContactService.GetContacts(AddressBook addressBook)
        {
            return _repository.FindBy(addressBook);
        }
    }
}