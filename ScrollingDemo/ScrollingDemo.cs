using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ScrollingDemo
{
    /// <summary>
    /// Scrolling map demo that demonstrates jitter in XNA/Monogame
    /// </summary>
    public class ScrollingDemo : Game
    {

        // Set to true to display time between each draw call
        private Boolean enableVsync = true;

        private readonly Stopwatch stopWatch = new Stopwatch();

        readonly GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        private RenderTarget2D renderTarget;
        private float lastKeypress = 1000f;

        // Internal resolution
        const int VirtualWidth = 480;
        const int VirtualHeight = 270;
        
        // For 16x16 tiles in 480x270 resolution...
        private const int TileWidth = 16;
        private const int TileHeight = 16;
        private const int ScreenTilesWide = 30;
        private const int ScreenTilesHigh = 17;

        // Array that contains random tiles from tileset that we will scroll through
        private readonly int[,] mapData = new int[ScreenTilesWide, 3000];

        // Generates random map tile index
        readonly Random randomNumber = new Random();

        // Contains map tiles
        private Texture2D tileSet;

        // Starts at the bottom of the map
        private Vector2 camera = new Vector2(0, 3000 - ScreenTilesHigh-1);

        private SpriteFont spriteFont;

        private int offset = 0;
        private float scrollTimer;

        // Speed of scrolling
        private int scrollSpeed = 1;

        public ScrollingDemo()
        {
            IsFixedTimeStep = false;
            graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferHeight = VirtualHeight*4,
                PreferredBackBufferWidth = VirtualWidth*4,
                IsFullScreen = false,
                PreferMultiSampling = false,
                SynchronizeWithVerticalRetrace = enableVsync // Disable vsync if frame timer is enabled
            };
            
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
            spriteBatch = new SpriteBatch(graphics.GraphicsDevice);

            // Screen is drawn on this smaller target which is stretched to screen size later
            renderTarget = new RenderTarget2D(graphics.GraphicsDevice, VirtualWidth, VirtualHeight, false,
                SurfaceFormat.Color, DepthFormat.None, graphics.GraphicsDevice.PresentationParameters.MultiSampleCount, RenderTargetUsage.DiscardContents);

            stopWatch.Start();

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
            tileSet = Content.Load<Texture2D>(@"TileSet");

            // Create a random map to scroll
            for (int row = 0; row < 3000; row++)
            {
                for (int col = 0; col < ScreenTilesWide; col++)
                {
                    // Pick a random tile from the tileset
                    int tileNo = randomNumber.Next(0, 3);
                    if (tileNo == 1) tileNo = randomNumber.Next(0, 3);
                    if (tileNo == 1) tileNo = randomNumber.Next(0, 3); // see that one less often
                    mapData[col, row] = tileNo;
                }
            }

            spriteFont = Content.Load<SpriteFont>(@"Arial");

        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            lastKeypress += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if ((Keyboard.GetState().IsKeyDown(Keys.F)) && (lastKeypress >= 250))
            {
                graphics.ToggleFullScreen();
                graphics.ApplyChanges();
                lastKeypress = 0f;
            }

            if ((Keyboard.GetState().IsKeyDown(Keys.S)) && (lastKeypress >= 250))
            {
                scrollSpeed++;
                if ((scrollSpeed) > 16) scrollSpeed = 1;
                lastKeypress = 0f;
            }

            if ((Keyboard.GetState().IsKeyDown(Keys.V)) && (lastKeypress >= 250))
            {
                enableVsync = !enableVsync;
                graphics.SynchronizeWithVerticalRetrace = enableVsync;
                graphics.ApplyChanges();
                lastKeypress = 0f;
            }


            //Scroll map down(sequential decrementation method)
            scrollTimer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (scrollTimer > (1000f/60) || enableVsync)
            {
                offset -= scrollSpeed;
                if (offset < 0)
                {
                    offset = 15;
                    camera.Y--;
                }
                scrollTimer = 0;
            }

            // Reset map to bottom if we go to far
            if (camera.Y <= 0) camera.Y = 3000 - ScreenTilesHigh - 1;

            base.Update(gameTime);

        }
        
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            TimeSpan drawTime = stopWatch.Elapsed;
            stopWatch.Reset();
            stopWatch.Start();

            // Draw onto our render target first
            graphics.GraphicsDevice.SetRenderTarget(renderTarget);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);

            // Render the map
            for (int row = 0; row <= ScreenTilesHigh; row++)
            {
                for (int col = 0; col < ScreenTilesWide; col++)
                {
                    int tile = mapData[(int)camera.X + col, (int)camera.Y + row];

                    //offset = (int) Math.Floor(MathHelper.Lerp(0, 15, (float) ((camera.Y - Math.Truncate(camera.Y))))); // imprecise method

                    int x = col * TileWidth;                    
                    int y = (row * TileHeight) - offset;


                    Rectangle tileSprite = new Rectangle(
                        tile * TileWidth,
                        0,
                        TileWidth,
                        TileHeight);

                    spriteBatch.Draw(
                        tileSet,
                        new Rectangle(x, y, TileWidth, TileHeight),
                        tileSprite,
                        Color.White);

                }
            }

            spriteBatch.DrawString(spriteFont, drawTime.TotalMilliseconds + @"ms", new Vector2(375, 10), Color.White);
            spriteBatch.DrawString(spriteFont, enableVsync ? @"vsync on" : @"vsync off", new Vector2(375, 30), Color.White);
            spriteBatch.DrawString(spriteFont, @"speed: "+scrollSpeed, new Vector2(375, 50), Color.White);

            spriteBatch.DrawString(spriteFont, @"v=vsync  f=fullscreen  s=speed", new Vector2(210, 245), Color.White);

            spriteBatch.End();

            // Draw the render target to screen
            graphics.GraphicsDevice.SetRenderTarget(null);
            graphics.GraphicsDevice.Clear(ClearOptions.Target, Color.Black, 1.0f, 0);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointClamp);
            spriteBatch.Draw(renderTarget, new Rectangle(0,0,graphics.PreferredBackBufferWidth,graphics.PreferredBackBufferHeight), Color.White);
            spriteBatch.End();

            base.Draw(gameTime);

        }
    }
}
