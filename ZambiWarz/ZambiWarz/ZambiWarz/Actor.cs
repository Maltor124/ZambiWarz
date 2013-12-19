using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ZambiWarz
{
    class Actor
    {
        public float Direction = 0f;
        private float turnSpeed = 0.7f, speed = 1f;
        public Vector2 Location;
        private Texture2D texture;

        public Stack<Vector2> path;

        public Actor(Texture2D texture, Vector2 location)
        {
            this.Location = location;
            this.texture = texture;
        }

        public void Update(float delta)
        {
            if(path.Count != 0)
            {
                Direction = (float)Math.Atan2(path.Peek().Y - Location.Y, path.Peek().X - Location.X); //MathHelper.Lerp(Direction, (float)Math.Atan2(location.Y - path.Peek().Y, location.X - path.Peek().X), turnSpeed);
                Location += speed * new Vector2((float)Math.Cos(Direction), (float)Math.Sin(Direction));

                if((Location - path.Peek()).Length() < 2) 
                    path.Pop();
            }
        }

        public void Draw(SpriteBatch batch)
        {
            batch.Draw(texture, Location, null, Color.Green, Direction, new Vector2(texture.Width / 2, texture.Height / 2), 4f, SpriteEffects.None, 0); 
        }
    }
}
