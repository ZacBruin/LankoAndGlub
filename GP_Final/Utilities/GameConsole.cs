using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;

namespace GP_Final
{
    public interface IGameConsole
    {
        string FontName { get; set; }
        string DebugText { get; set; }

        string GetGameConsoleText();
        void GameConsoleWrite(string s);
    }

    //Console State
    public enum GameConsoleState { Closed, Open };

    public class GameConsole : Microsoft.Xna.Framework.DrawableGameComponent, IGameConsole
    {
        protected string fontName;
        public string FontName { get { return fontName; } set { fontName = value; } }

        protected string debugText;
        public string DebugText { get { return debugText; } set { debugText = value; } }

        protected int maxLines;
        public int MaxLines { get { return maxLines; } set { maxLines = value; } }

        SpriteFont font;
        SpriteBatch spriteBatch;
        ContentManager content;

        protected List<string> gameConsoleText;
        protected GameConsoleState gameConsoleState;

        public Keys ToggleConsoleKey;

        InputHandler input;

        public GameConsole(Game game)
            : base(game)
        {
            // TODO: Construct any child components here
            fontName = "Arial";
            gameConsoleText = new List<string>();
            gameConsoleText.Add("Console Initalized");
            content = new ContentManager(game.Services);
            maxLines = 20;
            debugText = "Console default \n         debug text";
            ToggleConsoleKey = Keys.OemTilde;

            gameConsoleState = GameConsoleState.Closed;


            input = (InputHandler)game.Services.GetService(typeof(IInputHandler));

            //Make sure input service exsists
            if (input == null)
            {
                throw new Exception("GameConsole Depends on Input service please add input service before you add GameConsole.");
            }

            game.Services.AddService(typeof(IGameConsole), this);
        }

        protected override void LoadContent()
        {

            font = content.Load<SpriteFont>("content/Font");
            spriteBatch = new SpriteBatch(GraphicsDevice);
            base.LoadContent();
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        protected override void UnloadContent()
        {
            base.UnloadContent();
        }

        public override void Update(GameTime gameTime)
        {

            if (input.KeyboardState.HasReleasedKey(ToggleConsoleKey))
            {
                if (gameConsoleState == GameConsoleState.Closed)
                    gameConsoleState = GameConsoleState.Open;

                else
                    gameConsoleState = GameConsoleState.Closed;
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            if (gameConsoleState == GameConsoleState.Open)
            {
                spriteBatch.Begin();

                spriteBatch.DrawString(font, GetGameConsoleText(), new Vector2(263f, 63f), Color.Wheat);
                spriteBatch.DrawString(font, debugText, new Vector2(463f, 63f), Color.Wheat);

                spriteBatch.End();
            }
            base.Draw(gameTime);
        }

        public string GetGameConsoleText()
        {
            string Text = "";

            string[] current = new string[Math.Min(gameConsoleText.Count, MaxLines)];
            int offsetLines = (gameConsoleText.Count / maxLines) * maxLines;

            int offest = gameConsoleText.Count - offsetLines;

            int indexStart = offsetLines - (maxLines - offest);
            if (indexStart < 0)
                indexStart = 0;

            gameConsoleText.CopyTo(
                indexStart, current, 0, Math.Min(gameConsoleText.Count, MaxLines));

            foreach (string s in current)
            {
                Text += s;
                Text += "\n";
            }
            return Text;
        }

        public void GameConsoleWrite(string s)
        {
            gameConsoleText.Add(s);
        }


    }
}
