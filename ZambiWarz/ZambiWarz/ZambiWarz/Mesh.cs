using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ZambiWarz
{
    class Mesh
    {
        private HashSet<Vector2> vertices;
        private HashSet<Edge> edges;
        //HashSet<Triangle> triangles;

        public Mesh(List<Vector2> vertices, List<Triad> triads)
        {
            Stopwatch s = Stopwatch.StartNew();
            this.vertices = new HashSet<Vector2>();
            foreach (Vector2 v in vertices)
                foreach(Triad t in triads)
                    if (vertices[t.a] == v || vertices[t.b] == v || vertices[t.c] == v)
                    {
                        this.vertices.Add(v);
                        break;
                    }

            this.edges = new HashSet<Edge>(new EdgeComparer());
            foreach(Triad t in triads)
            {
                edges.Add(new Edge(vertices[t.a], vertices[t.b]));
                edges.Add(new Edge(vertices[t.a], vertices[t.c]));
                edges.Add(new Edge(vertices[t.b], vertices[t.c]));
            }
            s.Stop();
            Debug.WriteLine("Navigation mesh created in {0} milliseconds. {1} vertices | {2} edges | {3} triads", 
                s.ElapsedMilliseconds, vertices.Count, edges.Count, triads.Count);
        }

        public void DrawWireframe(SpriteBatch batch)
        {
            Texture2D rect = new Texture2D(batch.GraphicsDevice, 1, 1);
            rect.SetData(new[] { Color.White });

            foreach (Edge e in edges)
                DrawLine(batch, rect, e.Start, e.End);
        }

        private void DrawLine(SpriteBatch batch, Texture2D rect, Vector2 a, Vector2 b)
        {
            float angle = (float)Math.Atan2(b.Y - a.Y, b.X - a.X);
            float length = Vector2.Distance(a, b);

            batch.Draw(rect, a, null, Color.White, angle, Vector2.Zero, new Vector2(length, 1), SpriteEffects.None, 0);
        }

        public IList<Vector2> CalculateAdjacencies(Vector2 vertex)
        {
            if (!vertices.Contains(vertex)) throw new Exception("No matching vertex found for the input!");

            IList<Vector2> adjacents = new List<Vector2>();

            foreach (Edge e in edges)
                if (e.Start.Equals(vertex))
                    adjacents.Add(e.End);
                else if (e.End.Equals(vertex))
                    adjacents.Add(e.Start);

            return adjacents;
        }

        public Stack<Vector2> GetPath(Vector2 start, Vector2 end)
        {
            PriorityQueue<double, PathNode> open = new PriorityQueue<double, PathNode>();
            HashSet<PathNode> closed = new HashSet<PathNode>();

            open.Enqueue(0, new PathNode(start, null));

            Vector2 endVertex = GetNearestVertex(end);
            while (!open.Peek().Location.Equals(endVertex))
            {
                PathNode current = open.Dequeue();
                closed.Add(current);
                foreach (Vector2 v in CalculateAdjacencies(current.Location))
                {
                    PathNode neighbor = new PathNode(v, current);
                    float cost = current.Cost + 1;
                    if (open.Contains(neighbor) && cost < neighbor.Cost)
                        open.Remove(neighbor);
                    else if (!open.Contains(neighbor) && !closed.Contains(neighbor))
                        open.Enqueue(0.4f * neighbor.Cost + 0.6f * Vector2.Distance(neighbor.Location, end), neighbor);
                }
            }

            Stack<Vector2> path = new Stack<Vector2>();
            path.Push(end);

            PathNode node = open.Peek();
            while (node.Parent != null)
            {
                path.Push(node.Location);
                node = node.Parent;
            }

            return path;
        }

        public Vector2 GetNearestVertex(Vector2 point)
        {
            Vector2 nearest = Vector2.Zero;

            foreach (Vector2 v in vertices)
                if ((point - v).Length() < (point - nearest).Length())
                    nearest = v;

            return nearest;
        }

        internal class EdgeComparer : IEqualityComparer<Edge>
        {
            public bool Equals(Edge x, Edge y)
            {
                return x.Equals(y);
            }

            public int GetHashCode(Edge obj)
            {
                return obj.Start.GetHashCode() + obj.End.GetHashCode();
            }
        }

        public IList<Vector2> Vertices
        {
            get { return vertices.ToList().AsReadOnly(); }
        }

        public IList<Edge> Edges
        {
            get { return edges.ToList().AsReadOnly(); }
        }

        public IList<Vector2> EdgeMidpoints
        {
            get
            {
                IList<Vector2> midpoints = new List<Vector2>();

                foreach (Edge e in edges)
                    midpoints.Add(e.MidPoint);

                return midpoints.ToList().AsReadOnly();
            }
        }
    }
}
