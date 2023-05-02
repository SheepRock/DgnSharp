using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace DgnSharp
{
    public class TextElement : GraphicalElement
    {
        public TextElement()
        {

        }

        public TextElement(Element element)
        {
            Header = element.Header;
            RawContent = new byte[element.RawContent.Length];
            element.RawContent.CopyTo(RawContent, 0);            
        }

        public byte Font
        {
            get
            {
                return RawContent[(18 - 2) * 2];
            }
            set
            {
                RawContent[(18 - 2) * 2] = value;
            }
        }

        public enum TextJustification
        {
            LeftTop=0,
            LeftCenter=1,
            LeftBottom=2,
            CenterTop=6,
            CenterCenter=7,
            CenterBottom=8,
            RightTop=12,
            RightCenter=13,
            RightBottom=14
        };

        public enum HorizontalJustifications
        {
            Left,
            Center,
            Right            
        };

        public enum VerticalJustifications
        {
            Top,
            Center,
            Bottom
        }

        public TextJustification Justification
        {
            get
            {
                return (TextJustification)RawContent[(18 - 2) * 2 + 1];
            }
            set
            {
                RawContent[(18 - 2) * 2 + 1] = (byte)value;
            }
        }
        public HorizontalJustifications HorizontalJustification
        {
            get
            {
                switch (Justification)
                {
                    case TextJustification.LeftBottom:
                    case TextJustification.LeftCenter:
                    case TextJustification.LeftTop:
                        return HorizontalJustifications.Left;
                    case TextJustification.CenterBottom:
                    case TextJustification.CenterTop:
                    case TextJustification.CenterCenter:
                        return HorizontalJustifications.Center;
                    case TextJustification.RightBottom:
                    case TextJustification.RightTop:
                    case TextJustification.RightCenter:
                        return HorizontalJustifications.Right;

                }
                return HorizontalJustifications.Center;
            }
        }

        public VerticalJustifications VerticalJustification
        {
            get
            {
                switch (Justification)
                {
                    case TextJustification.RightBottom:
                    case TextJustification.LeftBottom:
                    case TextJustification.CenterBottom:
                        return VerticalJustifications.Bottom;
                    case TextJustification.LeftTop:
                    case TextJustification.CenterTop:
                    case TextJustification.RightTop:
                        return VerticalJustifications.Top;
                    case TextJustification.LeftCenter:
                    case TextJustification.CenterCenter:
                    case TextJustification.RightCenter:
                        return VerticalJustifications.Center;
                }
                return VerticalJustifications.Center;
            }
        }

        private int LengthMultiplierRaw
        {
            get
            {
                return Helper.GetLongMid(RawContent, 18);
            }
            set
            {
                Helper.SetLongMid(ref rawContent, 18, value);
            }
        }
        public double LengthMultiplier
        {
            get
            {
                return Math.Round((LengthMultiplierRaw * 6.0 / 1000) / scale,4);
            }
            set
            {
                LengthMultiplierRaw = (int)Math.Round(scale*1000*value / 6);
            }
        }

        private int HeightMultiplierRaw
        {
            get
            {
                return Helper.GetLongMid(RawContent, 20);
            }
            set
            {
                Helper.SetLongMid(ref rawContent, 20, value);
            }
        }
        public double HeightMultiplier
        {
            get
            {
                return Math.Round((HeightMultiplierRaw * 6.0 / 1000) / scale,4);
            }
            set
            {
                HeightMultiplierRaw = (int)Math.Round(scale * 1000 * value / 6);
            }
        }
        public int Rotation
        {
            get
            {
                return Helper.GetLong(RawContent, 22);
            }
            set
            {
                Helper.SetLong(ref rawContent, 22, value);
            }
        }
        private Point2d OriginRaw
        {
            get
            {
                return new Point2d (
                    Helper.GetLongMid(RawContent, 24),
                    Helper.GetLongMid(RawContent, 26)
                );
            }
            set
            {
                Helper.SetLongMid(ref rawContent, 24, value.X);
                Helper.SetLongMid(ref rawContent, 26, value.Y);
            }
        }

        public DPoint2d Origin
        {
            get
            {
                return new DPoint2d(
                    OriginRaw.X/scale,
                    OriginRaw.Y/scale
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

        public DPoint2d UserOrigin
        {
            get
            {
                var or = Origin;
                double y = or.Y;
                double x = or.X;
                switch (VerticalJustification)
                {
                    case VerticalJustifications.Bottom:
                        break;
                    case VerticalJustifications.Center:
                        y += (HeightMultiplier / 2);
                        break;
                    case VerticalJustifications.Top:
                        y += HeightMultiplier;
                        break;
                }
                switch (HorizontalJustification)
                {
                    case HorizontalJustifications.Left:
                        break;
                    case HorizontalJustifications.Center:
                        x += (LengthMultiplier * NumberOfChars / 2);
                        break;
                    case HorizontalJustifications.Right:
                        x += (LengthMultiplier * NumberOfChars);
                        break;
                }
                return new DPoint2d(x, y);
            }
        }
        public byte NumberOfChars
        {
            get
            {
                return RawContent[(29 - 2) * 2];
            }
            set
            {
                RawContent[(29 - 2) * 2] = value;
            }
        }
        public byte NumberOfEnterDataFields
        {
            get
            {
                return RawContent[(29 - 2) * 2 + 1];
            }
            set
            {
                RawContent[(29 - 2) * 2 + 1] = value;
            }
        }

        public string Text
        {
            get
            {
                return Encoding.GetEncoding(1252).GetString(RawContent, (30 - 2) * 2, NumberOfChars);
            }
            set
            {
                int deltaSize = value.Length - NumberOfChars;
                int firstCharIndex = (30 - 2) * 2;
                byte[] enterDataFields = new byte[0];
                if (NumberOfEnterDataFields > 0)
                {
                    enterDataFields = new Span<byte>(RawContent, firstCharIndex + NumberOfChars, 3 * NumberOfEnterDataFields).ToArray();
                }
                

                var endSize = RawContent.Length - (firstCharIndex + NumberOfChars);
                
                var att = Attributes;
                int newAttributeIndex = firstCharIndex + value.Length + enterDataFields.Length;
                if (newAttributeIndex % 2 != 0)
                {
                    ++newAttributeIndex;
                }
                int newSize = newAttributeIndex + att.Length;
                if (newSize % 2 != 0)
                {
                    ++newSize;
                }
                IndexToAttribute = newAttributeIndex / 2 - 14;
                Array.Resize(ref rawContent, newSize);
                Encoding.GetEncoding(1252).GetBytes(value.ToCharArray(), 0, value.Length, RawContent, firstCharIndex);
                if (enterDataFields.Length > 0)
                {
                    var newenterPosition = firstCharIndex + value.Length;
                    enterDataFields.CopyTo(RawContent, newenterPosition);
                }
                if (att.Length > 0)
                {
                    att.CopyTo(RawContent, newAttributeIndex);
                }
                    
                NumberOfChars = (byte)value.Length;
                switch (HorizontalJustification)
                {
                    case HorizontalJustifications.Center:
                        Translate(-deltaSize*LengthMultiplier/2, 0);
                        break;
                    case HorizontalJustifications.Right:
                        Translate(-deltaSize * LengthMultiplier, 0);
                        break;
                }
            }
        }

        public new TextElement Copy()
        {
            return new TextElement(this);
        }

        public override void ReplaceText(string oldText, string newText)
        {
            string t = Text;
            if (t.Contains(oldText))
            {
                Text = t.Replace(oldText, newText);
            }            
        }

        public override void ReplaceText(IEnumerable<(string oldText, string newText)> replacements)
        {
            string oldText = Text;
            string newText = oldText;
            foreach(var r in replacements)
            {
                newText = newText.Replace(r.oldText, r.newText);
            }
            if (newText!=oldText)
            {
                Text = newText;
            }
        }

        public override void Translate(double xOffset, double yOffset)
        {
            var orig = Origin;
            Origin = new DPoint2d(
                orig.X+xOffset,
                orig.Y+yOffset
                );
            var rgn = Range;
            Range = new DRange(
                new DPoint3d(
                    rgn.Minimum.X+xOffset,
                    rgn.Minimum.Y+yOffset,
                    rgn.Minimum.Z
                    ),
                new DPoint3d(
                    rgn.Maximum.X + xOffset,
                    rgn.Maximum.Y + yOffset,
                    rgn.Maximum.Z
                    )
                );
        }
    }
}
