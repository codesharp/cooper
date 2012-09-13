//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using Cooper.Model.Tasks;
using FluentNHibernate;
using FluentNHibernate.Mapping;

namespace Cooper.Model.Mappings
{
    public class TaskMap : ClassMap<Task>
    {
        public TaskMap()
        {
            Table("Cooper_Task");
            Id(m => m.ID);
            Map(m => m.Subject).Length(255);
            Map(m => m.Body).Length(10000);
            Map(m => m.Priority).CustomType<Priority>();
            Map(m => m.DueTime).Nullable();
            Map(m => m.IsCompleted);
            Component(Reveal.Member<Task, StringList>("_tagList"),
                component =>
                {
                    component.Map(Reveal.Member<StringList>("_serializedValue")).Column("Tags").Length(1000);
                });
            Map(m => m.CreateTime);
            Map(m => m.LastUpdateTime);
            Map(m => m.IsTrashed);

            DiscriminateSubClassesOnColumn("TaskType").Length(255);
        }
    }
    public class PersonalTaskMap : SubclassMap<PersonalTask>
    {
        public PersonalTaskMap()
        {
            Table("Cooper_Task");
            EntityName("PersonalTask");
            Map(m => m.CreatorAccountId);
            Map(m => m.TaskFolderId).Column("TasklistId").Nullable();
            DiscriminatorValue("personal");
        }
    }
    public class TeamTaskMap : SubclassMap<Cooper.Model.Teams.Task>
    {
        public TeamTaskMap()
        {
            Table("Cooper_Task");
            EntityName("TeamTask");
            Map(m => m.TeamId);
            Map(m => m.CreatorMemberId);
            Map(m => m.AssigneeId);
            HasMany(m => m.Comments)
                .KeyColumn("TaskId")
                .LazyLoad()
                .Cascade
                .SaveUpdate();
            HasManyToMany(m => m.ProjectIds)
                .Table("Cooper_TaskProjectRelationship")
                .ParentKeyColumn("TaskId")
                .ChildKeyColumn("ProjectId")
                .LazyLoad();
            DiscriminatorValue("team");
        }
    }
}
