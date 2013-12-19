using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace ZambiWarz
{
    /// <summary>
    /// Vertices belonging to the convex hull need to maintain a point and triad index
    /// </summary>
    internal class HullVertex
    {
        public int pointsIndex;
        public int triadIndex;
        public float X, Y;

        public HullVertex(List<Vector2> points, int pointIndex)
        {
            X = points[pointIndex].X;
            Y = points[pointIndex].Y;
            pointsIndex = pointIndex;
            triadIndex = 0;
        }


    }

    /// <summary>
    /// Hull represents a list of vertices in the convex hull, and keeps track of
    /// their indices (into the associated points list) and triads
    /// </summary>
    class Hull : List<HullVertex>
    {
        private int NextIndex(int index)
        {
            if (index == Count - 1)
                return 0;
            else
                return index + 1;
        }

        /// <summary>
        /// Return vector from the hull point at index to next point
        /// </summary>
        public void VectorToNext(int index, out float dx, out float dy)
        {
            HullVertex et = this[index], en = this[NextIndex(index)];

            dx = en.X - et.X;
            dy = en.Y - et.Y;
        }

        /// <summary>
        /// Return whether the hull Vector2 at index is visible from the supplied coordinates
        /// </summary>
        public bool EdgeVisibleFrom(int index, float dx, float dy)
        {
            float idx, idy;
            VectorToNext(index, out idx, out idy);

            float crossProduct = -dy * idx + dx * idy;
            return crossProduct < 0;
        }

        /// <summary>
        /// Return whether the hull Vector2 at index is visible from the point
        /// </summary>
        public bool EdgeVisibleFrom(int index, Vector2 point)
        {
            float idx, idy;
            VectorToNext(index, out idx, out idy);

            float dx = point.X - this[index].X;
            float dy = point.Y - this[index].Y;

            float crossProduct = -dy * idx + dx * idy;
            return crossProduct < 0;
        }
    }
}
