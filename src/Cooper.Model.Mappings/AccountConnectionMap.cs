//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

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
            DiscriminateSubClassesOnColumn("ConnectionType").AlwaysSelectWithValue().Length(255);
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
    public class EverNoteConnectionMap : SubclassMap<EverNoteConnection>
    {
        public EverNoteConnectionMap()
        {
            Table("Cooper_AccountConnection");
            DiscriminatorValue("evernote");
        }
    }
}