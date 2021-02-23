using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace GP_Final
{
    public class Util : GameComponent
    {
        public bool IsGamePaused
        {
            get;
            private set;
        }

        public bool IsShowingInstructions
        {
            get;
            private set;
        }

        public float LengthGamePaused;
        public MonogameRoundManager roundManager;

        private const Keys pauseKey = Keys.P;

        //This is essentially a way to track if the pauseKey has been pressed
        private bool canTogglePause;
        
        public Util(Game game) : base(game)
        {
            IsGamePaused = false;
            canTogglePause = true;
            IsShowingInstructions = true;
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            if (IsGamePaused)
                LengthGamePaused += (float)gameTime.ElapsedGameTime.Milliseconds / 1000;

            if (Keyboard.GetState().IsKeyDown(pauseKey) && canTogglePause)
                AttemptTogglePause();

            if (Keyboard.GetState().IsKeyUp(pauseKey) && !canTogglePause)
                canTogglePause = true;
        }

        private void AttemptTogglePause()
        {
            if (roundManager.FirstRoundStartHasStarted)
            {
                canTogglePause = false;
                IsGamePaused = !IsGamePaused;
           
                if (IsGamePaused)
                    MediaPlayer.Pause();
                else
                    MediaPlayer.Resume();
            }
        }

    }
}
