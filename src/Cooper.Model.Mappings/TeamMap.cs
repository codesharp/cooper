//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using Cooper.Model.Teams;
using FluentNHibernate;
using FluentNHibernate.Mapping;

namespace Cooper.Model.Mappings
{
    public class TeamMap : ClassMap<Team>
    {
        public TeamMap()
        {
            Table("Cooper_Team");
            Id(m => m.ID);
            Map(m => m.Name).Length(255);
            Component(m => m.Settings,
                component =>
                {
                    component.Map(Reveal.Member<ExtensionDictionary>("_extensions")).Column("Extensions").Length(10000);
                });
            Map(m => m.CreateTime);
            HasMany(m => m.Members)
                .KeyColumn("TeamId")
                .LazyLoad()
                .Cascade
                .SaveUpdate();
            HasMany(m => m.Projects)
                .KeyColumn("TeamId")
                .LazyLoad()
                .Cascade
                .SaveUpdate();
        }
    }
}