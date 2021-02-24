using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace GP_Final
{
    public class Lanko_And_Glub : Game
    {
        public static Util utility;
      
        GraphicsDeviceManager graphics;
        const int resolutionWidth = 1280;
        const int resolutionHeight = 720;

        SpriteBatch spriteBatch;
        Color borderColor;

        GameConsole console;
        InputHandler input;

        MonoGameLanko lanko;
        MonogameItemManager itemManager;
        MonogameRoundManager roundManager;
       
        public Lanko_And_Glub() : base()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = resolutionWidth;
            graphics.PreferredBackBufferHeight = resolutionHeight;
            graphics.HardwareModeSwitch = false;
            graphics.IsFullScreen = true;

            utility = new Util(this);
            Components.Add(utility);
          
            input = new InputHandler(this);
            Components.Add(input);

            console = new GameConsole(this);
            Components.Add(console);

            lanko = new MonoGameLanko(this);
            Components.Add(lanko);

            itemManager = new MonogameItemManager(this);
            Components.Add(itemManager);

            roundManager = new MonogameRoundManager(this);
            Components.Add(roundManager);
        }

        protected override void Initialize()
        {
            graphics.ApplyChanges();
            borderColor = new Color(14, 13, 17);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            itemManager.border = roundManager.border = lanko.Border;
            itemManager.lanko = lanko;
            itemManager.glub = lanko.Glub;
            itemManager.round = roundManager.round;

            roundManager.FirstTimeSetup();
            utility.roundManager = roundManager;

            IsMouseVisible = false;
            spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                MediaPlayer.Pause();
                Exit();
            }

            if (input.MouseState.RightButton == ButtonState.Pressed && !utility.IsGamePaused)
                StartNewRound();

            utility.Update(gameTime);
            base.Update(gameTime);
        }

        private void StartNewRound()
        {
            if (itemManager.round.RoundIsOver)
            {
                if (roundManager.FirstRoundStartHasStarted == true)
                {
                    utility.LengthGamePaused = 0;
                    itemManager.round.RoundIsOver = false;
                    roundManager.HasStartedRound = true;
                }
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(borderColor);

            base.Draw(gameTime);
            
            spriteBatch.Begin();
            itemManager.Draw(spriteBatch);
            lanko.Glub.DrawMarkers(spriteBatch);
            spriteBatch.End();
        }
    }
}
