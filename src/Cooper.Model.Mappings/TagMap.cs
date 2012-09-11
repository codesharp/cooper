//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using Cooper.Model.Tasks;
using FluentNHibernate.Mapping;

namespace Cooper.Model.Mappings
{
    public class TagMap : ClassMap<Tag>
    {
        public TagMap()
        {
            Table("Cooper_Tag");
            Id(m => m.ID);
            Map(m => m.Name).Length(255);
            Map(m => m.ReferenceEntityId);

            DiscriminateSubClassesOnColumn("TagType").Length(255);
        }
    }
    public class PersonalTaskTagMap : SubclassMap<PersonalTaskTag>
    {
        public PersonalTaskTagMap()
        {
            Table("Cooper_Tag");
            EntityName("PersonalTaskTag");
            DiscriminatorValue("personalTaskTag");
        }
    }
    public class TeamTaskTagMap : SubclassMap<Teams.Tag>
    {
        public TeamTaskTagMap()
        {
            Table("Cooper_Tag");
            EntityName("TeamTaskTag");
            DiscriminatorValue("teamTaskTag");
        }
    }
}