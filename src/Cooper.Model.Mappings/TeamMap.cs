//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using Cooper.Model.Contacts;
using FluentNHibernate.Mapping;
using Cooper.Model.Teams;

namespace Cooper.Model.Mappings
{
    public class TeamMap : ClassMap<Team>
    {
        public TeamMap()
        {
            Table("Cooper_Team");
            Id(m => m.ID);
            Map(m => m.Name).Length(200);
            Map(m => m.CreateTime);
        }
    }
}