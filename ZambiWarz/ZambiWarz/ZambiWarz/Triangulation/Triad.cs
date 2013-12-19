using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace ZambiWarz
{
    public class Triad
    {
        public int a, b, c;
        public int ab, bc, ac;  // adjacent edges index to neighbouring triangle.

        // Position and radius squared of circumcircle
        public float circumcircleR2, circumcircleX, circumcircleY;

        public Triad(int x, int y, int z)
        {
            a = x; b = y; c = z; ab = -1; bc = -1; ac = -1;
            circumcircleR2 = -1; //x = 0; y = 0;
        }

        public void Initialize(int a, int b, int c, int ab, int bc, int ac, List<Vector2> points)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            this.ab = ab;
            this.bc = bc;
            this.ac = ac;

            FindCircumcirclePrecisely(points);
        }

        /// <summary>
        /// If current orientation is not clockwise, swap b<->c
        /// </summary>
        public void MakeClockwise(List<Vector2> points)
        {
            float centroidX = (points[a].X + points[b].X + points[c].X) / 3.0f;
            float centroidY = (points[a].Y + points[b].Y + points[c].Y) / 3.0f;

            float dr0 = points[a].X - centroidX, dc0 = points[a].Y - centroidY;
            float dx01 = points[b].X - points[a].X, dy01 = points[b].Y - points[a].Y;

            float df = -dx01 * dc0 + dy01 * dr0;
            if (df > 0)
            {
                // Need to swap vertices b<->c and edges ab<->bc
                int t = b;
                b = c;
                c = t;

                t = ab;
                ab = ac;
                ac = t;
            }
        }

        /// <summary>
        /// Find location and radius ^2 of the circumcircle (through all 3 points)
        /// This is the most critical routine in the entire set of code.  It must
        /// be numerically stable when the points are nearly collinear.
        /// </summary>
        public bool FindCircumcirclePrecisely(List<Vector2> points)
        {
            // Use coordinates relative to point `a' of the triangle
            Vector2 pa = points[a], pb = points[b], pc = points[c];

            double xba = pb.X - pa.X;
            double yba = pb.Y - pa.Y;
            double xca = pc.X - pa.X;
            double yca = pc.Y - pa.Y;

            // Squares of lengths of the edges incident to `a'
            double balength = xba * xba + yba * yba;
            double calength = xca * xca + yca * yca;

            // Calculate the denominator of the formulae. 
            double D = xba * yca - yba * xca;
            if (D == 0)
            {
                circumcircleX = 0;
                circumcircleY = 0;
                circumcircleR2 = -1;
                return false;
            }

            double denominator = 0.5 / D;

            // Calculate offset (from pa) of circumcenter
            double xC = (yca * balength - yba * calength) * denominator;
            double yC = (xba * calength - xca * balength) * denominator;

            double radius2 = xC * xC + yC * yC;
            if ((radius2 > 1e10 * balength || radius2 > 1e10 * calength))
            {
                circumcircleX = 0;
                circumcircleY = 0;
                circumcircleR2 = -1;
                return false;
            }

            circumcircleR2 = (float)radius2;
            circumcircleX = (float)(pa.X + xC);
            circumcircleY = (float)(pa.Y + yC);

            return true;
        }

        /// <summary>
        /// Return true iff Vector2 p is inside the circumcircle of this triangle
        /// </summary>
        public bool InsideCircumcircle(Vector2 p)
        {
            float dx = circumcircleX - p.X;
            float dy = circumcircleY - p.Y;
            float r2 = dx * dx + dy * dy;
            return r2 < circumcircleR2;
        }

        /// <summary>
        /// Change any adjacent triangle index that matches fromIndex, to toIndex
        /// </summary>
        public void ChangeAdjacentIndex(int fromIndex, int toIndex)
        {
            if (ab == fromIndex)
                ab = toIndex;
            else if (bc == fromIndex)
                bc = toIndex;
            else if (ac == fromIndex)
                ac = toIndex;
        }

        /// <summary>
        /// Determine which edge matches the triangleIndex, then which Vector2 the Vector2Index
        /// Set the indices of the opposite Vector2, left and right edges accordingly
        /// </summary>
        public void FindAdjacency(int vertexIndex, int triangleIndex, out int indexOpposite, out int indexLeft, out int indexRight)
        {
            if (ab == triangleIndex)
            {
                indexOpposite = c;

                if (vertexIndex == a)
                {
                    indexLeft = ac;
                    indexRight = bc;
                }
                else
                {
                    indexLeft = bc;
                    indexRight = ac;
                }
            }
            else if (ac == triangleIndex)
            {
                indexOpposite = b;

                if (vertexIndex == a)
                {
                    indexLeft = ab;
                    indexRight = bc;
                }
                else
                {
                    indexLeft = bc;
                    indexRight = ab;
                }
            }
            else if (bc == triangleIndex)
            {
                indexOpposite = a;

                if (vertexIndex == b)
                {
                    indexLeft = ab;
                    indexRight = ac;
                }
                else
                {
                    indexLeft = ac;
                    indexRight = ab;
                }
            }
            else indexOpposite = indexLeft = indexRight = 0;
        }

        public override string ToString()
        {
            return string.Format("Triad vertices {0} {1} {2} ; edges {3} {4} {5}", a, b, c, ab, ac, bc);
        }
    }
}
