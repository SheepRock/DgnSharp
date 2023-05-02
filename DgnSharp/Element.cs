using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace DgnSharp
{
    public class Element : IElement
    {
        protected readonly double scale = 10000;        
        protected byte[] rawContent;

        public Element()
        {
        }

        public Element(int type)
        {
            Type = type;
        }

        public Element(Element element)
        {
            Header = element.Header;
            RawContent = new byte[element.RawContent.Length];
            element.RawContent.CopyTo(RawContent, 0);
        }

        public Element(int type, byte[] content, int level = 0, bool active = true, bool complexElement = false) : this(type)
        {
            content.CopyTo(RawContent, 0);
            Level = level;
            Deleted = active;
            ComplexElement = complexElement;
        }

        //public uint ElementId { get; set; }
        public int Type { get; set; }
        public int Level { get; set; }
        public bool Deleted { get; set; }
        public bool ComplexElement { get; set; }
        public int Header
        {
            get
            {
                int h = Deleted ? 1 << 15 : 0;
                h |= (ushort)(Type << 8);
                h |= ComplexElement ? 1 << 7 : 0;
                h |= (byte)(Level&0x3F);
                return h;
            }
            set
            {
                Level = value & 0x3F;
                Type = (value >> 8) & 0x7F;
                Deleted = (value & 0x8000) > 0;
                ComplexElement = (value & 0x80) > 0;
            }
        }
        public int WordToFollow { get => RawContent.Length/2; }

        public byte[] RawContent { get => rawContent; set => rawContent = value; }

        public byte[] GetRaw()
        {
            if (RawContent.Length % 2 != 0)
            {
                Array.Resize(ref rawContent, rawContent.Length + 1);
            }
            List<byte> ret = new List<byte>();
            ret.AddRange(BitConverter.GetBytes((ushort)Header));
            ret.AddRange(BitConverter.GetBytes((ushort)WordToFollow));
            ret.AddRange(RawContent);
            return ret.ToArray();
        }

        

        public Element Copy()
        {
            Element newElement = new Element(this);
            return newElement;
        }

        public virtual void ReplaceText(string oldText, string newText)
        {

        }

        public virtual void ReplaceText(IEnumerable<(string oldText, string newText)> replacements)
        {

        }
    }
}
