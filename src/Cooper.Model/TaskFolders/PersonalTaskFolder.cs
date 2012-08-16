//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cooper.Model.Accounts;

namespace Cooper.Model.Tasks
{
    public class PersonalTaskFolder : TaskFolder
    {
        /// <summary>获取拥有者账号标识
        /// </summary>
        public int OwnerAccountId { get; set; }

        protected PersonalTaskFolder() : base() { }
        public PersonalTaskFolder(string name, Account owner)
            : base(name)
        {
            Assert.IsValid(owner);
            this.OwnerAccountId = owner.ID;
        }
    }
}
