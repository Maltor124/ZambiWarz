using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;

namespace ZambiWarzMono
{
    public class World
    {
        private readonly static Random r = new Random();
        private readonly int width, height;

        private RenderTarget2D map;
        private Texture2D unitRect;
        private RotatedRectangle[] obstacles;
        private RotatedRectangle player;
        private Color dc = Color.White;
        private Mesh mesh;
        private List<Actor> actors;

        MouseState oldMs;

        public World(int width, int height)
        {
            this.width = width;
            this.height = height;
        }

        public void Generate(GraphicsDevice device, SpriteBatch batch, int numHouses)
        {
            actors = new List<Actor>();
            Stopwatch s = Stopwatch.StartNew();
            Debug.WriteLine("Beginning world generation...");
            oldMs = Mouse.GetState();
            player = new RotatedRectangle(new Rectangle(0, 0, 5, 5), 0);

            map = new RenderTarget2D(device, width, height);
            obstacles = new RotatedRectangle[numHouses];

            unitRect = new Texture2D(device, 1, 1);
            unitRect.SetData(new[] { Color.White });

            device.SetRenderTarget(map);
            device.Clear(Color.Transparent);
            long time0 = s.ElapsedMilliseconds;
            Debug.WriteLine("Initial setup completed in {0} milliseconds. Placing obstacles...", time0);

            batch.Begin();
            for (int n = 0; n < numHouses; ++n)
            {
                int x = r.Next(width), y = r.Next(height);
                float rotation = (float)(Math.PI * r.NextDouble());
                int w = r.Next(10, 25), h = r.Next(10, 25);

                RotatedRectangle rr = new RotatedRectangle(new Rectangle(x, y, w, h), rotation);
                obstacles[n] = rr;

                batch.Draw(unitRect,
                    rr.UpperLeftCorner,
                    null,
                    Color.Chocolate,
                    rr.Rotation,
                    Vector2.Zero,
                    new Vector2(w, h),
                    SpriteEffects.None,
                    0);
            }
            Array.Sort<RotatedRectangle>(obstacles);
            long time1 = s.ElapsedMilliseconds - time0;
            Debug.WriteLine("Obstacles placed and sorted in {0} milliseconds. Generating vertices...", time1);
            time0 = s.ElapsedMilliseconds;

            List<Vector2> corners = new List<Vector2>();
            corners.Add(new Vector2(0, 0));
            corners.Add(new Vector2(800, 600));
            corners.Add(new Vector2(800, 0));
            corners.Add(new Vector2(0, 600));

            for (int i = 0; i < obstacles.Length; ++i)
                foreach (Vector2 v in obstacles[i].Corners)
                {
                    bool f = true;
                    for (int j = 0; j < obstacles.Length; ++j)
                        if (j != i && obstacles[j].Contains(v))
                        {
                            f = false;
                            break;
                        }
                    if (f)
                    {
                        corners.Add(v);
                        batch.Draw(unitRect, new Rectangle((int)v.X - 1, (int)v.Y - 1, 2, 2), Color.Black);
                    }
                }

            time1 = s.ElapsedMilliseconds - time0;
            Debug.WriteLine("Vertices generated in {0} milliseconds. Generating Delaunay Triangulation...", time1);
            time0 = s.ElapsedMilliseconds;

            Triangulator t = new Triangulator();
            List<Triad> tris = t.Triangulation(corners);

            time1 = s.ElapsedMilliseconds - time0;
            Debug.WriteLine("Triangulation generated in {0} milliseconds. Removing triads that intersect obstacles...", time1);
            time0 = s.ElapsedMilliseconds;

            for (int i = tris.Count - 1; i >= 0; --i)
            {
                Vector2 a = corners[tris[i].a], b = corners[tris[i].b], c = corners[tris[i].c];
                if (LineInsideRectangle(a, b) || LineInsideRectangle(a, c) || LineInsideRectangle(b, c))
                    tris.RemoveAt(i);
            }

            time1 = s.ElapsedMilliseconds - time0;
            Debug.WriteLine("Triads verified in {0} milliseconds. Generating navigation mesh...", time1);
            time0 = s.ElapsedMilliseconds;

            mesh = new Mesh(corners, tris);
            mesh.DrawWireframe(batch);

            time1 = s.ElapsedMilliseconds - time0;
            Debug.WriteLine("Generation complete. Took {1} milliseconds", time1, s.ElapsedMilliseconds);
            s.Reset();

            batch.End();
            device.SetRenderTarget(null);
        }

        private bool LineInsideRectangle(Vector2 a, Vector2 b)
        {
            foreach (RotatedRectangle r in obstacles)
                if (r.PointsNonColinearAndInRectangle(a, b) || !r.PointsColinearAndInRectangle(a, b) && r.Contains(Vector2.Lerp(a, b, 0.5f)))
                    return true;

            return false;
        }

        public void Update(GameTime gameTime)
        {
            foreach (Actor a in actors)
                a.Update(gameTime.ElapsedGameTime.Milliseconds);
        }

        public void Draw(SpriteBatch batch)
        {
            batch.Draw(map, Vector2.Zero, dc);
            //batch.Draw(unitRect, player.UpperLeftCorner, null, dc, player.Rotation, Vector2.Zero, player.Dimensions, SpriteEffects.None, 0);

            if (Mouse.GetState().LeftButton == ButtonState.Pressed)
            {
                Actor a;
                switch (r.Next(4))
                {
                    case 0: a = new Actor(unitRect, new Vector2(r.Next(width), 0)); break;
                    case 1: a = new Actor(unitRect, new Vector2(width, r.Next(height))); break;
                    case 2: a = new Actor(unitRect, new Vector2(r.Next(width), height)); break;
                    default: a = new Actor(unitRect, new Vector2(0, r.Next(height))); break;
                }
                actors.Add(a);

                a.path = mesh.GetPath(mesh.GetNearestVertex(a.Location), new Vector2(Mouse.GetState().X, Mouse.GetState().Y));
            }

            foreach (Actor a in actors)
                a.Draw(batch);

            dc = Color.White;
        }

        private void DrawLine(SpriteBatch batch, Texture2D rect, Vector2 a, Vector2 b)
        {
            float angle = (float)Math.Atan2(b.Y - a.Y, b.X - a.X);
            float length = Vector2.Distance(a, b);

            batch.Draw(rect, a, null, Color.Purple, angle, Vector2.Zero, new Vector2(length, 1), SpriteEffects.None, 0);
        }
    }
}
