//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using CodeSharp.Core.DomainBase;
using Cooper.Model.Tasks;

namespace Cooper.Model
{
    /// <summary>Tag抽象实体
    /// </summary>
    public abstract class Tag : EntityBase<long>
    {
        public string Name { get; private set; }
        public long ReferenceEntityId { get; protected set; }

        protected Tag() { }
        protected Tag(string name)
        {
            Assert.IsNotNullOrWhiteSpace(name);
            this.Name = name;
        }
    }
}
