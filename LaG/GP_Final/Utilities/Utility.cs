using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace GP_Final
{
    public class Util : Microsoft.Xna.Framework.GameComponent
    {
        public bool GamePaused
        {
            get;
            private set;
        }

        public bool ShowInstructions
        {
            get;
            private set;
        }

        public bool canTogglePause;

        public Util(Game game) : base(game)
        {
            GamePaused = false;
            canTogglePause = true;
            ShowInstructions = true;
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            //if (Keyboard.GetState().IsKeyDown(Keys.P) && canTogglePause)
            //{
            //    canTogglePause = false;

            //    if (GamePaused)
            //    {
            //        GamePaused = false;
            //        MediaPlayer.Resume();
            //        MediaPlayer.Volume = .1f;
            //    }

            //    else
            //    {
            //        GamePaused = true;
            //        MediaPlayer.Pause();
            //    }
            //}

            //if (Keyboard.GetState().IsKeyUp(Keys.P) && !canTogglePause)
            //    canTogglePause = true;

            //if (Coroutines.Running)
            //    Coroutines.Update();
        }
    }
}
