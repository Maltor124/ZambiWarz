using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace ZambiWarz
{
    class QuadTree<T>
    {
        public static readonly ushort NODE_CAPACITY = 8;

        QuadTree<T> nw, ne, sw, se;
        Rectangle bounds;
        T[] elements;
        uint size;

        public QuadTree(int x, int y, int width, int height)
        {
            bounds = new Rectangle(x, y, width, height);
            elements = new T[NODE_CAPACITY];
        }

        public bool Insert(T element)
        {
            if (!bounds.Contains(new Point()))
                return false;
            else if (size < NODE_CAPACITY)
            {
                elements[size++] = element;
                return true;
            }
            else
            {
                if(nw == null) Subdivide();
                return nw.Insert(element) || ne.Insert(element) || se.Insert(element) || sw.Insert(element);
            }
        }

        private void Subdivide()
        {
            ne = new QuadTree<T>(bounds.X, bounds.Y, bounds.Width / 2, bounds.Height / 2);
            nw = new QuadTree<T>(bounds.X + bounds.Width / 2, bounds.Y, bounds.Width / 2, bounds.Height / 2);
            se = new QuadTree<T>(bounds.X, bounds.Y + bounds.Height / 2, bounds.Width / 2, bounds.Height / 2);
            sw = new QuadTree<T>(bounds.X + bounds.Width / 2, bounds.Y + bounds.Height / 2, bounds.Width / 2, bounds.Height / 2);
            
            List<T> others = new List<T>();
            foreach (T t in elements)
                if (!(nw.Insert(t) || ne.Insert(t) || se.Insert(t) || sw.Insert(t)))
                    others.Add(t);

            elements = others.ToArray();
        }
    }
}
