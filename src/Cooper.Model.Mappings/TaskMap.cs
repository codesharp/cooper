//Copyright (c) CodeSharp.  All rights reserved. - http://www.codesharp.cn/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;
using Cooper.Model.Tasks;

namespace Cooper.Model.Mappings
{
    public class TaskMap : ClassMap<Task>
    {
        public TaskMap()
        {
            Table("Cooper_Task");
            Id(m => m.ID);
            Map(m => m.Subject);
            Map(m => m.Body);
            Map(m => m.Priority).CustomType<Priority>();
            Map(m => m.DueTime).Nullable();
            Map(m => m.IsCompleted);
            
            Map(m => m.CreateTime);
            Map(m => m.LastUpdateTime);

            Map(m => m.CreatorAccountId);
            Map(m => m.AssignedContacterId);
        }
    }
}
