using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using C3.XNA;
using System;

namespace Polybeat
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Main : Game
    {
        #region attributes

        #region general

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        /// <summary>
        /// Ikkunan leveys pikseleinä
        /// </summary>
        int WindowWidth
        {
            get { return graphics.PreferredBackBufferWidth; } 
            set 
            {
                if (value > 0)
                    SetWindowSize(value, WindowHeight);
            }
        }

        /// <summary>
        /// Ikkunan korkeus pikseleinä
        /// </summary>
        int WindowHeight
        {
            get { return graphics.PreferredBackBufferHeight; }
            set 
            {
                if (value > 0)
                    SetWindowSize(WindowWidth, value);
            }
        }

        /// <summary>
        /// Ikkunan keskipiste pikseleinä
        /// </summary>
        Vector2 WindowCenter
        {
            get
            {
                return new Vector2(WindowWidth / 2, WindowHeight / 2);
            }
        }

        bool IsFullScreen
        {
            get { return graphics.IsFullScreen; }
            set
            { 
                graphics.IsFullScreen = value;
                graphics.ApplyChanges();
            }
        }

        public static readonly string PROGRAM_ROOT_DIRECTORY = AppDomain.CurrentDomain.BaseDirectory;

        #endregion

        #region gameplay

        Polygon CentralPolygon;

        // CP = Central polygon
        const int CPMinSides = 3;
        const int CPMaxSides = 21;

        const float CPMinRadius = 1;
        float CPMaxRadius;

        // SP = Shard polygon
        const int SPMinSides = 3;
        const int SPMaxSides = 15;

        const float SPMinRadius = 8;
        float SPMaxRadius;

        const int SPMinSkill = 2;
        const int SPMaxSkill = 10;

        const float SPMinSpeed = 200.0f;
        const float SPMaxSpeed = 600.0f;

        /// <summary>
        /// Kappaleen aikana kerätyt kokonaispisteet.
        /// Ei voi vähentyä.
        /// </summary>
        int Score = 0;

        private float skill = STARTING_SKILL;
        /// <summary>
        /// "Health", miten hyvin pelaaja pelaa, 
        /// kasvaa onnistuessa ja vähenee epäonnistuessa.
        /// Jos menee minimiin, peli päättyy.
        /// </summary>
        float Skill
        {
            get { return skill; }
            set
            {
                skill = value;

                if (value < MIN_SKILL)
                    skill = MIN_SKILL;
                if (value > MAX_SKILL)
                    skill = MAX_SKILL;

                skillChanged = true;
            }
        }
        bool skillChanged = false;

        const float MIN_SKILL = 0f;
        const float STARTING_SKILL = 20f;
        const float MAX_SKILL = 100f;

        float difficulty = DEFAULT_DIFFICULTY;

        /// <summary>
        /// Vaikeuskerroin. Pienempi on helpompi ja suurempi vaikeampi.
        /// Oletus 1.0.
        /// </summary>
        float Difficulty
        {
            get { return difficulty; }
            set 
            {
                if (value < MIN_DIFFICULTY || value > MAX_DIFFICULTY) return;
                difficulty = value;
            }
        }

        const float DEFAULT_DIFFICULTY = 1.0f;
        const float MIN_DIFFICULTY = 0.0f;
        const float MAX_DIFFICULTY = float.MaxValue;

        const float SECONDS_TO_CLICK_ONSET = 0.3f;
        const float ONSET_CLICK_MAX_SKILL_GAIN = 3.0f;
        const float ONSET_MISS_SKILL_LOSE = 3.0f;

        /// <summary>
        /// Yleinen väri kaikille aktiivisille kappaleille.
        /// </summary>
        Color ResponsiveObjectColor = Color.CornflowerBlue;

        #endregion

        #region effects

        const byte SPClickEffectSpeed = 4;

        #endregion

        #endregion

        #region initialization

        public Main()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            MusicDataRetriever.UpdateSongDataFiles();

            Begin();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
            SongManager.Dispose();
        }
        
        #endregion

        #region game logic

        /// <summary>
        /// Aloittaa pelin.
        /// </summary>
        void Begin()
        {
            SetWindowSize(1920, 1080);
            IsFullScreen = true;
            IsMouseVisible = true;
            StartGame("");
        }

        void StartGame(string songName)
        {
            Difficulty = 1.0f;

            SongManager.FindSongs();
            SongManager.LoadSong(SongManager.Songs[0]);
            SongManager.CurrentSong.Play();

            SongManager.CurrentSong.OnsetMissed += OnsetMissed;

            CreateCentralPolygon();
        }

        void GameOver()
        {
            Exit();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            KeyboardChange keyChange = UpdateControls(gameTime);

            SongUpdate(SongManager.CurrentSong.Update(), keyChange);

            Layer.Update(gameTime);

            base.Update(gameTime);
        }

        KeyboardChange UpdateControls(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            MouseHandler.Update(gameTime);
            KeyboardChange change = KeyboardHandler.UpdateState();
            if (KeyboardHandler.IsKeyPressed(change))
                KeyPressed();

            return change;
        }

        void SongUpdate(SongState state, KeyboardChange keyChange)
        {
            if (state.IsBeat)
            {
                CreateShardPolygon(RandomGen.NextFloat(SPMinRadius, SPMaxRadius), RandomGen.NextFloat(SPMinSpeed, SPMaxSpeed));
            }

            if (state.IsOnset)
            {

            }
        }

        void KeyPressed()
        {
            Tuple<float, int> timeToOnset = SongManager.CurrentSong.TimeToClosestOnset();

            if (!SongManager.CurrentSong.IsClicked(timeToOnset.Item2))
            {
                if (timeToOnset.Item1 < SECONDS_TO_CLICK_ONSET)
                {
                    float successScaled = timeToOnset.Item1 / SECONDS_TO_CLICK_ONSET; // 0 täydellinen, 1 huonoin
                    float successFactor = 1 - successScaled;

                    Skill += ONSET_CLICK_MAX_SKILL_GAIN * successFactor * (1 / Difficulty);

                    SongManager.CurrentSong.SetClicked(timeToOnset.Item2);
                }
                else
                {
                    OnsetMissed();
                }
            }
            else
            {
                OnsetMissed();
            }
        }

        void OnsetMissed()
        {
            Skill -= ONSET_MISS_SKILL_LOSE * Difficulty;
        }

        #endregion

        #region graphics


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp);

            for (int i = 0; i < Layer.Layers.Length; i++)
            {
                Layer.Layers[i].Draw(spriteBatch);
            }

            base.Draw(gameTime);
            spriteBatch.End();
        }

        void SetWindowSize(int width, int height)
        {
            graphics.PreferredBackBufferWidth = width;
            graphics.PreferredBackBufferHeight = height;

            graphics.ApplyChanges();
        }

        #endregion

        #region central polygon

        void CreateCentralPolygon()
        {
            CentralPolygon = new Polygon(WindowCenter, 6, 50.0f, 5.0f, Color.CornflowerBlue);
            Layer.AddToDraw(CentralPolygon);
            Layer.AddToUpdate(CentralPolygon);
            CentralPolygon.Updated += UpdateCentralPolygon;

            CPMaxRadius = (WindowHeight / 2.0f) * 0.8f;
            SPMaxRadius = CPMaxRadius * 0.25f;

            CentralPolygon.MinRadius = CPMinRadius;
            CentralPolygon.MinRadiusReached += GameOver;

            skillChanged = true;
        }

        void UpdateCentralPolygon()
        {
            if (skillChanged)
            {
                float skMultiplier = Skill / MAX_SKILL;

                int sides = (int)(CPMinSides + (CPMaxSides - CPMinSides) * skMultiplier);
                float radius = CPMinRadius + (CPMaxRadius - CPMinRadius) * skMultiplier;

                CentralPolygon.EdgeCount = sides;
                CentralPolygon.Radius = radius;

                skillChanged = false;
            }

            CentralPolygon.Angle++;
        }

        #endregion

        #region shard polygons

        void CreateShardPolygon(float radius, float speed)
        {
            float rSize = radius * (1 / Difficulty);

            if (rSize < SPMinRadius)
                rSize = SPMinRadius;

            float sideMp = radius / SPMaxRadius;
            int sides = (int)(SPMinSides + (SPMaxSides - SPMinSides) * sideMp);
            float edgeThickness = 3.0f;

            Vector2 posModifier = MathHelper.FromLengthAndAngle(CentralPolygon.Radius, RandomGen.NextDouble(0, 2 * Math.PI));
            Vector2 pos = WindowCenter + posModifier;

            posModifier.Normalize();
            Vector2 vel = posModifier * speed * Difficulty;

            Polygon poly = CreatePolygon(pos, vel, radius, sides, edgeThickness, ResponsiveObjectColor, -1);

            poly.ListenMouseOn = true;
            poly.MouseLeftClick += delegate { ShardPolygonClicked(poly); };

            poly.Updated += delegate
            {
                if (CheckForOffScreen(poly))
                    ShardPolygonOffScreen(poly);
            };
        }

        Polygon CreatePolygon(Vector2 position, Vector2 velocity, float radius, int edgeCount, float edgeThickness, Color color, int layer = 0)
        {
            Polygon poly = new Polygon(position, edgeCount, radius, edgeThickness, color);
            poly.Velocity = velocity;

            Layer.AddToDraw(poly, layer);
            Layer.AddToUpdate(poly);

            return poly;
        }

        /// <summary>
        /// Onko monikulmio kokonaan poissa ruudulta.
        /// </summary>
        /// <param name="poly"></param>
        /// <returns></returns>
        bool CheckForOffScreen(Polygon poly)
        {
            if ((poly.Position.X + poly.Radius + poly.EdgeThickness / 2) < 0)
                return true;
            if ((poly.Position.X - poly.Radius - poly.EdgeThickness / 2) > WindowWidth)
                return true;
            if ((poly.Position.Y + poly.Radius + poly.EdgeThickness / 2) < 0)
                return true;
            if ((poly.Position.Y - poly.Radius - poly.EdgeThickness / 2) > WindowHeight)
                return true;

            return false;
        }

        void ShardPolygonClicked(Polygon poly)
        {
            poly.ListenMouseOn = false;

            float sizeMp = poly.Radius / (SPMaxRadius * (1 / Difficulty)); // mitä pienempi pg, sitä pienempi
            float skillGain = SPMinSkill + (SPMaxSkill - SPMinSkill) * (1 - sizeMp) * (1 / Difficulty); // pienempi pg antaa enemmän skilliä, difficulty vähentää skillgainia

            Skill += skillGain;

            poly.ClearUpdateListeners();
            poly.EdgeColor = Color.White;

            poly.Updated += delegate
            {
                poly.Radius += SPClickEffectSpeed;

                poly.EdgeColor = new Color(poly.EdgeColor.R - SPClickEffectSpeed, poly.EdgeColor.G - SPClickEffectSpeed, poly.EdgeColor.B - SPClickEffectSpeed);

                if (poly.EdgeColor.R == 0)
                {
                    Layer.Remove(poly);
                }
            };
        }

        void ShardPolygonOffScreen(Polygon poly)
        {
            poly.ListenMouseOn = false;

            float sizeMp = poly.Radius / (SPMaxRadius * (1 / Difficulty));
            float skillLoss = SPMinSkill + (SPMaxSkill - SPMinSkill) * (1 - sizeMp) * Difficulty; 

            Skill -= skillLoss;

            Layer.Remove(poly);
        }

        #endregion
    }
}