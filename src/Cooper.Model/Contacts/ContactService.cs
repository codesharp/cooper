using System.Collections.Generic;
using Cooper.Model.Accounts;
using Cooper.Model.AddressBooks;

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
        Contact GetContact(int id);
        /// <summary>获取指定通讯簿的所有联系人
        /// </summary>
        IEnumerable<Contact> GetContacts(AddressBook addressBook);
    }
    /// <summary>联系人领域服务实现
    /// </summary>
    public class ContactService : IContactService
    {
        void IContactService.Create(Contact contact)
        {
            throw new System.NotImplementedException();
        }

        void IContactService.Update(Contact contact)
        {
            throw new System.NotImplementedException();
        }

        void IContactService.Delete(Contact contact)
        {
            throw new System.NotImplementedException();
        }

        Contact IContactService.GetContact(int id)
        {
            throw new System.NotImplementedException();
        }

        IEnumerable<Contact> IContactService.GetContacts(AddressBook addressBook)
        {
            throw new System.NotImplementedException();
        }
    }
}