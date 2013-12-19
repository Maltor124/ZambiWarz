using System;
using Microsoft.Xna.Framework;

namespace ZambiWarzMono
{
    class Edge : IComparable
    {
        public Vector2 Start, End;

        public Edge(Vector2 start, Vector2 end)
        {
            this.Start = start;
            this.End = end;
        }

        public Vector2 MidPoint
        {
            get
            {
                return Vector2.Lerp(Start, End, 0.5f);
            }
        }

        public override bool Equals(object obj)
        {
            Edge that = (Edge)obj;
            //return Start.Equals(that.Start) && End.Equals(that.End) || Start.Equals(that.End) && End.Equals(that.Start);
            //return Start == that.Start && End == that.End || Start == that.End && End == that.Start;
            return Math.Abs(Start.X - that.Start.X) <= 2 && Math.Abs(Start.Y - that.Start.Y) <= 2 &&
                Math.Abs(End.X - that.End.X) <= 2 && Math.Abs(End.Y - that.End.Y) <= 2 ||
                Math.Abs(Start.X - that.End.X) <= 2 && Math.Abs(Start.Y - that.End.Y) <= 2 &&
                Math.Abs(End.X - that.Start.X) <= 2 && Math.Abs(End.Y - that.Start.Y) <= 2;
        }

        public int CompareTo(object obj)
        {
            Edge that = (Edge)obj;
            return Start.X.CompareTo(that.Start.X);
        }
    }
}
