using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Polybeat
{
    /// <summary>
    /// Säännöllinen, n-sivuinen monikulmio.
    /// </summary>
    public class Polygon : IDrawable, IUpdatable
    {
        private List<Primitive> edges;
        private List<Vector2> vertices;

        private int edgeCount;
        /// <summary>
        /// Kulmien ja sivujen määrä.
        /// </summary>
        public int EdgeCount
        {
            get { return edgeCount; }
            set 
            {
                if (value < 3)
                    throw new ArgumentOutOfRangeException("edgeCount", "There must be at least 3 edges.");

                edgeCount = value;
                verticesChanged = true;
            }
        }

        private Color edgeColor;
        /// <summary>
        /// Sivujen väri.
        /// </summary>
        public Color EdgeColor
        {
            get { return edgeColor; }
            set
            {
                edgeColor = value;
                edgesChanged = true;
            }
        }

        private float edgeThickness;
        /// <summary>
        /// Sivujen paksuus.
        /// </summary>
        public float EdgeThickness
        {
            get { return edgeThickness; }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("edgeThickness", "Thickness must be greater than zero.");

                edgeThickness = value;
                edgesChanged = true;
            }
        }

        private Vector2 position;

        /// <summary>
        /// Keskipiste.
        /// </summary>
        public Vector2 Position
        {
            get { return position; }
            set
            {
                position = value;
                verticesChanged = true;
            }
        }

        private Vector2 velocity;

        /// <summary>
        /// Nopeus pikseleinä sekunnissa.
        /// </summary>
        public Vector2 Velocity
        {
            get { return velocity; }
            set { velocity = value; }
        }

        private float radius;
        /// <summary>
        /// Kulmien etäisyys keskustasta.
        /// </summary>
        public float Radius
        {
            get { return radius; }
            set 
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("radius", "Radius must be greater than zero.");

                if (value <= MinRadius)
                {
                    if (MinRadiusReached != null)
                        MinRadiusReached();
                }

                radius = value;
                verticesChanged = true;
            }
        }

        private float minRadius;
        public float MinRadius
        {
            get { return minRadius; }
            set
            {
                if (value > 0)
                    minRadius = value;
            }
        }

        private double angle;

        /// <summary>
        /// Kulma asteina. Nollakulma suoraan oikealle, kasvaa vastapäivään.
        /// Yli 360 astetta on x mod 360.
        /// </summary>
        public double Angle
        {
            get { return angle; }
            set
            {
                angle = value % 360;
                verticesChanged = true;
            }
        }

        private bool verticesChanged = false;
        private bool edgesChanged = false;

        public bool ListenMouseOn = false;

        public event Action MouseLeftClick;
        public event Action MouseRightClick;

        public event Action Updated;
        public event Action MinRadiusReached;

        public Polygon(Vector2 position, int edgeCount, float radius, float edgeThickness, Color color)
        {
            edges = new List<Primitive>();
            vertices = new List<Vector2>();

            Position = position;

            // suora sijoitus backing fieldiin jotta vältetään ennenaikainen UpdateVertices
            this.EdgeCount = edgeCount;
            this.Radius = radius;
            this.EdgeColor = color;
            this.EdgeThickness = edgeThickness;

            UpdateVertices();
        }

        /// <summary>
        /// Updates vertices and edges.
        /// </summary>
        private void UpdateVertices()
        {
            // yksi sisäkulma
            float singleInnerAngleDegrees = ((edgeCount - 2) * 180) / edgeCount;

            //float angleToFirstVertex = 180 - 90 - (singleAngleDegrees / 2);

            vertices.Clear();

            for (int i = 0; i < edgeCount; i++)
            {
                // tulee tähti tietyillä edgecountin arvoilla, esim 18
                //vertices.Add(Position + MathHelper.FromLengthAndAngle(Radius, MathHelper.DegreesToRadians(angleToFirstVertex + i * singleAngleDegrees + Angle)));

                double rotationFromCenter = MathHelper.DegreesToRadians(i * (180 - singleInnerAngleDegrees));

                // vähän purkkaviritys, joskus kun verteksejä tulee liikaa, tulee pari ylimääräistä ja kulma ylittää 360 => ilmestyy viiva ympyrän poikki :/
                // ei oteta mukaan ylimääräisiä sivuja jos monikulmio on jo umpinainen => korkeilla edgecountin arvoilla se ja aito sivujen määrä eivät täsmää,
                // sivujen määrä tuolloin jo niin suuri ettei huomaa
                // rendauskin kevenee, win :D
                if (rotationFromCenter > 2 * Math.PI)
                    break;

                vertices.Add(Position + MathHelper.FromLengthAndAngle(Radius, rotationFromCenter + MathHelper.DegreesToRadians(Angle)));
            }

            verticesChanged = false;
            UpdateEdges();
        }

        /// <summary>
        /// Updates only edges.
        /// </summary>
        private void UpdateEdges()
        {
            edges.Clear();

            for (int i = 0; i < vertices.Count; i++)
            {
                List<Vector2> vs = new List<Vector2>();
                vs.Add(vertices[i]);

                if (i == vertices.Count - 1)
                {
                    vs.Add(vertices[0]);
                }
                else
                {
                    vs.Add(vertices[i + 1]);
                }

                edges.Add(new Primitive(PrimitiveType.Line, vs, EdgeColor, EdgeThickness));
            }

            edgesChanged = false;
        }

        public void Draw(SpriteBatch batch)
        {
            // update state
            if (verticesChanged)
            {
                UpdateVertices();
            }
            else if (edgesChanged)
            {
                UpdateEdges();
            }

            for (int i = 0; i < edges.Count; i++)
            {
                edges[i].Draw(batch);
            }
        }

        public void Update(GameTime time)
        {
            if (ListenMouseOn)
            {
                CheckMouse();
            }

            if (this.Velocity != Vector2.Zero)
                this.Position += (this.Velocity / 60.0f); // jaetaan, jotta velocity on px / s

            if (Updated != null)
                Updated();
        }

        public void ClearUpdateListeners()
        {
            Updated = null;
        }

        private void CheckMouse()
        {
            if (MouseHandler.LeftMouseClicked)
            {
                if (MathHelper.AreVectorsClose(this.Position, MouseHandler.MousePosition, this.Radius))
                {
                    MouseLeftClick();
                }
            }

            if (MouseHandler.RightMouseClicked)
            {
                if (MathHelper.AreVectorsClose(this.Position, MouseHandler.MousePosition, this.Radius))
                {
                    MouseRightClick();
                }
            }
        }
    }
}