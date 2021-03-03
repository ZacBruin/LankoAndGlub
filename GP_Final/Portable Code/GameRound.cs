using Microsoft.Xna.Framework;

namespace GP_Final
{
    public class GameRound : GameComponent
    {
        public float CurrentRoundTime { get; private set; }
        public bool RoundIsOver;
        public int Points;

        private bool roundHasStarted;
        private float timeRoundStarted;

        private const float maxRoundLength = 50;
        public float MaxRoundLength
        {
            get {return maxRoundLength;}
        }

        public GameRound(Game game) : base(game)
        {
            RoundIsOver = true;
            roundHasStarted = true;
        }

        public override void Update(GameTime gameTime)
        {
            float totalGameTime = (float)gameTime.TotalGameTime.TotalMilliseconds / 1000;

            if (!Lanko_And_Glub.utility.IsGamePaused)
            {
                if (RoundIsOver == false)
                {
                    if (roundHasStarted)
                    {
                        roundHasStarted = false;
                        timeRoundStarted = totalGameTime;
                    }

                    else
                    {
                        CurrentRoundTime = totalGameTime - timeRoundStarted - Lanko_And_Glub.utility.LengthGamePaused/2f;

                        if (CurrentRoundTime >= maxRoundLength)
                            RoundIsOver = true;
                    }
                }
                base.Update(gameTime);
            }
        }

        public void ResetRound()
        {
            roundHasStarted = true;
            Lanko_And_Glub.utility.LengthGamePaused = 0;
            Points = 0;
        }
    }
}
