using Microsoft.Xna.Framework;

namespace GP_Final
{
    public class GameRound : Microsoft.Xna.Framework.GameComponent
    {
        public float TimeRoundStarted, MaxRoundLength, CurrentRoundTime;
        public bool HasRoundJustStarted, RoundIsOver;

        public int Points;

        public GameRound(Game game) : base(game)
        {
            this.MaxRoundLength = 10;
            this.RoundIsOver = true;
            HasRoundJustStarted = true;
        }

        public override void Update(GameTime gameTime)
        {
            if (!Lanko_And_Glub.utility.GamePaused)
            {
                if (RoundIsOver == false)
                {
                    if (HasRoundJustStarted)
                    {
                        HasRoundJustStarted = false;
                        TimeRoundStarted = ((float)gameTime.TotalGameTime.TotalMilliseconds / 1000);
                    }

                    else
                    {
                        CurrentRoundTime =
                            ((float)gameTime.TotalGameTime.TotalMilliseconds / 1000) - TimeRoundStarted - Lanko_And_Glub.utility.lengthGamePaused/2f;

                        if (this.CurrentRoundTime >= this.MaxRoundLength)
                        {
                            this.RoundIsOver = true;
                        }


                    }
                }

                base.Update(gameTime);
            }
        }

        public void ResetRound()
        {
            this.HasRoundJustStarted = true;
            Lanko_And_Glub.utility.lengthGamePaused = 0;
            this.Points = 0;
        }
    }
}
