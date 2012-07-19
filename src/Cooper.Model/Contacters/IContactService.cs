using System;
using System.Collections.Generic;

namespace Taobao.Cooper.Model
{
    /// <summary>
    /// 提供联系人操作相关的服务
    /// </summary>
    public interface IContactService
    {
        /// <summary>
        /// 根据给定的用户帐号返回其所有的联系人
        /// </summary>
        /// <param name="accountId">用户帐号ID</param>
        IEnumerable<Contacter> GetAccountContacts(Account account);

        /// <summary>
        /// 新建一个联系人
        /// </summary>
        void CreateContacter(Contacter contacter);

        /// <summary>
        /// 更新一个联系人
        /// </summary>
        void UpdateContacter(Contacter contacter);

        /// <summary>
        /// 根据联系人ID获取一个联系人
        /// </summary>
        /// <param name="contacterId">联系人ID</param>
        Contacter GetContacterById(int contacterId);

        /// <summary>
        /// 删除联系人
        /// </summary>
        void DeleteContacter(Contacter contacter);
    }
}