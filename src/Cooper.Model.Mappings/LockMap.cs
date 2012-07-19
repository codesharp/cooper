//Copyright (c) CodeSharp.  All rights reserved. - http://www.codesharp.cn/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;

namespace Cooper.Model.Mappings
{
    public class LockMap : ClassMap<Lock>
    {
        public LockMap()
        {
            Table("Cooper_Lock");
            Id(m => m.ID).GeneratedBy.Assigned().UnsavedValue(null);
        }
    }
}
