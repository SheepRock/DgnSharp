using System;
using System.Collections.Generic;
using System.Text;

namespace DgnSharp
{
    public class Range
    {

        public Point3d Minimum { get; }
        public Point3d Maximum { get; }

        public Range()
        {
            Minimum = new Point3d();
            Maximum = new Point3d();
        }
        public Range(Point3d minimum, Point3d maximum)
        {
            Minimum = minimum;
            Maximum = maximum;
        }
    }

    public class DRange
    {

        public DPoint3d Minimum { get; }
        public DPoint3d Maximum { get; }

        public DRange()
        {
            Minimum = new DPoint3d();
            Maximum = new DPoint3d();
        }
        public DRange(DPoint3d minimum, DPoint3d maximum)
        {
            Minimum = minimum;
            Maximum = maximum;
        }
    }

    public struct Point2d
    {
        public Point2d(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; }
        public int Y { get; }
    }

    public struct Point3d
    {
        public Point3d(uint x, uint y, uint z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public uint X { get; }
        public uint Y { get; }
        public uint Z { get; }
    }

    public struct DPoint2d
    {
        public DPoint2d(double x, double y)
        {
            X = x;
            Y = y;
        }

        public double X { get; }
        public double Y { get; }
    }

    public struct DPoint3d
    {
        public DPoint3d(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public double X { get; }
        public double Y { get; }
        public double Z { get; }
    }
}
