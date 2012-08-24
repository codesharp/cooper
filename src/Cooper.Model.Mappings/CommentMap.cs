//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using Cooper.Model.Teams;
using FluentNHibernate.Mapping;
using Cooper.Model.Accounts;
using FluentNHibernate;

namespace Cooper.Model.Mappings
{
    public class CommentMap : ClassMap<Comment>
    {
        public CommentMap()
        {
            Table("Cooper_TaskComment");
            Id(m => m.ID);
            Map(m => m.Body);
            Map(m => m.CreateTime);
            References(m => m.Creator, "CreatorId");
        }
    }
}