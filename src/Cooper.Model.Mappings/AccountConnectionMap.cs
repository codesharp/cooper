//Copyright (c) CodeSharp.  All rights reserved. - http://www.codesharp.cn/

using FluentNHibernate.Mapping;
using Cooper.Model.Accounts;

namespace Cooper.Model.Mappings
{
    public class AccountConnectionMap : ClassMap<AccountConnection>
    {
        public AccountConnectionMap()
        {
            Table("Cooper_AccountConnection");
            Id(m => m.ID);
            Map(m => m.Name).Length(255);
            Map(m => m.AccountId);
            Map(m => m.CreateTime);
            Map(m => m.Token).Length(1000);
            DiscriminateSubClassesOnColumn("ConnectionType");
        }
    }
    public class GoogleConnectionMap : SubclassMap<GoogleConnection>
    {
        public GoogleConnectionMap()
        {
            Table("Cooper_AccountConnection");
            DiscriminatorValue("google");
        }
    }
    public class GitHubConnectionMap : SubclassMap<GitHubConnection>
    {
        public GitHubConnectionMap()
        {
            Table("Cooper_AccountConnection");
            DiscriminatorValue("git");
        }
    }
}