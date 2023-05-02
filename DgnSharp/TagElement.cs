using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace DgnSharp
{
    public class TagElement : GraphicalElement
    {
        public TagElement()
        {
        }

        public TagElement(Element element)
        {
            Header = element.Header;
            RawContent = new byte[element.RawContent.Length];
            element.RawContent.CopyTo(RawContent, 0);
            //ProcessElement();
        }

        public int TagValue
        {
            get
            {
                return Helper.GetLong(RawContent, (36 - 4) / 2 + 1);
            }
            set
            {
                Helper.SetLong(ref rawContent, (36 - 4) / 2 + 1, value);
            }
        }
        public int Version
        {
            get
            {
                return Helper.GetWord(RawContent, (40 - 4) / 2 + 1);
            }
            set
            {
                Helper.SetWord(ref rawContent, (40 - 4) / 2 + 1, value);
            }
        }
        public int Flags
        {
            get
            {
                return Helper.GetWord(RawContent, (42 - 4) / 2 + 1);
            }
            set
            {
                Helper.SetWord(ref rawContent, (42 - 4) / 2 + 1, value);
            }
        }
        public int AssociatedId
        {
            get
            {
                return Helper.GetLong(RawContent, (44 - 4) / 2 + 1);
            }
            set
            {
                Helper.SetWord(ref rawContent, (44 - 4) / 2 + 1, value);
            }
        }
        public int TagSetId
        {
            get
            {
                return Helper.GetLongFromByteIndex(RawContent, 68 - 4);
            }
            set
            {
                Helper.SetLongFromByteIndex(ref rawContent, 68 - 4, value);
            }
        }
        public int TagId
        {
            get
            {
                return Helper.GetWord(RawContent, (72 - 4) / 2 + 1);
            }
            set
            {
                Helper.SetWord(ref rawContent, (72 - 4) / 2 + 1, value);
            }
        }
        public TagSet.TagTypes DataType
        {
            get
            {
                return (TagSet.TagTypes)Helper.GetWord(RawContent, (74 - 4) / 2 + 1);
            }
            set
            {
                Helper.SetWord(ref rawContent, (74 - 4) / 2 + 1, (int)value);
            }
        }
        public int DataBytes
        {
            get
            {
                return Helper.GetWord(RawContent, (150 - 4) / 2 + 1);
            }
            set
            {
                Helper.SetWord(ref rawContent, (150 - 4) / 2 + 1, value);
            }
        }
        public int NewDataBytes
        {
            get
            {
                return Helper.GetWord(RawContent, (152 - 4) / 2 + 1);
            }
            set
            {
                Helper.SetWord(ref rawContent, (152 - 4) / 2 + 1, value);
            }
        }
        public object Value
        {
            get
            {
                switch (DataType)
                {
                    case TagSet.TagTypes.Integer:
                        return Helper.GetWord(RawContent, (154 - 4) / 2 + 1);
                        
                    case TagSet.TagTypes.String:
                        var byteArr = RawContent.Skip(154 - 4).TakeWhile(x => x != 0).ToArray();
                        return Encoding.GetEncoding(1252).GetString(byteArr);
                        
                    case TagSet.TagTypes.Float:
                        return Helper.GetDouble(RawContent, (154 - 4) / 2 + 1);
                        
                    case TagSet.TagTypes.Attr:
                    default:
                        return null;
                        
                }
            }
            set
            {
                switch (DataType)
                {
                    case TagSet.TagTypes.Integer:
                        Helper.SetWord(ref rawContent, (154 - 4) / 2 + 1, (int)value);
                        break;
                    case TagSet.TagTypes.String:
                        //Pega os bytes finais para adicionar ao final do elemento 
                        var endBytes = RawContent.Skip(154 - 4).SkipWhile(x => x != 0).Skip(1).ToArray();
                        //Novo tamanho do valor. Deve-se adcionar um caracter nulo ao final da string
                        int stringLength = (value as string).Length + 1;
                        int newArraySize = 154 - 3 + stringLength + endBytes.Length;
                        //O tamanho do elemento deve sempre ser múltiplo de 2 (1 palavra)
                        if (newArraySize % 2 != 0)
                        {
                            ++newArraySize;
                        }
                        //Redimensional o elemento de acordo com o novo tamanho do valor
                        Array.Resize(ref rawContent, newArraySize);
                        //Escreve o novo texto ao elemento, adicionando o caracter nulo ao final
                        Encoding.GetEncoding(1252)
                            .GetBytes((value as string) + "\0")
                            .CopyTo(RawContent, 154 - 4);
                        //Escreve o final do elemento, armazenado anteriormente
                        endBytes.CopyTo(RawContent, 154 - 4 + stringLength);
                        //Novo tamanho do valor, em bytes
                        DataBytes = stringLength;
                        break;
                    case TagSet.TagTypes.Float:
                        Helper.SetDouble(ref rawContent, (154 - 4) / 2 + 1, (double)value);
                        break;
                    case TagSet.TagTypes.Attr:
                    default:
                        break;
                }
            }
        }

        
        public new TagElement Copy()
        {
            TagElement newElement = new TagElement(this);
            return newElement;
        }

        public override void ReplaceText(string oldText, string newText)
        {
            if(DataType == TagSet.TagTypes.String)
            {
                Value = (Value as string).Replace(oldText, newText);
            }            
        }

        public override void ReplaceText(IEnumerable<(string oldText, string newText)> replacements)
        {
            if (DataType == TagSet.TagTypes.String)
            {
                string oldText = (string)Value;
                string newText = oldText;
                foreach (var r in replacements)
                {
                    newText = newText.Replace(r.oldText, r.newText);
                }
                if (newText != oldText)
                {
                    Value = newText;
                }
            }
        }
    }
}
