using System;
using System.Collections.Generic;
using System.Text;

namespace DgnSharp
{
    public class ArcElement : GraphicalElement
    {
        public ArcElement()
        {

        }
        public ArcElement(Element element)
        {
            Header = element.Header;
            RawContent = new byte[element.RawContent.Length];
            element.RawContent.CopyTo(RawContent, 0);
        }

        public int StartAngle
        {
            get
            {
                return Helper.GetLong(RawContent, 17);
            }
            set
            {
                Helper.SetLong(ref rawContent, 17, value);
            }
        }
        public int SweepAngle
        {
            get
            {
                return Helper.GetLong(RawContent, 19);
            }
            set
            {
                Helper.SetLong(ref rawContent, 19, value);
            }
        }
        public double PrimaryAxis
        {
            get
            {
                return Helper.GetDouble(RawContent, 21);
            }
            set
            {
                Helper.SetDouble(ref rawContent, 21, value);
            }
        }
        public double SecondaryAxis
        {
            get
            {
                return Helper.GetDouble(RawContent, 25);
            }
            set
            {
                Helper.SetDouble(ref rawContent, 25, value);
            }
        }
        public int Rotation
        {
            get
            {
                return Helper.GetLong(RawContent, 29);
            }
            set
            {
                Helper.SetLong(ref rawContent, 29, value);
            }
        }
        private DPoint2d CenterRaw
        {
            get
            {
                return new DPoint2d(Helper.GetDouble(RawContent, 31),
                    Helper.GetDouble(RawContent, 35));

            }
            set
            {
                Helper.SetDouble(ref rawContent, 31, value.X);
                Helper.SetDouble(ref rawContent, 35, value.Y);
            }
        }

        public DPoint2d Center
        {
            get
            {
                return new DPoint2d(
                    CenterRaw.X / scale,
                    CenterRaw.Y / scale
                    );
            }
            set
            {
                CenterRaw = new DPoint2d(
                    value.X * scale,
                    value.Y * scale
                    );
            }
        }

        public new ArcElement Copy()
        {
            return new ArcElement(this);
        }

        public override void Translate(double xOffset, double yOffset)
        {
            var c = Center;
            Center = new DPoint2d(
                c.X + xOffset,
                c.Y + yOffset
                );
            var rgn = Range;
            Range = new DRange(
                new DPoint3d(
                    rgn.Minimum.X + xOffset,
                    rgn.Minimum.Y + yOffset,
                    0
                    ),
                new DPoint3d(
                    rgn.Maximum.X + xOffset,
                    rgn.Maximum.Y + yOffset,
                    0
                    )
                );
        }
    }
}
