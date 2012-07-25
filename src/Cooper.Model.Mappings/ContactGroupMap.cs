//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using Cooper.Model.ContactGroups;
using FluentNHibernate.Mapping;

namespace Cooper.Model.Mappings
{
    public class ContactGroupMap : ClassMap<ContactGroup>
    {
        public ContactGroupMap()
        {
            Table("Cooper_ContactGroup");
            Id(m => m.ID);
            Map(m => m.Name).Length(200);
            Map(m => m.AddressBookId);
            Map(m => m.CreateTime);
            HasMany(m => m.Contacts).KeyColumn("GroupId").LazyLoad();
        }
    }
}