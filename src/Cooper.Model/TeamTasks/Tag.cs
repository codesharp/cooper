//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using CodeSharp.Core.DomainBase;
using Cooper.Model.Tasks;

namespace Cooper.Model.Teams
{
    /// <summary>团队任务的Tag
    /// </summary>
    public class Tag : Cooper.Model.Tag
    {
        protected Tag() { }
        public Tag(string name, Teams.Task teamTask) : base(name)
        {
            Assert.IsValid(teamTask);
            this.ReferenceEntityId = teamTask.ID;
        }
    }
}
