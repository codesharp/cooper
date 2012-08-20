//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using Cooper.Model.Contacts;
using FluentNHibernate.Mapping;

namespace Cooper.Model.Mappings
{
    public class ContactMap : ClassMap<Contact>
    {
        public ContactMap()
        {
            Table("Cooper_Contact");
            Id(m => m.ID);
            Map(m => m.AddressBookId);
            Map(m => m.AccountId);
            Map(m => m.FullName).Length(255);
            Map(m => m.Email).Length(255);
            Map(m => m.Phone).Length(100);
            Map(m => m.CreateTime);
            Map(m => m.LastUpdateTime);
        }
    }
}