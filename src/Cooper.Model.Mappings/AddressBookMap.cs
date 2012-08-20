//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using Cooper.Model.Contacts;
using FluentNHibernate.Mapping;

namespace Cooper.Model.Mappings
{
    public class AddressBookMap : ClassMap<AddressBook>
    {
        public AddressBookMap()
        {
            Table("Cooper_AddressBook");
            Id(m => m.ID);
            Map(m => m.Name).Length(255);
            Map(m => m.CreateTime);
            References(m => m.Parent).Column("ParentId").LazyLoad().Nullable();
            DiscriminateSubClassesOnColumn("AddressBookType");
        }
    }
    public class PersonalAddressBookMap : SubclassMap<PersonalAddressBook>
    {
        public PersonalAddressBookMap()
        {
            Table("Cooper_AddressBook");
            Map(m => m.OwnerAccountId);
            DiscriminatorValue("personal");
        }
    }
    public class SystemAddressBookMap : SubclassMap<SystemAddressBook>
    {
        public SystemAddressBookMap()
        {
            Table("Cooper_AddressBook");
            DiscriminatorValue("system");
        }
    }
}