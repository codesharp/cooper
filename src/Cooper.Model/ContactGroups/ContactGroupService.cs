//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System.Collections.Generic;
using Castle.Services.Transaction;
using CodeSharp.Core;
using CodeSharp.Core.RepositoryFramework;
using CodeSharp.Core.Services;
using Cooper.Model.Contacts;

namespace Cooper.Model.Contacts
{
    /// <summary>联系人组领域服务定义
    /// </summary>
    public interface IContactGroupService
    {
        /// <summary>创建联系人组
        /// </summary>
        void Create(ContactGroup contactGroup);
        /// <summary>更新联系人组
        /// </summary>
        void Update(ContactGroup contactGroup);
        /// <summary>删除联系人组
        /// </summary>
        void Delete(ContactGroup contactGroup);
        /// <summary>根据唯一标识获取联系人组
        /// </summary>
        ContactGroup GetContactGroup(int id);
        /// <summary>获取指定通讯簿的所有联系人组
        /// </summary>
        IEnumerable<ContactGroup> GetContactGroups(AddressBook addressBook);
    }
    /// <summary>联系人组领域服务实现
    /// </summary>
    [Transactional]
    public class ContactGroupService : IContactGroupService
    {
        private static IContactGroupRepository _repository;
        private ILog _log;

        static ContactGroupService()
        {
            _repository = RepositoryFactory.GetRepository<IContactGroupRepository, int, ContactGroup>();
        }

        public ContactGroupService(ILoggerFactory factory)
        {
            this._log = factory.Create(typeof(ContactGroupService));
        }

        [Transaction(TransactionMode.Requires)]
        void IContactGroupService.Create(ContactGroup contactGroup)
        {
            _repository.Add(contactGroup);
            if (this._log.IsInfoEnabled)
            {
                this._log.InfoFormat("新增联系人组#{0}|{1}|{2}", contactGroup.ID, contactGroup.Name, contactGroup.AddressBookId);
            }
        }
        [Transaction(TransactionMode.Requires)]
        void IContactGroupService.Update(ContactGroup contactGroup)
        {
            _repository.Update(contactGroup);
        }
        [Transaction(TransactionMode.Requires)]
        void IContactGroupService.Delete(ContactGroup contactGroup)
        {
            _repository.Remove(contactGroup);
            if (this._log.IsInfoEnabled)
            {
                this._log.InfoFormat("删除联系人组#{0}", contactGroup.ID);
            }
        }
        ContactGroup IContactGroupService.GetContactGroup(int id)
        {
            return _repository.FindBy(id);
        }
        IEnumerable<ContactGroup> IContactGroupService.GetContactGroups(AddressBook addressBook)
        {
            return _repository.FindBy(addressBook);
        }
    }
}