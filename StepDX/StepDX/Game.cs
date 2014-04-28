using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;


namespace StepDX
{
    public partial class Game : Form
    {

        /// <summary>
        /// The DirectX device we will draw on
        /// </summary>
        private Device device = null;

        /// <summary>
        /// Height of our playing area (meters)
        /// </summary>
        private float playingH = 4;

        /// <summary>
        /// Width of our playing area (meters)
        /// </summary>
        private float playingW = 32;

        /// <summary>
        /// Vertex buffer for our drawing
        /// </summary>
        private VertexBuffer vertices = null;

        /// <summary>
        /// The background image class
        /// </summary>
        private Background background = null;

        /// <summary>
        /// What the last time reading was
        /// </summary>
        private long lastTime;

        private int asteroidIndex = 2;

        private int playerIndex = 1;

        private Random random;

        /// <summary>
        /// ZSP
        /// The player's score for the game
        /// </summary>
        private decimal score;

        /// <summary>
        /// ZSP
        /// The player's highest score
        /// </summary>
        private decimal highScore = 0;

        /// <summary>
        /// ZSP
        /// tells us if the player has crashed
        /// </summary>
        private bool crashed = false;

        /// <summary>
        /// ZSP
        /// The sprite used for the text
        /// </summary>
        private Sprite textSprite;

        /// <summary>
        /// ZSP
        /// The font for the sprite text
        /// </summary>
        private Microsoft.DirectX.Direct3D.Font d3dFont;

        /// <summary>
        /// ZSP
        /// Another font thing
        /// </summary>
        private System.Drawing.Font gdiFont;

        /// <summary>
        /// A stopwatch to use to keep track of time
        /// </summary>
        private System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

        /// <summary>
        /// All of the polygons that make up our world
        /// </summary>
        List<Polygon> world = new List<Polygon>();

        List<Polygon> hexagons = new List<Polygon>();

        /// <summary>
        /// All of the polygons that make up our world
        /// </summary>
        List<Asteroid> asteroids = new List<Asteroid>();
        /// <summary>
        /// List of the power ups
        /// </summary>
        List<Polygon> powerUps = new List<Polygon>();

        /// <summary>
        /// Our player sprite
        /// </summary>
        GameSprite player = new GameSprite();

        /// <summary>
        /// The collision testing subsystem
        /// </summary>
        Collision collision = new Collision();

        const float gravity = (float)-9.8;

        float numFrame = 1;

        List<Background> backgrounds = new List<Background>();

        GameSounds gamesound;

        public Game()
        {
            InitializeComponent();
            if (!InitializeDirect3D())
                return;

            Polygon floor = new Polygon();
            floor.AddVertex(new Vector2(0, 0.1f));
            floor.AddVertex(new Vector2(playingW, 0.1f));
            floor.AddVertex(new Vector2(playingW, 0f));
            floor.AddVertex(new Vector2(0, 0f));
            floor.Color = Color.CornflowerBlue;
            world.Add(floor);

            Polygon ceiling = new Polygon();
            ceiling.AddVertex(new Vector2(0, 4.0f));
            ceiling.AddVertex(new Vector2(playingW, 4.0f));
            ceiling.AddVertex(new Vector2(playingW, 3.9f));
            ceiling.AddVertex(new Vector2(0, 3.9f));
            ceiling.Color = Color.CornflowerBlue;
            world.Add(ceiling);

            Texture spritetexture = TextureLoader.FromFile(device, "../../ship.bmp");
            player.Tex = spritetexture;
            player.AddVertex(new Vector2(0, 0));
            player.AddTex(new Vector2(0, 1));
            player.AddVertex(new Vector2(0, 0.5f));
            player.AddTex(new Vector2(0, 0));
            player.AddVertex(new Vector2(0.175f, 0.5f));
            player.AddTex(new Vector2(0.35f, 0));
            player.AddVertex(new Vector2(0.5f, 0.325f));
            player.AddTex(new Vector2(1, 0.45f));
            player.AddVertex(new Vector2(0.5f, 0.225f));
            player.AddTex(new Vector2(1, 0.55f));
            player.AddVertex(new Vector2(0.175f, 0));
            player.AddTex(new Vector2(0.35f, 1));
            player.Color = Color.Transparent;
            player.Transparent = true;

            player.P = new Vector2(0.5f, 2.7f);
            player.A = new Vector2(0, gravity);

            Init();

        }

        /// <summary>
        /// ZSP
        /// Initializes our gamestate
        /// </summary>
        public void Init()
        {
            gamesound = new GameSounds(this);
            vertices = new VertexBuffer(typeof(CustomVertex.PositionColored), // Type of vertex
                            4,      // How many
                            device, // What device
                            0,      // No special usage
                            CustomVertex.PositionColored.Format,
                            Pool.Managed);

            //ZSP set up the score
            score = 0;
            crashed = false;
            textSprite = new Sprite(device);
            numFrame = 1;

            asteroidIndex = 2;

            playerIndex = 1;

            random = new Random();

            gdiFont = new System.Drawing.Font(FontFamily.GenericSansSerif, 10.0f, FontStyle.Regular);
            d3dFont = new Microsoft.DirectX.Direct3D.Font(device, gdiFont);

            background = new Background(device, playingW * numFrame, playingH, (playingW * numFrame) - playingW);
            backgrounds.Add(background);

            player.P = new Vector2(0.5f, 2.7f);
            Vector2 velocity = player.V;
            velocity.Y = 0f;
            player.V = velocity;
            player.A = new Vector2(0, gravity);


            hexagons.Clear();           

            addHexagon(new Vector2(5.0f, 1.0f), .45f);
            asteroids.Add(new Asteroid(device, .5f, new Vector2(5.0f, 1.0f), 1));
            asteroids.Add(new Asteroid(device, .5f, new Vector2(-10.0f, 1.0f), 2));
            asteroids.Add(new Asteroid(device, .5f, new Vector2(-15.0f, 1.0f), 3));

            // Determine the last time
            stopwatch.Start();
            lastTime = stopwatch.ElapsedMilliseconds;

            player.Boosting = false;
            player.SpeedUp = 1.0f;
            powerUps.Clear();
        }

        /// <summary>
        /// Initialize the Direct3D device for rendering
        /// </summary>
        /// <returns>true if successful</returns>
        private bool InitializeDirect3D()
        {
            try
            {
                // Now let's setup our D3D stuff
                PresentParameters presentParams = new PresentParameters();
                presentParams.Windowed = true;
                presentParams.SwapEffect = SwapEffect.Discard;

                device = new Device(0, DeviceType.Hardware, this, CreateFlags.SoftwareVertexProcessing, presentParams);
            }
            catch (DirectXException)
            {
                return false;
            }

            return true;
        }

        public void Render()
        {
            if (device == null)
                return;

            device.Clear(ClearFlags.Target, System.Drawing.Color.Blue, 1.0f, 0);

            int wid = Width;                            // Width of our display window
            int hit = Height;                           // Height of our display window.
            float aspect = (float)wid / (float)hit;     // What is the aspect ratio?

            device.RenderState.ZBufferEnable = false;   // We'll not use this feature
            device.RenderState.Lighting = false;        // Or this one...
            device.RenderState.CullMode = Cull.None;    // Or this one...

            float widP = playingH * aspect;         // Total width of window
            float winCenter = player.P.X;
            if (winCenter - widP / 2 < 0)
                winCenter = widP / 2;
            /*else if (winCenter + widP / 2 > playingW*2)
                winCenter = playingW - widP / 2;*/

            device.Transform.Projection = Matrix.OrthoOffCenterLH(winCenter - widP / 2,
                                                                  winCenter + widP / 2,
                                                                  0, playingH, 0, 1);

            //Begin the scene
            device.BeginScene();

            foreach (Polygon h in hexagons)
            {
                h.Render(device);
            }

            // Render the background
            //background.Render();
            foreach (Background bg in backgrounds)
            {
                bg.Render();
            }

            

            foreach (Polygon p in world)
            {
                p.Render(device);
            }

            

            foreach (Asteroid a in asteroids)
            {
                a.Render();
            }

            foreach (Polygon p in powerUps)
            {
                p.Render(device);
            }

            player.Render(device);

            //ZSP, render the score text last

            textSprite.Begin(SpriteFlags.AlphaBlend);
            Math.Round(score, 2);
            Math.Round(highScore, 2);
            string strScore = "Score: " + score.ToString();

            if (!crashed)
            {
                d3dFont.DrawText(textSprite, strScore, 20, 20, Color.White);
            }
            else
            {
                string strGameOver = "GAME OVER";
                string strPlayAgain = "Press 'P' to play again!";
                if (score > highScore)
                {
                    highScore = score;
                }
                string strHighScore = "High Score: " + highScore.ToString();

                gdiFont = new System.Drawing.Font(FontFamily.GenericSansSerif, 10.0f, FontStyle.Regular);
                d3dFont = new Microsoft.DirectX.Direct3D.Font(device, gdiFont);

                d3dFont.DrawText(textSprite, strScore, 350, 225, Color.White);
                d3dFont.DrawText(textSprite, strHighScore, 350, 240, Color.White);
                d3dFont.DrawText(textSprite, strPlayAgain, 350, 255, Color.White);

                gdiFont = new System.Drawing.Font(FontFamily.GenericSansSerif, 26.0f, FontStyle.Regular);
                d3dFont = new Microsoft.DirectX.Direct3D.Font(device, gdiFont);

                d3dFont.DrawText(textSprite, strGameOver, 325, 190, Color.White);
            }

            textSprite.End();

            //End the scene
            device.EndScene();
            device.Present();
        }

        protected override void OnKeyDown(System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                this.Close(); // Esc was pressed
            else if (e.KeyCode == Keys.Right)
            {
                Vector2 v = player.V;
                //v.X = 1.5f;
                player.V = v;
            }
            else if (e.KeyCode == Keys.Left)
            {
                Vector2 v = player.V;
                //v.X = -1.5f;
                player.V = v;
            }
            else if (e.KeyCode == Keys.Space) //&& standingOn == true)
            {
                gamesound.Thruster();
                Vector2 v = player.V;
                v.Y = 4;
                player.V = v;
                player.A = new Vector2(0, -9.8f);
            }
            else if (e.KeyCode == Keys.P && crashed) //ZSP, code for restarting the game if we crash
            {
                Init();
            }

        }

        protected override void OnKeyUp(System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Right || e.KeyCode == Keys.Left)
            {
                Vector2 v = player.V;
                v.X = 0;
                player.V = v;
            }
        }

        /// <summary>
        /// Advance the game in time
        /// </summary>
        public void Advance()
        {
            // How much time change has there been?
            long time = stopwatch.ElapsedMilliseconds;
            //ZSP, do not update delta if we have crashed
            float delta = 0;
            if (!crashed)
            {
                delta = (time - lastTime) * 0.001f;       // Delta time in milliseconds

                //ZSP, incremement the score
                score += (Decimal)delta;
                if (player.Boosting)
                {
                    score += (Decimal)delta;
                }
                Math.Round(score, 2);
            }
            lastTime = time;


            while (delta > 0)
            {

                float step = delta;
                if (step > 0.05f)
                    step = 0.05f;

                float maxspeed = Math.Max(Math.Abs(player.V.X), Math.Abs(player.V.Y));
                if (maxspeed > 0)
                {
                    step = (float)Math.Min(step, 0.05 / maxspeed);
                }
                player.Advance(step);
                if (player.P.X >= (playingW * numFrame) - 6f)
                {
                    numFrame +=1;
                    Background bg = new Background(device, playingW * numFrame, playingH, (playingW * numFrame) - (playingW + .1f));
                    backgrounds.Add(bg);
                    
                    AddObstacle((playingW * numFrame) - (playingW+.1f), playingW * numFrame, 0, .1f, Color.CornflowerBlue);
                    AddObstacle((playingW * numFrame) - (playingW+.1f), playingW * numFrame, 3.9f, 4.0f, Color.CornflowerBlue);
                    if (backgrounds.Count > 2)
                    {
                        backgrounds.Remove(backgrounds.First());
                    }
                }

                if((int)(player.P.X)/(5.0*playerIndex) == 1){

                    float r = (float)(random.NextDouble() * 2.7f) + .5f;

                    addHexagon(new Vector2(5.0f * (playerIndex + 1), r), .45f);
                    if (hexagons.Count > 3) hexagons.Remove(hexagons.First());
                    asteroids[asteroidIndex++ % 3].Position = new Vector2(5.0f * (playerIndex++ + 1),r);

                }


                // Chance to add power up!
                if (random.NextDouble() * 800.0f > 799.0f)
                {
                    float pos = (float)random.NextDouble() * 3.0f;
                    AddPowerUp(player.P.X + 10.0f, player.P.X + 10.2f, pos, pos + 0.2f);
                    if (powerUps.Count > 10) powerUps.Remove(powerUps.First());
                }
               

                foreach (Polygon p in world)
                    p.Advance(step);

                foreach (Polygon p in world)
                {
                    if (collision.Test(player, p) || player.P.Y <= .1 || player.P.Y >= 3.9 - .5)
                    {
                        float depth = collision.P1inP2 ?
                                  collision.Depth : -collision.Depth;
                        player.P = player.P + collision.N * depth;
                        Vector2 v = player.V;
                        //ZSP
                        v.X = 0;
                        v.Y = 0;
                        gamesound.Explode();
                        crashed = true;
                        player.V = v;
                        player.Advance(0);
                    }
                }

        
                foreach (Polygon p in hexagons)
                {
                    if ((collision.Test(player, p) || player.P.Y <= .1 || player.P.Y >= 3.9 -.5) && !player.Boosting)
                    {
                        float depth = collision.P1inP2 ?
                                  collision.Depth : -collision.Depth;
                        player.P = player.P + collision.N * depth;
                        Vector2 v = player.V;
                        //ZSP
                        v.X = 0;
                        v.Y = 0;
                        /*if (collision.N.X != 0)
                            v.X = 0;
                        if (collision.N.Y != 0)
                        {
                            v.Y = 0;
                            if (collision.N.Y > 0)
                            {
                                standingOn = true;
                            }
                        }
                        if (standingOn)
                        {
                            v.Y = -1;
                        }*/
                        gamesound.Explode();
                        crashed = true;
                        player.V = v;
                        player.Advance(0);
                    }
                }

                foreach (Polygon p in powerUps)
                {
                    if (collision.Test(player, p) || player.P.Y <= .1 || player.P.Y >= 3.9 - .5)
                    {
                        player.Boosting = true;
                    }
                }

                delta -= step;
            }

        }

        /// <summary>
        /// Adds a polygon object as an obstacle in the game world.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="bottom"></param>
        /// <param name="top"></param>
        /// <param name="color"></param>
        public void AddObstacle(float left, float right, float bottom, float top, Color color)
        {
            Polygon poly = new Polygon();
            poly.AddVertex(new Vector2(left, top));
            poly.AddVertex(new Vector2(right, top));
            poly.AddVertex(new Vector2(right, bottom));
            poly.AddVertex(new Vector2(left, bottom));
            poly.Color = color;
            world.Add(poly);
        }

        public void addHexagon(Vector2 position, float size = 1)
        {
            Vector2 one = new Vector2(-1.0f, 0) *size + position;
            Vector2 two = new Vector2(-.5f, 1.0f) * size + position;
            Vector2 three = new Vector2(.5f, 1.0f) * size + position;
            Vector2 four = new Vector2(1.0f, 0) * size + position;
            Vector2 five = new Vector2(.5f, -1.0f) * size + position;
            Vector2 six = new Vector2(-.5f, -1f) * size + position;
            Vector2 seven = new Vector2(0, 0) * size + position;
    
      

            Polygon poly = new Polygon();
            poly.AddVertex(one);
            poly.AddVertex(two);
            poly.AddVertex(three);
            poly.AddVertex(four);
            poly.AddVertex(five);
            poly.AddVertex(six);
            poly.Color = Color.Transparent;
            hexagons.Add(poly);

   
        
        }

        public void AddTriangle(float left, float right, float bottom, float top, Color color)
        {
            Polygon poly = new Polygon();
            poly.AddVertex(new Vector2((right+left)/2, top));
            poly.AddVertex(new Vector2(right, bottom));
            poly.AddVertex(new Vector2(left, bottom));
            //poly.AddVertex(new Vector2(left, bottom));
            poly.Color = color;
            world.Add(poly);

        }

        public void AddPowerUp(float left, float right, float bottom, float top)
        {
            Polygon poly = new Polygon();
            poly.AddVertex(new Vector2(left, top));
            poly.AddVertex(new Vector2(right, top));
            poly.AddVertex(new Vector2(right, bottom));
            poly.AddVertex(new Vector2(left, bottom));
            poly.Color = Color.LimeGreen;
            powerUps.Add(poly);
        }

        public void AddObstacleTextured()
        {
        }
    }
}
