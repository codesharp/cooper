//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cooper.Model.Tasks;
using FluentNHibernate;
using FluentNHibernate.Mapping;

namespace Cooper.Model.Mappings
{
    public class TaskFolderMap : ClassMap<TaskFolder>
    {
        public TaskFolderMap()
        {
            Table("Cooper_Tasklist");
            Id(m => m.ID);
            Map(m => m.CreateTime);
            Map(m => m.Name).Length(50);
            Map(Reveal.Member<TaskFolder>("_extensions")).Column("Extensions").Length(4000);
            DiscriminateSubClassesOnColumn("ListType");
        }
    }
    public class PersonalTaskFolderMap : SubclassMap<PersonalTaskFolder>
    {
        public PersonalTaskFolderMap()
        {
            Table("Cooper_Tasklist");
            Map(m => m.OwnerAccountId);
            DiscriminatorValue("per");
        }
    }
}
