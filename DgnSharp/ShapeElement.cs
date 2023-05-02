using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Collections.Immutable;

namespace DgnSharp
{
    public class ShapeElement : GraphicalElement
    {
        public ShapeElement()
        {

        }
            
        public ShapeElement(Element element)
        {
            Header = element.Header;
            RawContent = new byte[element.RawContent.Length];
            element.RawContent.CopyTo(RawContent, 0);
            
        }

        public int NumberOfVertexes
        {
            get
            {
                return Helper.GetWord(RawContent, 17);
            }
            set
            {
                Helper.SetWord(ref rawContent, 17, value);
            }
        }

        public bool Hole
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

        private ImmutableList<Point2d> VertexRaw
        {
            get
            {
                var vtx = new List<Point2d>();
                for (int i = 0; i < NumberOfVertexes; i++)
                {
                    int wordNumber = 18 + 4 * i;
                    Point2d p = new Point2d(
                        Helper.GetLongMid(RawContent, wordNumber),
                        Helper.GetLongMid(RawContent, wordNumber + 2)
                        );
                    vtx.Add(p);
                }
                return ImmutableList<Point2d>.Empty.AddRange(vtx);
            }
            set
            {
                var vertexEndAddress = (18 - 1) * 2 + NumberOfVertexes * 8;
                var attLength = RawContent.Length - vertexEndAddress; 
                var attBytes = new Span<byte>(RawContent, vertexEndAddress,attLength).ToArray();
                int sizeDelta = value.Count * 4 - NumberOfVertexes * 4;
                Array.Resize(ref rawContent, rawContent.Length + sizeDelta);
                for (int i = 0; i < value.Count; i++)
                {
                    int wordNumber = 18 + 4 * i;
                    Helper.SetLongMid(ref rawContent, wordNumber, value[i].X);
                    Helper.SetLongMid(ref rawContent, wordNumber + 2, value[i].Y);
                }
                NumberOfVertexes = value.Count;
                IndexToAttribute += (sizeDelta / 2);
                vertexEndAddress = (18 - 1) * 2 + value.Count * 8;
                attBytes.CopyTo(rawContent, vertexEndAddress);
            }
        }


        public ImmutableList<DPoint2d> Vertex
        {
            get
            {
                return ImmutableList<DPoint2d>.Empty.AddRange(
                    VertexRaw.Select(p => 
                    new DPoint2d(
                        p.X / scale, 
                        p.Y / scale)
                    )
                );
            }
            set
            {
                VertexRaw = ImmutableList<Point2d>.Empty.AddRange(
                    value.Select(p => 
                    new Point2d(
                        (int)Math.Round(p.X * scale), 
                        (int)Math.Round(p.Y * scale))
                    )
                );
            }
        }

        public bool IsFilled
        {
            get
            {
                return AttributeList.Any(x => x.Id == 0x0041);
            }
            set
            {
                if(value && !IsFilled)
                {
                    Fill = 0;
                }
                if(!value && IsFilled)
                {
                    var al = AttributeList.ToList();
                    al.RemoveAll(x => x.Id == 0x41);
                    AttributeList = ImmutableList<DgnAttribute>.Empty.AddRange(al);
                }
            }
        }

        public int Fill
        {
            get
            {
                var att = AttributeList.FirstOrDefault(x => x.Id == 0x0041);
                if (att != null)
                {
                    return att.Content[4];
                }
                return -1;
            }
            set
            {
                var al = AttributeList.ToList();
                var att = al.FirstOrDefault(x => x.Id == 0x0041);
                if (att != null)
                {
                    att.Content[4] = (byte)value;
                }
                else
                {
                    att = new DgnAttribute();
                    att.Header = 0x1007;
                    att.Id = 0x0041;
                    att.Content = new byte[] { 0, 0, 0, 0, (byte)value, 0, 0, 0, 0, 0, 0, 0 };
                    al.Add(att);
                }
                AttributeList = ImmutableList<DgnAttribute>.Empty.AddRange(al);                
            }
        }

        public new ShapeElement Copy()
        {
            return new ShapeElement(this);
        }

        public override void Translate(double xOffset, double yOffset)
        {
            var vertices = Vertex.ToList();
            
            for(int i=0;i<vertices.Count;i++)
            {
                vertices[i] = new DPoint2d(vertices[i].X + xOffset, vertices[i].Y + yOffset);
            }
            Vertex = vertices.ToImmutableList();

            Range = new DRange(
                new DPoint3d(
                    vertices.Select(p => p.X).Min(),
                    vertices.Select(p => p.Y).Min(),
                    0),
                new DPoint3d(
                    vertices.Select(p => p.X).Max(),
                    vertices.Select(p => p.Y).Max(),
                    0)
                );
        }
    }
}
