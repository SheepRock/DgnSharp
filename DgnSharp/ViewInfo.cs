using System;
using System.Collections.Generic;
using System.Text;

namespace DgnSharp
{
    public class ViewInfo
    {
        public ViewInfo()
        {
            TransformationMatrix = new double[9];
        }

        public int Flags { get; set; }
        public ulong EnabledLevels { get; set; }
        public int XOrigin { get; set; }
        public int YOrigin { get; set; }
        public int ZOrigin { get; set; }
        public int ViewWidth { get; set; }
        public int ViewHeight { get; set; }
        public int ViewDepth { get; set; }
        public double[] TransformationMatrix { get; set; }
        public double Conversion { get; set; }
        public int Activez { get; set; }
    }
}
