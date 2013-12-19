using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace ZambiWarzMono
{
    class QuadTree
    {
        public static readonly ushort NODE_CAPACITY = 8;

        private QuadTree nw, ne, sw, se;
        public readonly Rectangle bounds;
        private Vector2[] elements;
        private uint size;

        public QuadTree NorthWest
        {
            get { return nw; }
        }
        public QuadTree NorthEast
        {
            get { return ne; }
        }
        public QuadTree SouthWest
        {
            get { return sw; }
        }
        public QuadTree SouthEast
        {
            get { return se; }
        }
        public QuadTree[] Quadrants
        {
            get { return new QuadTree[] { nw, ne, se, sw }; }
        }

        public QuadTree(int x, int y, int width, int height)
        {
            bounds = new Rectangle(x - 1, y - 1, width + 2, height + 2);
            elements = new Vector2[NODE_CAPACITY];
        }

        public bool Insert(Vector2 element)
        {
            if (!bounds.Contains((int)element.X, (int)element.Y))
                return false;
            else if (size < NODE_CAPACITY)
            {
                elements[size++] = element;
                return true;
            }
            else
            {
                if (nw == null) Subdivide();
                return nw.Insert(element) || ne.Insert(element) || se.Insert(element) || sw.Insert(element);
            }
        }

        public List<Vector2> QueryRange(Rectangle range)
        {
            List<Vector2> points = new List<Vector2>();

            if (!bounds.Intersects(range))
                return points;

            foreach (Vector2 v in elements)
                if (range.Contains((int)v.X, (int)v.Y)) 
                    points.Add(v);

            if (nw == null)
                return points;

            foreach (var quadrant in Quadrants)
                points.AddRange(quadrant.QueryRange(range));

            return points;
        }

        /// <summary>
        /// Finds the nearest existing point in the tree to the given point.
        /// </summary>
        /// <param name="seed"></param>
        /// <returns></returns>
        public Vector2 FindNearest(Vector2 seed)
        {
            Point p_seed = new Point((int)seed.X, (int)seed.Y);

            if (!bounds.Contains(p_seed)) throw new Exception("Input point was not within this QuadTree's range!");

            if (nw == null)
            {
                float min_dist = float.PositiveInfinity;
                Vector2 closestPoint = new Vector2(float.NegativeInfinity);
                foreach (Vector2 v in elements)
                {
                    float dist = Vector2.Distance(seed, v);
                    if (dist < min_dist)
                    {
                        min_dist = dist;
                        closestPoint = v;
                    }
                }

                return closestPoint;
            }


            foreach (var quadrant in Quadrants)
                if (quadrant.bounds.Contains(p_seed))
                    return quadrant.FindNearest(seed);

            throw new Exception("Input point was not valid!");
        }

        public bool Contains(Vector2 point)
        {
            Point p_point = new Point((int)point.X, (int)point.Y);

            if (!bounds.Contains(p_point)) return false;

            if (nw == null)
                foreach (Vector2 v in elements)
                    if (v.Equals(point))
                        return true;


            foreach (var quadrant in Quadrants)
                if (quadrant.bounds.Contains(p_point))
                    return quadrant.Contains(point);

            return false;
        }

        private void Subdivide()
        {
            ne = new QuadTree(bounds.X, bounds.Y, bounds.Width / 2, bounds.Height / 2);
            nw = new QuadTree(bounds.X + bounds.Width / 2, bounds.Y, bounds.Width / 2, bounds.Height / 2);
            se = new QuadTree(bounds.X, bounds.Y + bounds.Height / 2, bounds.Width / 2, bounds.Height / 2);
            sw = new QuadTree(bounds.X + bounds.Width / 2, bounds.Y + bounds.Height / 2, bounds.Width / 2, bounds.Height / 2);

            List<Vector2> others = new List<Vector2>();
            foreach (Vector2 t in elements)
                if (!(nw.Insert(t) || ne.Insert(t) || se.Insert(t) || sw.Insert(t)))
                    others.Add(t);

            elements = others.ToArray();
        }
    }
}
