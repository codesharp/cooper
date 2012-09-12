//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using FluentNHibernate;
using FluentNHibernate.Mapping;

namespace Cooper.Model.Mappings
{
    public class StringListMap : ComponentMap<StringList>
    {
        public StringListMap()
        {
            Map(Reveal.Member<StringList>("_serializedValue")).Column("Tags").Length(1000);
        }
    }
}
