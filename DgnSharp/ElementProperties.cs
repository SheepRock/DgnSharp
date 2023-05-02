using System;
using System.Collections.Generic;
using System.Text;

namespace DgnSharp
{
    public class ElementProperties
    {

        public ElementProperties(int value)
        {
            if (value >= 0)
            {
                H = (value & 0x8000) > 0;
                Snappable = (value & 0x4000) == 0;
                Planar = (value & 0x2000) == 0;
                OrientedRelativeToScreen = (value & 0x1000) > 0;
                HasAttributes = (value & 0x0800) > 0;
                Modified = (value & 0x0400) > 0;
                IsNew = (value & 0x0200) > 0;
                Locked = (value & 0x0100) > 0;
                Class = (ElementClass)(value & 0x000F);
            }            
        }

        public ElementProperties()
        {
        }

        public int GetInt()
        {
            int r = (int)Class;
            r |= H ? 0x8000 : 0;
            r |= !Snappable ? 0x4000 : 0;
            r |= !Planar ? 0x2000 : 0;
            r |= OrientedRelativeToScreen ? 0x1000 : 0;
            r |= HasAttributes ? 0x0800 : 0;
            r |= Modified ? 0x0400 : 0;
            r |= IsNew ? 0x0200 : 0;
            r |= Locked ? 0x0100 : 0;            
            return r;
        }

        public bool H { get; }
        public ElementProperties SetH(bool value)
        {
            ElementProperties ep;
            if (value == true && H == false)
            {
                ep = new ElementProperties(GetInt() | 0x8000);
            }
            else if (value == false && H == true)
            {
                ep = new ElementProperties(GetInt() & (~0x8000));
            }
            else
            {
                ep = this;
            }
            return ep;
        }
        public bool Snappable { get;  }
        public bool Planar { get;  }
        public bool OrientedRelativeToScreen { get;  }
        public bool HasAttributes { get;  }
        public ElementProperties SetHasAttributes(bool value)
        {
            ElementProperties ep;
            if(value == true && HasAttributes == false)
            {
                ep = new ElementProperties(GetInt() | 0x0800);
            }
            else if (value == false && HasAttributes == true)
            {
                ep = new ElementProperties(GetInt() & (~0x0800));
            }
            else
            {
                ep = this;
            }
            return ep;
        }
        public bool Modified { get; }
        public bool IsNew { get;  }
        public bool Locked { get;  }
        public enum ElementClass {
            Primary = 0,
            Pattern = 1,
            Construction = 2,
            Dimension = 3,
            PrimaryRule = 4,
            LinearPatterned = 5,
            ConstructionRule = 6}
        public ElementClass Class { get;  }
    }
}
