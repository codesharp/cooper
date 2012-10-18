using System;

namespace CooperDemo.Infrastructure
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ComponentAttribute : Attribute
    {
        public LifeStyle LifeStyle { get; set; }

        public ComponentAttribute() : this(LifeStyle.Transient) { }
        public ComponentAttribute(LifeStyle lifeStyle)
        {
            this.LifeStyle = lifeStyle;
        }
    }
}

