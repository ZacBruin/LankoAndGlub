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

        private int directionChanges, 
                    maxDirectionChanges;

        private float lastTimeDirectionChanged, 
                      timeRequiredBeforeDirectionChange, 
                      timeBetweenChecks;

        private const string moveSpriteSheet = "SpriteSheets/GoldBat";
        private const string spawnSpriteSheet = "SpriteSheets/GoldBatSpawn";

        private const int spriteSheetFrames = 5;
        private const int maxTimeOnScreen = 6;
        private const int pointVal = 1;

        private const float startingSpeed = 250;
        private const float minimumSpeed = 100;
        private const float maximumSpeed = 250;
        private const float speedCrement = 2;

        public GoldenBat(Game game) : base(game)
        {
            MaxTimeOnScreen = maxTimeOnScreen;
            PointValue = pointVal;
            Speed = startingSpeed;

            timeBetweenChecks = .1f;
            timeRequiredBeforeDirectionChange = 1.0f;

            rand = new Random();
            coroutines = new Coroutine();
        }

        protected override void LoadContent()
        {
            movementSpriteSheet = content.Load<Texture2D>(moveSpriteSheet);
            spawningSpriteSheet = content.Load<Texture2D>(spawnSpriteSheet);

            spriteTexture = spawningSpriteSheet;
            SheetInfo = new SpriteSheetInfo(spriteSheetFrames, spriteTexture.Width, spriteTexture.Height, updatesPerFrame);

            SourceRectangle = SheetInfo.SourceFrame;
            spriteSheetFramesWide = SheetInfo.TotalFrames;

            SetTranformAndRect();
            UpdateHitbox();

            maxDirectionChanges = rand.Next(1,3);

            base.LoadContent();
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }

        public override void Update(GameTime gameTime)
        {
            float totalGameTime = (float)gameTime.TotalGameTime.TotalMilliseconds;

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
                    coroutines.Start(DirectionChangeDelay(totalGameTime));

            ManageState(totalGameTime);
            base.Update(gameTime);
        }

        private void CheckForDirectionChange(float totalGameTime)
        {
            if (lastTimeDirectionChanged + 1 < totalGameTime || lastTimeDirectionChanged == 0)
            {
                int num = rand.Next(20);

                if (num > 15)
                    GetNewDirection();
            }
        }

        private IEnumerator DirectionChangeDelay(float totalGameTime)
        {
            //Allow bat to be on screen for one second before it can change its direction
            if (CurrentTimeOnScreen <= timeRequiredBeforeDirectionChange)
                yield return coroutines.Pause(timeRequiredBeforeDirectionChange);

            else
            {
                yield return coroutines.Pause(timeBetweenChecks);
                CheckForDirectionChange(totalGameTime);
            }
        }

        private void GetNewDirection()
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

        private void ChangeDirection(float totalGameTime)
        {
            Direction = newDirection;

            SpriteEffects = Direction.X > 0 
                ? SpriteEffects.FlipHorizontally
                : SpriteEffects.None;

            state = State.SpeedUp;

            lastTimeDirectionChanged = totalGameTime;
            directionChanges++;
        }

        private void ManageState(float totalGameTime)
        {
            switch (state)
            {
                case State.SpeedDown:
                    if (Speed > minimumSpeed)
                        Speed -= speedCrement;
                    else
                        ChangeDirection(totalGameTime);
                    break;

                case State.SpeedUp:
                    if (Speed < maximumSpeed)
                        Speed += speedCrement;
                    else
                        state = State.Moving;
                    break;

                case State.DeSpawning:
                case State.Dying:
                    if (coroutines.Running)
                        coroutines.StopAll();
                    break;
            }

            if (state != State.Dying && state != State.DeSpawning)
            {
                if (coroutines.Running)
                    coroutines.Update();
            }
        }

    }
}
