//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

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
            Map(m => m.TaskFolderId).Column("TasklistId").Nullable();
            DiscriminateSubClassesOnColumn("TaskType");
        }
    }
    public class TeamTaskMap : SubclassMap<Cooper.Model.Teams.Task>
    {
        public TeamTaskMap()
        {
            Table("Cooper_Task"); 
            EntityName("TeamTask");
            Map(m => m.TeamId);
            Map(m => m.AssigneeId);
            HasManyToMany(m => m.Projects)
                .Table("Cooper_TaskProjectRelationship")
                .ParentKeyColumn("TaskId")
                .ChildKeyColumn("ProjectId")
                .LazyLoad()
                .Cascade.SaveUpdate();
            DiscriminatorValue("team");
        }
    }
}
