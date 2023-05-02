using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DgnSharp
{
    public class DgnReader : IDisposable
    {
        Stream stream;
        private int filePosition;
        public List<IElement> Elements { get; set; }

        public DgnReader(string file)
        {
            stream = new FileStream(file, FileMode.Open,FileAccess.Read,FileShare.ReadWrite);
            Elements = new List<IElement>();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public DgnReader(Stream stream)
        {
            this.stream = stream;
            Elements = new List<IElement>();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public void Read()
        {
            filePosition = 4000000;
            while (stream.Position<stream.Length)
            {
                Elements.Add(ReadNextElement());
            }            
            ProcessCells();
        }

        public GraphicalElement GetByID(int id)
        {
            return (GraphicalElement)Elements.FirstOrDefault(x => x is GraphicalElement ge && ge.ElementID == id);
        }

        public GraphicalElement GetByFilePosition(int position)
        {
            return (GraphicalElement)Elements.FirstOrDefault(x => x is GraphicalElement ge && ge.FilePosition == position);
        }

        public IEnumerable<TagElement> GetAllAssociatedTags(int id)
        {
            return Elements.Where(x => x is TagElement te && te.AssociatedId == id).Cast<TagElement>();
        }

        public IEnumerable<TagDefinition> TagDefinitions()
        {
            return Elements.Where(x => x is TagDefinition).Cast<TagDefinition>();
        }

        public IElement ReadNextElement()
        {
            long position = stream.Position;
            byte[] header = new byte[4];
            stream.Read(header, 0, 4);
            int elTypeLevel = Helper.GetWord(header, 1);
            int wordsToFollow = (ushort)Helper.GetWord(header, 2);
            byte[] rawContents = new byte[wordsToFollow * 2];
            stream.Read(rawContents, 0, rawContents.Length);
            Element el = new Element();
            el.Header = elTypeLevel;
            
            el.RawContent = rawContents;
            switch (el.Type)
            {
                //CellHeader
                case 2:
                    el = new CellHeaderElement(el);
                    break;
                //Line
                case 3:
                    el = new LineElement(el);
                    break;
                //Line String (Type 4), Shape (Type 6), Curve (Type 11), and Bspline Pole Element (Type 21)
                //Todos tem a mesma estrutura
                case 4:
                case 6:
                case 11:
                case 21:
                    el = new ShapeElement(el);
                    break;
                //Line
                case 9:
                    el = new TCBElement(el);
                    break;
                //Ellipse
                case 15:
                    el = new EllipseElement(el);
                    break;
                //Ellipse
                case 16:
                    el = new ArcElement(el);
                    break;
                //Text
                case 17:
                    el = new TextElement(el);
                    break;
                //Tag Element
                case 37:
                    el = new TagElement(el);
                    break;
                //MicroStation Application Elements
                case 66:
                    //Tag Set Definition 
                    if (el.Level == 24)
                    {
                        el = new TagDefinition(el);
                    }
                    else
                    {
                        goto default;
                    }
                    break;
                default:
                    
                    break;
            }
            
            if(!el.Deleted && el is GraphicalElement ge && !(ge is TagDefinition))
            {
                ge.FilePosition = filePosition++;
            }
            return el;            
        }

        public void ProcessCells()
        {
            for(int i = 0; i < Elements.Count; i++)
            {
                if(Elements[i] is CellHeaderElement ce)
                {
                    var sc = GetCellElements(i + 1, ce.CellLength - (ce.WordToFollow+2-19));
                    i += sc.count;
                    ce.Elements.AddRange(sc.elementos);
                }
            }
        }

        public (List<IElement> elementos, int totalSize, int count) GetCellElements(int start, int size)
        {
            List<IElement> els = new List<IElement>();
            int totsize = 0;
            int c = 0;
            for(int i = start; i < Elements.Count; i++)
            {
                if(size <= 0 || Elements[i].ComplexElement == false)
                {
                    break;
                }
                //if (Elements[i].Deleted == true)
                //{
                //    ++c;
                //    continue;
                //}
                els.Add(Elements[i]);
                ++c;
                size -= (Elements[i].WordToFollow+2);
                totsize += (Elements[i].WordToFollow+2);
                if(Elements[i] is CellHeaderElement ce)
                {
                    var sc = GetCellElements(i + 1, ce.CellLength - (ce.WordToFollow+2-19));
                    ce.Elements.AddRange(sc.elementos);
                    i += sc.count;
                    size -= sc.totalSize;
                    totsize += sc.totalSize;
                    c += sc.count;
                }
            }
            return (els,totsize,c);
        }

        public CellHeaderElement GetParent(GraphicalElement ge)
        {
            if (!ge.ComplexElement)
            {
                return null;
            }
            return (CellHeaderElement)Elements.Where(x => x is CellHeaderElement c && c.Elements.Contains(ge))?.Last();
        }

        public void RecalculateAllCells()
        {
            foreach(var c in Elements.Where(x=>x is CellHeaderElement).Reverse().Cast<CellHeaderElement>())
            {
                c.RecalculateLength();
            }
        }
        public int GetNewId()
        {
            var els = Elements.Where(x => x is GraphicalElement).ToList();
            if (els.Count == 0)
            {
                return 1;
            }

            return els.Max(x => ((GraphicalElement)x).ElementID ?? 0) + 1;
        }

        public int GetNewGraphicalGroup()
        {
            var els = Elements.Where(x => x is GraphicalElement).ToList();
            if (els.Count == 0)
            {
                return 1;
            }

            return els.Max(x => ((GraphicalElement)x).GroupNumber) + 1;
        }
        public void ReplaceText(string oldText, string newText)
        {
            foreach(var el in Elements)
            {
                el.ReplaceText(oldText, newText);
            }
        }

        public void ReplaceText(IEnumerable<(string oldText,string newText)> replacements)
        {
            foreach (var el in Elements)
            {
                el.ReplaceText(replacements);
            }
        }

        public void SaveAs(string fileName)
        {
            SaveAs(fileName, false);
        }

        public void SaveAs(string fileName, bool overwrite)
        {
            FileMode mode;
            if (overwrite)
            {
                mode = FileMode.Create;
            }
            else
            {
                mode = FileMode.CreateNew;
            }
            
            using (FileStream fs = new FileStream(fileName, mode))
            {
                SaveAs(fs);
            }
        }

        public void SaveAs(Stream fileStream)
        {            
            foreach (var e in Elements)
            {
                fileStream.Write(e.GetRaw());
            }            
        }

        public void Dispose()
        {
            stream.Dispose();
        }

        
    }
}
