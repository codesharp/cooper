//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using CodeSharp.Core.DomainBase;
using Cooper.Model.Tasks;

namespace Cooper.Model.Tasks
{
    /// <summary>个人任务的Tag
    /// </summary>
    public class PersonalTaskTag : Tag
    {
        protected PersonalTaskTag() { }
        public PersonalTaskTag(string name, PersonalTask personalTask) : base(name)
        {
            Assert.IsValid(personalTask);
            this.ReferenceEntityId = personalTask.ID;
        }
    }
}
