using System;
using System.Collections.Generic;
using System.Text;

namespace DgnSharp
{
    public struct ElementSymbology
    {
        
        public ElementSymbology(int value)
        {
            Style = (Styles)(value & 0x7);
            Weight = (value >> 3) & 0x1F;
            Color = (value >> 8) & 0xFF;
        }

        public ElementSymbology(int color, int weight, Styles style)
        {
            Weight = weight;
            Style = style;
            Color = color;
        }

        

        public int GetInt()
        {
            int value = (int)Style;
            value += (Weight << 3);
            value += (Color << 8);
            return value;
        }
        public int Color { get; }
        public int Weight { get; }
        public enum Styles {
            Solid=0,
            Dotted=1,
            MediumDashed=2,
            LongDashed=3,
            DotDashed=4,
            ShortDashed=5,
            DashDoubleDot=6,
            LongDashShortDash=7
        }

        public Styles Style { get; }

    }
}
