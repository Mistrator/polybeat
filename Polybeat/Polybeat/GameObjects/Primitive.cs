using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using C3.XNA;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Polybeat
{
    public class Primitive : IDrawable
    {
        public PrimitiveType Type;

        /// <summary>
        /// Vertices counter-clockwise.
        /// </summary>
        public List<Vector2> Vertices;

        public Color Color;

        public bool IsVisible;

        public float Thickness;

        public Vector2 Position
        {
            get 
            {
                if (Vertices.Count == 0)
                    return Vector2.Zero;

                return GetPosition();
            }
        }

        public Primitive(PrimitiveType type, List<Vector2> vertices, Color color, float thickness)
	    {
            Type = type;
            Vertices = vertices;
            Color = color;
            IsVisible = true;
            Thickness = thickness;
	    }

        /// <summary>
        /// Keskikohta, ääripäiden keskiarvo.
        /// </summary>
        /// <returns></returns>
        private Vector2 GetPosition()
        {
            float largestX = float.MinValue;
            float smallestX = float.MaxValue;

            float largestY = float.MinValue;
            float smallestY = float.MaxValue;

            for (int i = 0; i < Vertices.Count; i++)
            {
                if (Vertices[i].X > largestX)
                    largestX = Vertices[i].X;
                if (Vertices[i].X < smallestX)
                    smallestX = Vertices[i].X;

                if (Vertices[i].Y > largestY)
                    largestY = Vertices[i].Y;
                if (Vertices[i].Y < smallestY)
                    smallestY = Vertices[i].Y;
            }

            return new Vector2((largestX + smallestX) / 2.0f, (largestY + smallestY) / 2.0f);
        }

        public void Draw(SpriteBatch batch)
        {
            if (!IsVisible) return;
            if (Vertices.Count == 0) return;

            switch (Type)
            {
                case PrimitiveType.Pixel:
                    batch.PutPixel(Vertices[0], Color);
                    break;
                case PrimitiveType.Line:
                    batch.DrawLine(Vertices[0], Vertices[1], Color, Thickness);
                    break;
            }
        }
    }
}