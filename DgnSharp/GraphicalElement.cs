using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace DgnSharp
{
    public class GraphicalElement : Element
    {
        public GraphicalElement() : base()
        {

        }
        public GraphicalElement(int type) : base(type)
        {
        }

        public GraphicalElement(int type, byte[] content, int level = 0,
                                bool active = true, bool complexElement = false)
            : base(type, content, level, active, complexElement)
        {
        }

        public GraphicalElement(Element element)
        {
            Header = element.Header;
            RawContent = new byte[element.RawContent.Length];
            element.RawContent.CopyTo(RawContent, 0);
        }

        public long FilePosition { get; set; }

        //Palavras 3-14 (1-12 após o cabeçalho)
        private Range RangeRawUnits
        {
            get
            {
                uint lx = (uint)Helper.GetLongMid(RawContent, 1);
                uint ly = (uint)Helper.GetLongMid(RawContent, 3);
                uint lz = (uint)Helper.GetLongMid(RawContent, 5);
                uint hx = (uint)Helper.GetLongMid(RawContent, 7);
                uint hy = (uint)Helper.GetLongMid(RawContent, 9);
                uint hz = (uint)Helper.GetLongMid(RawContent, 11);
                Point3d min = new Point3d(lx, ly, lz);
                Point3d max = new Point3d(hx, hy, hz);
                return new Range(min, max);
            }
            set
            {
                Helper.SetLongMid(ref rawContent, 1, (int)value.Minimum.X);
                Helper.SetLongMid(ref rawContent, 3, (int)value.Minimum.Y);
                Helper.SetLongMid(ref rawContent, 5, (int)value.Minimum.Z);
                Helper.SetLongMid(ref rawContent, 7, (int)value.Maximum.X);
                Helper.SetLongMid(ref rawContent, 9, (int)value.Maximum.Y);
                Helper.SetLongMid(ref rawContent, 11, (int)value.Maximum.Z);
            }
        }

        public DRange Range
        {
            get
            {
                const long offset = 2147483648;
                var rng = RangeRawUnits;
                var p1 = new DPoint3d(
                    (rng.Minimum.X - offset) / scale,
                    (rng.Minimum.Y - offset) / scale,
                    (rng.Minimum.Z - offset) / scale
                    );
                var p2 = new DPoint3d(
                    (rng.Maximum.X - offset) / scale,
                    (rng.Maximum.Y - offset) / scale,
                    (rng.Maximum.Z - offset) / scale
                    );
                return new DRange(p1, p2);
            }
            set
            {
                const long offset = 2147483648;
                var p1 = new Point3d(
                    (uint)(value.Minimum.X * scale + offset),
                    (uint)(value.Minimum.Y * scale + offset),
                    (uint)(value.Minimum.Z * scale + offset)
                    );
                var p2 = new Point3d(
                    (uint)(value.Maximum.X * scale + offset),
                    (uint)(value.Maximum.Y * scale + offset),
                    (uint)(value.Maximum.Z * scale + offset)
                    );
                RangeRawUnits = new Range(p1, p2);
            }
        }
        //Grupo é a palavra 15 (13 após o cabeçalho)
        public ushort GroupNumber
        {
            get
            {
                return (ushort)Helper.GetWord(RawContent, 13);
            }
            set
            {
                Helper.SetWord(ref rawContent, 13, (ushort)value);
            }
        }

        //palavra 16 (14 após o cabeçalho)
        public int IndexToAttribute
        {
            get
            {
                return Helper.GetWord(RawContent, 14);
            }
            set
            {
                Helper.SetWord(ref rawContent, 14, value);
            }
        }

        //Palavra 17 (15 após o cabeçalho)
        public ElementProperties Properties
        {
            get
            {
                return new ElementProperties(Helper.GetWord(RawContent, 15));
            }
            set
            {
                Helper.SetWord(ref rawContent, 15, value.GetInt());
            }
        }

        //Palavra 18 (16 após o cabeçalho)
        public ElementSymbology Symbology
        {
            get
            {
                return new ElementSymbology(Helper.GetWord(RawContent, 16));
            }
            set
            {
                Helper.SetWord(ref rawContent, 16, value.GetInt());
            }
        }

        protected byte[] Attributes
        {
            get
            {
                int firstAttByte = 2 * (16 + IndexToAttribute - 2);
                if (firstAttByte < RawContent.Length)
                {
                    return new Span<byte>(RawContent, firstAttByte, RawContent.Length - firstAttByte).ToArray();
                }
                else
                {
                    return new byte[0];
                }

            }
        }

        public ImmutableList<DgnAttribute> AttributeList
        {
            get
            {
                var att = Attributes;
                int index = 0;
                var il = ImmutableList<DgnAttribute>.Empty;
                try
                {
                    while (att.Length > index)
                    {
                        DgnAttribute da = new DgnAttribute();
                        da.Header = (att[index]<<8)+att[index+1];
                        if ((att[index + 1] & 0x10) > 0)
                        {
                            da.Size = att[index] * 2 + 2;                                                
                        }
                        else
                        {
                            da.Size = 8;
                        }
                        da.Id = (att[index + 3] << 8) + att[index + 2];
                        da.Content = new Span<byte>(att, index + 4, da.Size - 4).ToArray();
                        index += da.Size;
                        il = il.Add(da);
                    }
                }
                catch 
                {

                }
                
                return il;
                
            }
            set
            {
                List<byte> newAtt = new List<byte>();
                var att = Attributes;
                foreach(var el in value)
                {
                    newAtt.Add((byte)(el.Header >> 8));
                    newAtt.Add((byte)(el.Header & 0xFF));
                    newAtt.Add((byte)(el.Id & 0xFF));
                    newAtt.Add((byte)(el.Id >> 8 ));
                    newAtt.AddRange(el.Content);
                }
                if (att.Length != newAtt.Count)
                {
                    Array.Resize(ref rawContent, rawContent.Length + (newAtt.Count - att.Length));
                    if(newAtt.Count>0 && !Properties.HasAttributes)
                    {
                        Properties = Properties.SetHasAttributes(true);
                    }
                    if (newAtt.Count == 0 && Properties.HasAttributes)
                    {
                        Properties = Properties.SetHasAttributes(false);
                    }
                }                
                newAtt.CopyTo(RawContent, 2 * (16 + IndexToAttribute - 2));                
            }
        }

        public int? ElementID
        {
            get
            {
                var att = AttributeList.FirstOrDefault(x => x.Id == 0x7D2F);
                if (att == null)
                {
                    return null;
                }
                return Helper.GetLongFromByteIndex(att.Content, 0);
            }
            set
            {
                var att = AttributeList.FirstOrDefault(x => x.Id == 0x7D2F);
                
                AttributeList = AttributeList.RemoveAll(x => x.Id == 0x7D2F);
                
                if (value != null)
                {
                    if (att == null)
                    {
                        att = new DgnAttribute();
                        att.Id = 0x7D2F;
                        att.Header = 0x0310;
                        
                    }
                    byte[] elid = new byte[4];
                    Helper.SetLongFromByteIndex(ref elid, 0, value.Value);
                    att.Content = elid;
                    AttributeList = AttributeList.Add(att);
                }
            }
        }

        public new GraphicalElement Copy()
        {
            GraphicalElement newElement = new GraphicalElement();
            newElement.Header = Header;
            newElement.RawContent = new byte[RawContent.Length];
            RawContent.CopyTo(newElement.rawContent, 0);
            return newElement;
        }

        public virtual void Translate(double xOffset, double yOffset)
        {

        }

        

    }
}
