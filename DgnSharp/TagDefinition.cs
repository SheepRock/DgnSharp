using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace DgnSharp
{
    public class TagDefinition : GraphicalElement
    {
        public TagDefinition()
        {
        }

        public TagDefinition(Element element)
        {
            Header = element.Header;
            RawContent = new byte[element.RawContent.Length];
            element.RawContent.CopyTo(RawContent, 0);
            ProcessElement();
        }

        public string Name
        {
            get;
            set;
        }
        public int TagCount
        {
            get;
            set;
        }
        public List<TagSet> TagSets
        {
            get;
            set;
        }
        
        public void ProcessElement()
        {
            TagSets = new List<TagSet>();
            TagCount = BitConverter.ToUInt16(RawContent, 40);            
            var nameBytes = RawContent.Skip(44).TakeWhile(x => x != 0).ToArray();
            Name = Encoding.GetEncoding(1252).GetString(nameBytes);

            int currIndex = 44 + nameBytes.Length + 2;
            for (int i = 0; i < TagCount; i++)
            {
                TagSet ts = new TagSet();
                var tagNameBytes = RawContent.Skip(currIndex).TakeWhile(x => x != 0).ToArray();
                ts.Name = Encoding.GetEncoding(1252).GetString(tagNameBytes);
                currIndex += tagNameBytes.Length + 1;
                ts.Id = BitConverter.ToUInt16(RawContent, currIndex);
                currIndex += 2;
                var promptBytes = RawContent.Skip(currIndex).TakeWhile(x => x != 0).ToArray();
                ts.UserPrompt = Encoding.GetEncoding(1252).GetString(promptBytes);
                currIndex += promptBytes.Length + 1;
                ts.TagType = (TagSet.TagTypes)BitConverter.ToUInt16(RawContent, currIndex);
                currIndex += 2+2;
                ts.Flag = (TagSet.Flags)RawContent[currIndex];
                currIndex += 3;
                switch (ts.TagType)
                {
                    case TagSet.TagTypes.Float:
                        ts.DefaultValue = Helper.GetDoubleFromByteIndex(RawContent, currIndex);
                        currIndex += 8;
                        break;
                    case TagSet.TagTypes.Integer:
                        ts.DefaultValue = (int)Helper.GetLongFromByteIndex(RawContent, currIndex);
                        currIndex += 4;
                        break;
                    case TagSet.TagTypes.String:
                        var defaultBytes = RawContent.Skip(currIndex).TakeWhile(x => x != 0).ToArray();
                        ts.DefaultValue = Encoding.GetEncoding(1252).GetString(defaultBytes);
                        currIndex += defaultBytes.Length + 1;
                        break;
                }
                TagSets.Add(ts);

            }
            
        }

        public new TagDefinition Copy()
        {
            TagDefinition newElement = new TagDefinition(this);
            return newElement;
        }
    }

    public class TagSet
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public string UserPrompt { get; set; }
        public enum TagTypes { String=1, Integer=3, Float=4, Attr=5}
        public TagTypes TagType { get; set; }
        public object DefaultValue { get; set; }

        [Flags]
        public enum Flags {NotDisplay = 1, Default=2, Confirm=4, NotVariable = 8 };
        public Flags Flag { get; set; }
    }
}
