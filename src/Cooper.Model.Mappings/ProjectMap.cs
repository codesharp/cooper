//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using Cooper.Model.Contacts;
using FluentNHibernate.Mapping;
using Cooper.Model.Teams;

namespace Cooper.Model.Mappings
{
    public class ProjectMap : ClassMap<Project>
    {
        public ProjectMap()
        {
            Table("Cooper_Project");
            Id(m => m.ID);
            Map(m => m.Name).Length(100);
            Map(m => m.IsPublic);
            Map(m => m.TeamId);
            Map(m => m.CreateTime);
        }
    }
}