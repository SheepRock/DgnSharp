using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace DgnSharp
{
    public class CellHeaderElement : GraphicalElement
    {
        public CellHeaderElement()
        {
            Elements = new List<IElement>();
        }

        public CellHeaderElement(Element element)
        {
            Header = element.Header;
            RawContent = new byte[element.RawContent.Length];
            element.RawContent.CopyTo(RawContent, 0);
            Elements = new List<IElement>();
        }

        public ushort CellLength
        {
            get
            {
                return (ushort)Helper.GetWord(RawContent, 17);
            }
            set
            {
                Helper.SetWord(ref rawContent, 17, value);
            }
        }
        public int Name
        {
            get
            {
                return Helper.GetLong(RawContent, 18);
            }
            set
            {
                Helper.SetLong(ref rawContent, 18, value);
            }
        }
        public int Class
        {
            get
            {
                return Helper.GetWord(RawContent, 20);
            }
            set
            {
                Helper.SetWord(ref rawContent, 20, value);
            }
        }
        public int[] Levels
        {
            get
            {
                int[] lv = new int[4];
                for(int i = 0; i < 4; i++)
                {
                    lv[i] = Helper.GetWord(RawContent, 21 + i);
                }
                return lv;
            }
            set
            {
                for (int i = 0; i < 4; i++)
                {
                    Helper.SetWord(ref rawContent, 21 + i, value[i]);
                }
            }
        }
        public Point2d RngLow
        {
            get
            {
                return new Point2d(Helper.GetLongMid(RawContent, 25), Helper.GetLongMid(RawContent, 27));
            }
            set
            {
                Helper.SetLongMid(ref rawContent, 25, value.X);
                Helper.SetLongMid(ref rawContent, 27, value.Y);
            }
        }
        public Point2d RngHigh
        {
            get
            {
                return new Point2d(Helper.GetLongMid(RawContent, 29), Helper.GetLongMid(RawContent, 31));
            }
            set
            {
                Helper.SetLongMid(ref rawContent, 29, value.X);
                Helper.SetLongMid(ref rawContent, 31, value.Y);
            }
        }
        public ImmutableArray<int> Transformation
        {
            get
            {
                int[] t = new int[4];
                for(int i = 0; i < 4; i++)
                {
                    t[i] = Helper.GetLongMid(RawContent, 33 + 2*i);
                }
                return t.ToImmutableArray();
            }
            set
            {
                for (int i = 0; i < 4; i++)
                {
                    Helper.SetLongMid(ref rawContent, 33 + 2*i, value[i]);
                }
            }
        }

        public double XScale
        {
            get
            {
                var t = Transformation;
                double a2 = Math.Pow(t[0],2);
                double c2 = Math.Pow(t[2], 2);
                return Math.Sqrt(a2 + c2) / 214748;
            }
        }
        public double YScale
        {
            get
            {
                var t = Transformation;
                double b2 = Math.Pow(t[1], 2);
                double d2 = Math.Pow(t[3], 2);
                return Math.Sqrt(b2 + d2) / 214748;                
            }
        }

        public double CellRotation
        {
            get
            {
                var t = Transformation;
                double a = t[0];
                double b = t[1];
                double a2 = Math.Pow(t[0],2);
                double c2 = Math.Pow(t[2],2);
                double rotation;
                if ((a2 + c2) <= 0.0)
                    rotation = 0.0;
                else
                    rotation = Math.Acos(a / Math.Sqrt(a2 + c2));

                if (b <= 0)
                    rotation = rotation * 180 / Math.PI;
                else
                    rotation = 360 - rotation * 180 / Math.PI;

                return rotation;
            }
        }

        private Point2d OriginRaw
        {
            get
            {
                return new Point2d(Helper.GetLongMid(RawContent, 41), Helper.GetLongMid(RawContent, 43));
            }
            set
            {
                Helper.SetLongMid(ref rawContent, 41, value.X);
                Helper.SetLongMid(ref rawContent, 43, value.Y);
            }
        }

        public DPoint2d Origin
        {
            get
            {
                var p = OriginRaw;
                return new DPoint2d(
                    p.X/scale,
                    p.Y/scale
                    );
            }
            set
            {
                OriginRaw = new Point2d(
                    (int)Math.Round(value.X * scale),
                    (int)Math.Round(value.Y * scale)
                    );
            }
        }

        public List<IElement> Elements { get; set; }
        public new CellHeaderElement Copy()
        {
            CellHeaderElement newLine = new CellHeaderElement(this);
            return newLine;
        }

        public override void Translate(double xOffset, double yOffset)
        {
            var o = Origin;
            Origin = new DPoint2d(
                o.X + xOffset,
                o.Y + yOffset
                );
            var rgn = Range;
            Range = new DRange(
                new DPoint3d(
                    rgn.Minimum.X + xOffset,
                    rgn.Minimum.Y + yOffset,
                    rgn.Minimum.Z
                    ),
                new DPoint3d(
                    rgn.Maximum.X + xOffset,
                    rgn.Maximum.Y + yOffset,
                    rgn.Minimum.Z
                    )
                );
        }

        public void RecalculateLength()
        {
            int len = WordToFollow + 2 - 19;
            foreach(var el in Elements)
            {
                if(el is CellHeaderElement c)
                {
                    c.RecalculateLength();
                    len += c.CellLength + 19;
                }
                else
                {
                    len += el.WordToFollow + 2;
                }
            }
            CellLength = (ushort)len;
        }
    }
}
