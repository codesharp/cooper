//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using Cooper.Model.Contacts;
using FluentNHibernate.Mapping;
using Cooper.Model.Teams;

namespace Cooper.Model.Mappings
{
    public class MemberMap : ClassMap<Member>
    {
        public MemberMap()
        {
            Table("Cooper_TeamMember");
            Id(m => m.ID);
            Map(m => m.TeamId);
            Map(m => m.Name).Length(255);
            Map(m => m.Email).Length(255);
            Map(m => m.CreateTime);
            HasMany(m => m.AssignedTasks).EntityName("TeamTask").KeyColumn("AssigneeId").LazyLoad();
        }
    }
}