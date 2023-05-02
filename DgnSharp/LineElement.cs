using System;
using System.Collections.Generic;
using System.Text;

namespace DgnSharp
{
    public class LineElement : GraphicalElement
    {
        public LineElement()
        {
        }

        public LineElement(Element element)
        {
            Header = element.Header;
            RawContent = new byte[element.RawContent.Length];
            element.RawContent.CopyTo(RawContent, 0);
        }

        private Point2d StartRaw
        {
            get
            {
                return new Point2d(Helper.GetLongMid(RawContent, 17), Helper.GetLongMid(RawContent, 19));
            }
            set
            {
                Helper.SetLongMid(ref rawContent, 17, value.X);
                Helper.SetLongMid(ref rawContent, 19, value.Y);
            }
        }

        
        private Point2d EndRaw
        {
            get
            {
                return new Point2d(Helper.GetLongMid(RawContent, 21), Helper.GetLongMid(RawContent, 23));
            }
            set
            {
                Helper.SetLongMid(ref rawContent, 21, value.X);
                Helper.SetLongMid(ref rawContent, 23, value.Y);
            }
        }

        public DPoint2d Start
        {
            get
            {
                return new DPoint2d(StartRaw.X / scale, StartRaw.Y / scale);
            }
            set
            {
                StartRaw = new Point2d((int)Math.Round(value.X * scale), (int)Math.Round(value.Y * scale));
            }
        }
        public DPoint2d End
        {
            get
            {
                return new DPoint2d(EndRaw.X/scale, EndRaw.Y/scale);
            }
            set
            {
                EndRaw = new Point2d((int)Math.Round(value.X * scale), (int)Math.Round(value.Y * scale));
            }
        }
        public bool InfiniteLength
        {
            get
            {
                return Properties.H;
            }
            set
            {
                Properties = Properties.SetH(value);
            }
        }

        public new LineElement Copy()
        {
            LineElement newLine = new LineElement(this);
            return newLine;
        }

        public override void Translate(double xOffset, double yOffset)
        {
            var currStartCoord = Start;
            var currEndCoord = End;
            Start = new DPoint2d(currStartCoord.X + xOffset, currStartCoord.Y + yOffset);
            End = new DPoint2d(currEndCoord.X + xOffset, currEndCoord.Y + yOffset);
            Range = new DRange(
                new DPoint3d(
                    Math.Min(Start.X, End.X),
                    Math.Min(Start.Y, End.Y),
                    0),
                new DPoint3d(
                    Math.Max(Start.X, End.X),
                    Math.Max(Start.Y, End.Y),
                    0)
                );

        }
    }
}
