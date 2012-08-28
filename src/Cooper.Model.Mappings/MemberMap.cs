﻿//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

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
            Map(m => m.AssociatedAccountId);
            DiscriminateSubClassesOnColumn("MemberType").Length(255);
        }
    }
    public class FullMemberMap : SubclassMap<FullMember>
    {
        public FullMemberMap()
        {
            Table("Cooper_TeamMember");
            DiscriminatorValue("full");
        }
    }
    public class GuestMemberMap : SubclassMap<GuestMember>
    {
        public GuestMemberMap()
        {
            Table("Cooper_TeamMember");
            DiscriminatorValue("guest");
        }
    }
}