using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;

namespace GP_Final
{
    public sealed class GoldenBat : Target
    {
        Random rand;
        Coroutine coroutines;
        Vector2 newDirection;

        private int numTimesChangedDir, 
                    maxAllowedDirChanges;

        private float timeChangedDir, 
                      minTimeBeforeDirChange, 
                      timeBetweenChecks;

        public GoldenBat(Game game) : base(game)
        {
            MaxTimeOnScreen = 4f;
            pointValue = 3;
            Speed = 250;

            timeBetweenChecks = .1f;
            minTimeBeforeDirChange = 1.0f;

            rand = new Random();
            coroutines = new Coroutine();
        }

        protected override void LoadContent()
        {
            movementSpriteSheet = content.Load<Texture2D>("SpriteSheets/GoldBat");
            spawningSpriteSheet = content.Load<Texture2D>("SpriteSheets/GoldBatSpawn");

            spriteTexture = spawningSpriteSheet;
            SheetInfo = new SpriteSheetInfo(5, spriteTexture.Width, spriteTexture.Height, updatesPerFrame);

            SourceRectangle = SheetInfo.SourceFrame;
            spriteSheetFramesWide = SheetInfo.TotalFrames;

            SetTranformAndRect();
            UpdateHitbox();

            maxAllowedDirChanges = rand.Next(1,3);

            base.LoadContent();
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }

        public override void Update(GameTime gameTime)
        {
            if (hasSpawned)
            {
                spriteTexture = movementSpriteSheet;
                SheetInfo.CurrentFrame = 0;
                SheetInfo.UpdateSourceFrame();
                SourceRectangle = SheetInfo.SourceFrame;
                hasSpawned = false;
            }

            if (coroutines.Count == 0)
                if(CurrentTimeOnScreen <= MaxTimeOnScreen)
                    coroutines.Start(DirectionChangeDelay(gameTime));

            switch (state)
            {
                case State.SpeedDown:
                    if (Speed > 100)
                        Speed -= 2;
                    else
                        ChangeDirection(gameTime);
                    break;

                case State.SpeedUp:
                    if (Speed < 250)
                        Speed += 2;
                    else
                        state = State.Moving;
                    break;   
            
                case State.DeSpawning:
                case State.Dying:
                    if(coroutines.Running)
                        coroutines.StopAll();
                    break;
            }

            if (state != State.Dying && state != State.DeSpawning)
            {
                if (coroutines.Running)
                    coroutines.Update();
            }

            base.Update(gameTime);
        }

        private void CheckForDirectionChange(GameTime gameTime)
        {
            if (timeChangedDir + 1 < (gameTime.TotalGameTime.TotalMilliseconds / 1000) || timeChangedDir == 0)
            {
                int num = rand.Next(20);

                if (num > 15)
                    GetNewDirection(gameTime);
            }
        }

        private IEnumerator DirectionChangeDelay(GameTime gameTime)
        {
            //Allow bat to be on screen for one second before it can change its direction
            if (CurrentTimeOnScreen <= minTimeBeforeDirChange)
                yield return coroutines.Pause(minTimeBeforeDirChange);

            else
            {
                yield return coroutines.Pause(timeBetweenChecks);
                CheckForDirectionChange(gameTime);
            }
        }

        private void GetNewDirection(GameTime gameTime)
        {          
            do
            {
                newDirection.X = Direction.X >= 0
                    ? rand.Next(-100, -30)
                    : rand.Next(30, 100);

                newDirection.Y = rand.Next(-20, 20);
            }
            while (Math.Abs(newDirection.Y) > Math.Abs(newDirection.X));

            state = State.SpeedDown;
        }

        private void ChangeDirection(GameTime gameTime)
        {
            Direction = newDirection;

            SpriteEffects = Direction.X > 0 
                ? SpriteEffects.FlipHorizontally
                : SpriteEffects.None;

            state = State.SpeedUp;

            timeChangedDir = (float)gameTime.TotalGameTime.TotalMilliseconds / 1000;
            numTimesChangedDir++;
        }

    }
}
