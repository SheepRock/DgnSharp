using System;
using System.Collections.Generic;
using System.Text;

namespace DgnSharp
{
    public class TCBElement : Element
    {
        public TCBElement()
        {
        }

        public TCBElement(Element element)
        {
            Header = element.Header;
            RawContent = new byte[element.RawContent.Length];
            element.RawContent.CopyTo(RawContent, 0);
        }

        public int MasterUnitsPerDesign
        {
            get
            {
                return Helper.GetLongFromByteIndex(RawContent, 1104);
            }
            set
            {
                Helper.SetLongFromByteIndex(ref rawContent, 1104, value);
            }
        }

        public int UnitsOfResolutionPerSubunit
        {
            get
            {
                return Helper.GetLongFromByteIndex(RawContent, 1108);
            }
            set
            {
                Helper.SetLongFromByteIndex(ref rawContent, 1108, value);
            }
        }
        public int SubunitsPerMasterUnit
        {
            get
            {
                return Helper.GetLongFromByteIndex(RawContent, 1112);
            }
            set
            {
                Helper.SetLongFromByteIndex(ref rawContent, 1112, value);
            }
        }

        public bool Is3D
        {
            get
            {
                return (RawContent[1210] & 0x40) > 0;
            }
            set
            {
                if (value)
                {
                    RawContent[1210] |= 0x40;
                }
                else
                {
                    RawContent[1210] &= 0xBF;
                }
            }
        }

        public double GlobalXOrigin
        {
            get
            {
                return Helper.GetDoubleFromByteIndex(RawContent, 1236);
            }
            set
            {
                Helper.SetDoubleFromByteIndex(ref rawContent, 1236, value);
            }
        }
        public double GlobalYOrigin
        {
            get
            {
                return Helper.GetDoubleFromByteIndex(RawContent, 1244);
            }
            set
            {
                Helper.SetDoubleFromByteIndex(ref rawContent, 1244, value);
            }
        }
        public double GlobalZOrigin
        {
            get
            {
                return Helper.GetDoubleFromByteIndex(RawContent, 1252);
            }
            set
            {
                Helper.SetDoubleFromByteIndex(ref rawContent, 1252, value);
            }
        }

        public string SubunitsName
        {
            get
            {
                return Encoding.GetEncoding(1252).GetString(RawContent, 1116, 2);
            }
            set
            {
                Encoding.GetEncoding(1252).GetBytes(value.ToCharArray(), 0, 2, RawContent, 1116);
            }
        }

        public string MasterUnitsName
        {
            get
            {
                return Encoding.GetEncoding(1252).GetString(RawContent, 1118, 2);
            }
            set
            {
                Encoding.GetEncoding(1252).GetBytes(value.ToCharArray(), 0, 2, RawContent, 1118);
            }
        }

        public ViewInfo[] Views
        {
            get
            {
                ViewInfo[] views = new ViewInfo[9];
                for(int i = 0; i < 9; i++)
                {
                    int startByte = 46 - 4 + i * 118;
                    ViewInfo vi = new ViewInfo()
                    {
                        Flags = Helper.GetWordFromByteIndex(RawContent, startByte),
                        EnabledLevels = BitConverter.ToUInt64(RawContent, startByte + 2),
                        XOrigin = Helper.GetLongFromByteIndex(RawContent, startByte + 10),
                        YOrigin = Helper.GetLongFromByteIndex(RawContent, startByte + 14),
                        ZOrigin = Helper.GetLongFromByteIndex(RawContent, startByte + 18),
                        ViewWidth = Helper.GetLongFromByteIndex(RawContent, startByte + 22),
                        ViewHeight = Helper.GetLongFromByteIndex(RawContent, startByte + 26),
                        ViewDepth = Helper.GetLongFromByteIndex(RawContent, startByte + 30),
                        Conversion = BitConverter.ToUInt64(RawContent, startByte + 106),
                        Activez = Helper.GetLongFromByteIndex(RawContent, startByte + 114)
                    };
                    for (int j = 0; j < 9; j++)
                    {
                        vi.TransformationMatrix[j] = Helper.GetDoubleFromByteIndex(RawContent, startByte + 34 + j * 8);
                    }
                    views[i] = vi;
                }
                return views;
                
            }
            set
            {

            }
        }

        public new TCBElement Copy()
        {
            TCBElement newElement = new TCBElement(this);
            return newElement;
        }
    }
}
