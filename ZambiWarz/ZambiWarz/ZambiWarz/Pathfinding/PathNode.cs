﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace ZambiWarz
{
    class PathNode
    {
        public Vector2 Location;
        public PathNode Parent;
        public float Cost;

        public PathNode(Vector2 location, PathNode parent)
        {
            this.Location = location;
            this.Parent = parent;
            if (parent == null) this.Cost = 0;
            else this.Cost = Vector2.Distance(location, parent.Location) + parent.Cost;
        }
    }
}
