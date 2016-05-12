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

            graphics.PreferredBackBufferWidth = 1380;
            graphics.PreferredBackBufferHeight = 720;

            utility = new Util(this);
            this.Components.Add(utility);
          
            input = new InputHandler(this);
            this.Components.Add(input);

            console = new GameConsole(this);
            this.Components.Add(console);

            lanko = new MonoGameLanko(this);
            this.Components.Add(lanko);

            itemMan = new MonogameItemManager(this);
            this.Components.Add(itemMan);

            gameRoundMan = new MonogameRoundManager(this);
            this.Components.Add(gameRoundMan);
        }

        protected override void Initialize()
        {
            //this.graphics.IsFullScreen = true;
            this.graphics.ApplyChanges();

            this.sideColor = new Color(14, 13, 17);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            itemMan.border = gameRoundMan.border = this.lanko.border;
            itemMan.lanko = lanko;
            itemMan.glub = lanko.glub;
            itemMan.round = gameRoundMan.round;

            gameRoundMan.FirstTimeSetup();
            utility.MRM = gameRoundMan;

            IsMouseVisible = false;
            spriteBatch = new SpriteBatch(GraphicsDevice);

            //lanko.ShowMarkers = true;
            //lanko.glub.ShowMarkers = true;
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || 
                Keyboard.GetState().IsKeyDown(Keys.Escape) || Keyboard.GetState().IsKeyDown(Keys.End))
                Exit();

            //if(Keyboard.GetState().IsKeyDown(Keys.P) && canTogglePause)
            //{
            //    canTogglePause = false;

            //    if (gamePaused)
            //    {
            //        gamePaused = false;
            //        MediaPlayer.Resume();
            //        MediaPlayer.Volume = .1f;
            //    }

            //    else
            //    {
            //        gamePaused = true;
            //        MediaPlayer.Pause();
            //    }
            //}

            //if(Keyboard.GetState().IsKeyUp(Keys.P) && !canTogglePause)
            //    canTogglePause = true;

            #region Fullscreen
            //if (Keyboard.GetState().IsKeyDown(Keys.F1))
            //    this.ToggleFullScreen();
            
            //if(Keyboard.GetState().IsKeyDown(Keys.F2))
            //{
            //    this.graphics.PreferredBackBufferWidth += 10;
            //    this.graphics.ApplyChanges();
            //    this.lanko.border.Initialize();
            //}

            //else if (Keyboard.GetState().IsKeyDown(Keys.F3))
            //{
            //    this.graphics.PreferredBackBufferWidth -= 10;
            //    this.graphics.ApplyChanges();
            //    this.lanko.border.Initialize();
            //}

            //else if (Keyboard.GetState().IsKeyDown(Keys.F4))
            //{
            //    this.graphics.PreferredBackBufferHeight += 10;
            //    this.graphics.ApplyChanges();
            //    this.lanko.border.Initialize();
            //}

            //else if (Keyboard.GetState().IsKeyDown(Keys.F5))
            //{
            //    this.graphics.PreferredBackBufferHeight -= 10;
            //    this.graphics.ApplyChanges();
            //    this.lanko.border.Initialize();
            //}
            #endregion

            if (input.MouseState.RightButton == ButtonState.Pressed && !utility.GamePaused)
            {
                if (this.itemMan.round.RoundIsOver )
                {
                    if (this.gameRoundMan.FirstRoundStartHasStarted == true)
                    {
                        utility.lengthGamePaused = 0;
                        this.itemMan.round.RoundIsOver = false;
                        this.gameRoundMan.HasStartedRound = true;
                        
                    }
                }
            }

            utility.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(this.sideColor);

            base.Draw(gameTime);
            
            spriteBatch.Begin();
            itemMan.Draw(spriteBatch);
            //lanko.DrawMarkers(spriteBatch);
            lanko.glub.DrawMarkers(spriteBatch);
            spriteBatch.End();
        }

        //private void ToggleFullScreen()
        //{
        //    if (this.graphics.IsFullScreen)
        //        this.graphics.IsFullScreen = false;

        //    else
        //        this.graphics.IsFullScreen = true;

        //    this.graphics.ApplyChanges();
        //}
    }
}
