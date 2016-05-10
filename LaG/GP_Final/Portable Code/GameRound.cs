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
            this.MaxRoundLength = 5;
            this.RoundIsOver = true;
            HasRoundJustStarted = true;
        }

        public override void Update(GameTime gameTime)
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
                        ((float)gameTime.TotalGameTime.TotalMilliseconds / 1000) - TimeRoundStarted;

                    if (this.CurrentRoundTime >= this.MaxRoundLength)
                    {
                        this.RoundIsOver = true;
                    }
                        

                }
            }

            base.Update(gameTime);
        }

        public void ResetRound()
        {
            this.HasRoundJustStarted = true;
            this.Points = 0;
        }
    }
}
