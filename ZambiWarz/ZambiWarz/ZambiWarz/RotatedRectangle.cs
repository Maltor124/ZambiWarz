using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace ZambiWarz
{
    class RotatedRectangle : IComparable
    {
        public Rectangle CollisionRectangle;
        public float Rotation;
        public Vector2 Origin;

        private float? urx, ury, ulx, uly, lrx, lry, llx, lly;

        public RotatedRectangle(Rectangle rect, float rotation)
        {
            CollisionRectangle = rect;
            Rotation = rotation;

            //Calculate the Rectangles origin. We assume the center of the Rectangle will
            //be the point that we will be rotating around and we use that for the origin
            Origin = new Vector2(rect.Width / 2, rect.Height / 2);
        }

        /// <summary>
        /// Used for changing the X and Y position of the RotatedRectangle
        /// </summary>
        /// <param name="xOffset"></param>
        /// <param name="yOffset"></param>
        public void Offset(int xOffset, int yOffset)
        {
            CollisionRectangle.X += xOffset;
            CollisionRectangle.Y += yOffset;
        }

        /// <summary>
        /// This intersects method can be used to check a standard XNA framework Rectangle
        /// object and see if it collides with a Rotated Rectangle object
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        public bool Intersects(Rectangle rect)
        {
            return Intersects(new RotatedRectangle(rect, 0.0f));
        }

        /// <summary>
        /// Check to see if two Rotated Rectangles have collided
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        public bool Intersects(RotatedRectangle rect)
        {
            //Calculate the Axis we will use to determine if a collision has occurred
            //Since the objects are rectangles, we only have to generate 4 Axis (2 for
            //each rectangle) since we know the other 2 on a rectangle are parallel.
            List<Vector2> rectAxes = new List<Vector2>();
            rectAxes.Add(UpperRightCorner - UpperLeftCorner);
            rectAxes.Add(UpperRightCorner - LowerRightCorner);
            rectAxes.Add(rect.UpperLeftCorner - rect.LowerLeftCorner);
            rectAxes.Add(rect.UpperLeftCorner - rect.UpperRightCorner);

            //Cycle through all of the Axis we need to check. If a collision does not occur
            //on ALL of the Axis, then a collision is NOT occurring. We can then exit out 
            //immediately and notify the calling function that no collision was detected. If
            //a collision DOES occur on ALL of the Axis, then there is a collision occurring
            //between the rotated rectangles. We know this to be true by the Seperating Axis Theorem
            foreach (Vector2 axis in rectAxes)
                if (!IsAxisCollision(rect, axis))
                    return false;

            return true;

            //return rect.Contains(UpperLeftCorner) || rect.Contains(UpperRightCorner)
            //    || rect.Contains(LowerLeftCorner) || rect.Contains(LowerRightCorner)
            //    || Contains(rect.UpperLeftCorner) || Contains(rect.UpperRightCorner)
            //    || Contains(rect.LowerLeftCorner) || Contains(rect.LowerRightCorner);
        }

        /// <summary>
        /// Determines if a collision has occurred on an Axis of one of the
        /// planes parallel to the Rectangle
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="axis"></param>
        /// <returns></returns>
        private bool IsAxisCollision(RotatedRectangle rect, Vector2 axis)
        {
            //Project the corners of the Rectangle we are checking on to the Axis and
            //get a scalar value of that project we can then use for comparison
            List<int> rectAScalars = new List<int>();
            foreach (Vector2 a in rect.Corners)
                rectAScalars.Add(GenerateScalar(a, axis));

            //Project the corners of the current Rectangle on to the Axis and
            //get a scalar value of that project we can then use for comparison
            List<int> rectBScalars = new List<int>();
            foreach (Vector2 b in Corners)
                rectBScalars.Add(GenerateScalar(b, axis));

            //Get the Maximum and Minium Scalar values for each of the Rectangles
            int rectAMin = rectAScalars.Min();
            int rectAMax = rectAScalars.Max();
            int rectBMin = rectBScalars.Min();
            int rectBMax = rectBScalars.Max();

            //If we have overlaps between the Rectangles (i.e. Min of B is less than Max of A)
            //then we are detecting a collision between the rectangles on this Axis
            return rectBMin <= rectAMax && rectBMax >= rectAMax || rectAMin <= rectBMax && rectAMax >= rectBMax;
        }

        /// <summary>
        /// Generates a scalar value that can be used to compare where corners of 
        /// a rectangle have been projected onto a particular axis. 
        /// </summary>
        /// <param name="rectCorner"></param>
        /// <param name="axis"></param>
        /// <returns></returns>
        private int GenerateScalar(Vector2 rectCorner, Vector2 axis)
        {
            //Using the formula for Vector projection. Take the corner being passed in
            //and project it onto the given Axis
            float numerator = (rectCorner.X * axis.X) + (rectCorner.Y * axis.Y);
            float denominator = (axis.X * axis.X) + (axis.Y * axis.Y);
            float divisionResult = numerator / denominator;
            Vector2 cornerProjected = new Vector2(divisionResult * axis.X, divisionResult * axis.Y);

            //Now that we have our projected Vector, calculate a scalar of that projection
            //that can be used to more easily do comparisons
            float scalar = (axis.X * cornerProjected.X) + (axis.Y * cornerProjected.Y);
            return (int)scalar;
        }

        /// <summary>
        /// Determines if the given point is contained by the rectangle.
        /// </summary>
        /// <param name="point">The point to check for containment</param>
        /// <returns></returns>
        public bool Contains(Vector2 point)
        {
            float dx = point.X - X, dy = point.Y - Y;
            double length = Math.Sqrt(dx * dx + dy * dy);
            double theta = Math.Atan2(dy, dx);
            double thi = theta - Rotation;
            double u = Math.Cos(thi) * length, v = Math.Sin(thi) * length;

            return u > 0 && u < Width && v > 0 && v < Height;
        }

        public bool PointsColinearAndInRectangle(Vector2 a, Vector2 b)
        {
            return a == UpperLeftCorner && b == UpperRightCorner
                || a == UpperLeftCorner && b == LowerLeftCorner
                || a == UpperRightCorner && b == UpperLeftCorner
                || a == UpperRightCorner && b == LowerRightCorner
                || a == LowerRightCorner && b == UpperRightCorner
                || a == LowerRightCorner && b == LowerLeftCorner
                || a == LowerLeftCorner && b == UpperLeftCorner
                || a == LowerLeftCorner && b == LowerRightCorner;
        }

        public bool PointsNonColinearAndInRectangle(Vector2 a, Vector2 b)
        {
            return a == UpperLeftCorner && b == LowerRightCorner
                || a == LowerRightCorner && b == UpperLeftCorner
                || a == UpperRightCorner && b == LowerLeftCorner
                || a == LowerLeftCorner && b == UpperRightCorner;
        }

        /// <summary>
        /// Rotate a point from a given location and adjust using the Origin we
        /// are rotating around
        /// </summary>
        /// <param name="point"></param>
        /// <param name="origin"></param>
        /// <param name="rotation"></param>
        /// <returns></returns>
        private Vector2 RotatePoint(Vector2 point, Vector2 origin, float rotation)
        {
            float sin = (float)Math.Sin(rotation);
            float cos = (float)Math.Cos(rotation);
            return new Vector2(
                origin.X + (point.X - origin.X) * cos - (point.Y - origin.Y) * sin,
                origin.Y + (point.Y - origin.Y) * cos + (point.X - origin.X) * sin
                );
        }

        public Vector2 UpperLeftCorner
        {
            get
            {
                if (ulx == null || uly == null)
                {
                    Vector2 aUpperLeft = new Vector2(CollisionRectangle.Left, CollisionRectangle.Top);
                    aUpperLeft = RotatePoint(aUpperLeft, aUpperLeft + Origin, Rotation);
                    ulx = aUpperLeft.X;
                    uly = aUpperLeft.Y;
                }
                return new Vector2((float)ulx, (float)uly);
            }
        }

        public Vector2 UpperRightCorner
        {
            get
            {
                if (urx == null || ury == null)
                {
                    Vector2 aUpperRight = new Vector2(CollisionRectangle.Right, CollisionRectangle.Top);
                    aUpperRight = RotatePoint(aUpperRight, aUpperRight + new Vector2(-Origin.X, Origin.Y), Rotation);
                    urx = aUpperRight.X;
                    ury = aUpperRight.Y;
                }
                return new Vector2((float)urx, (float)ury);
            }
        }

        public Vector2 LowerLeftCorner
        {
            get
            {
                if (llx == null || lly == null)
                {
                    Vector2 aLowerLeft = new Vector2(CollisionRectangle.Left, CollisionRectangle.Bottom);
                    aLowerLeft = RotatePoint(aLowerLeft, aLowerLeft + new Vector2(Origin.X, -Origin.Y), Rotation);
                    llx = aLowerLeft.X;
                    lly = aLowerLeft.Y;
                }
                return new Vector2((float)llx, (float)lly);
            }
        }

        public Vector2 LowerRightCorner
        {
            get
            {
                if (lrx == null || lry == null)
                {
                    Vector2 aLowerRight = new Vector2(CollisionRectangle.Right, CollisionRectangle.Bottom);
                    aLowerRight = RotatePoint(aLowerRight, aLowerRight + new Vector2(-Origin.X, -Origin.Y), Rotation);
                    lrx = aLowerRight.X;
                    lry = aLowerRight.Y;
                }
                return new Vector2((float)lrx, (float)lry);
            }
        }

        /// <summary>
        /// The four corners in the rectangle, starting with the the upper left corner and proceeding clockwise
        /// </summary>
        public Vector2[] Corners
        {
            get { return new Vector2[] { UpperLeftCorner, UpperRightCorner, LowerRightCorner, LowerLeftCorner }; }
        }

        public int X
        {
            get { return (int)UpperLeftCorner.X; }
            set 
            { 
                CollisionRectangle.X = value;
                urx = ulx = lrx = llx = null;
            }
        }

        public int Y
        {
            get { return (int)UpperLeftCorner.Y; }
            set 
            { 
                CollisionRectangle.Y = value;
                ury = uly = lry = lly = null;
            }
        }

        public int Width
        {
            get { return CollisionRectangle.Width; }
        }

        public int Height
        {
            get { return CollisionRectangle.Height; }
        }

        public Vector2 Dimensions
        {
            get { return new Vector2(CollisionRectangle.Width, CollisionRectangle.Height); }
        }

        public int CompareTo(object obj)
        {
            RotatedRectangle that = (RotatedRectangle)obj;
            if (X.CompareTo(that.X) == 0) return Y.CompareTo(that.Y);
            else return X.CompareTo(that.X);
        }
    }
}
