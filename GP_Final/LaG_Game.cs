using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
//using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Media;

namespace GP_Final
{
    public class Lanko_And_Glub : Game
    {
        public static Util utility;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Color sideColor;

        GameConsole console;
        InputHandler input;

        MonoGameLanko lanko;

        MonogameItemManager itemMan;
        MonogameRoundManager gameRoundMan;
       
        public Lanko_And_Glub() : base()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;

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

            itemMan = new MonogameItemManager(this);
            Components.Add(itemMan);

            gameRoundMan = new MonogameRoundManager(this);
            Components.Add(gameRoundMan);
        }

        protected override void Initialize()
        {
            graphics.ApplyChanges();

            sideColor = new Color(14, 13, 17);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            itemMan.border = gameRoundMan.border = lanko.border;
            itemMan.lanko = lanko;
            itemMan.glub = lanko.glub;
            itemMan.round = gameRoundMan.round;

            gameRoundMan.FirstTimeSetup();
            utility.MRM = gameRoundMan;

            IsMouseVisible = false;
            spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape) || Keyboard.GetState().IsKeyDown(Keys.End))
            {
                MediaPlayer.Pause();
                Exit();
            }

            if (input.MouseState.RightButton == ButtonState.Pressed && !utility.GamePaused)
            {
                if (itemMan.round.RoundIsOver )
                {
                    if (gameRoundMan.FirstRoundStartHasStarted == true)
                    {
                        utility.lengthGamePaused = 0;
                        itemMan.round.RoundIsOver = false;
                        gameRoundMan.HasStartedRound = true;
                        
                    }
                }
            }

            utility.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(sideColor);

            base.Draw(gameTime);
            
            spriteBatch.Begin();
            itemMan.Draw(spriteBatch);
            lanko.glub.DrawMarkers(spriteBatch);
            spriteBatch.End();
        }
    }
}
