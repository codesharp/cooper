﻿//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using Cooper.Model.Teams;
using FluentNHibernate;
using FluentNHibernate.Mapping;

namespace Cooper.Model.Mappings
{
    public class ProjectMap : ClassMap<Project>
    {
        public ProjectMap()
        {
            Table("Cooper_Project");
            Id(m => m.ID);
            Map(m => m.Name).Length(255);
            Map(m => m.IsPublic);
            Map(m => m.TeamId);
            Map(Reveal.Member<Project>("_extensions")).Column("Extensions").Length(4000);
            Map(m => m.CreateTime);
        }
    }
}