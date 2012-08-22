//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using FluentNHibernate;
using FluentNHibernate.Mapping;
using Cooper.Model.Accounts;

namespace Cooper.Model.Mappings
{
    public class AccountMap : ClassMap<Account>
    {
        public AccountMap()
        {
            Table("Cooper_Account");
            Id(m => m.ID);
            Map(m => m.Name).Length(255);
            Map(m => m.CreateTime);
            Map(Reveal.Member<Account>("_password")).Column("Password").Length(255);
            //严格一对一实现
            HasOne<Profile>(Reveal.Member<Account>("_profile")).Cascade.SaveUpdate();
        }
    }
    public class ProfileMap : ClassMap<Profile>
    {
        public ProfileMap()
        {
            Table("Cooper_AccountProfile");
            Id(m => m.ID)
                .Column("AccountId")
                .GeneratedBy.Foreign("_account");
            HasOne<Account>(Reveal.Member<Profile>("_account"));
            Map(Reveal.Member<Profile>("_profile")).Column("Profile").Length(5000);
        }
    }
}
