//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;
using Cooper.Model.Tasks;

namespace Cooper.Model.Mappings
{
    public class TasklistMap : ClassMap<Tasklist>
    {
        public TasklistMap()
        {
            Table("Cooper_Tasklist");
            Id(m => m.ID);
            Map(m => m.CreateTime);
            Map(m => m.Name).Length(50);
            DiscriminateSubClassesOnColumn("ListType");
        }
    }
    public class PersonalTasklistMap : SubclassMap<PersonalTasklist>
    {
        public PersonalTasklistMap()
        {
            Table("Cooper_Tasklist");
            Map(m => m.OwnerAccountId);
            DiscriminatorValue("per");
        }
    }
}
