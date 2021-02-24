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

        private int directionChanges, maxDirectionChanges;

        private float 
            lastTimeDirectionChanged, 
            timeRequiredBeforeDirectionChange, 
            timeBetweenChecks;

        #region Consts
        //Assets
        private const string MOVING_SPRITE_SHEET = "SpriteSheets/GoldBat";
        private const string SPAWNING_SPRITE_SHEET = "SpriteSheets/GoldBatSpawn";

        //Numbers
        private const int SPRITE_SHEET_FRAMES = 5;
        private const int MAX_SECONDS_ON_SCREEN = 6;
        private const int POINTS = 3;

        private const float BASE_SPEED = 250;
        private const float LOWEST_SPEED = 100;
        private const float HIGHEST_SPEED = 250;
        private const float SPEED_CREMENT = 2;
        #endregion

        public GoldenBat(Game game) : base(game)
        {
            MaxTimeOnScreen = MAX_SECONDS_ON_SCREEN;
            PointValue = POINTS;
            Speed = BASE_SPEED;

            timeBetweenChecks = .1f;
            timeRequiredBeforeDirectionChange = 1.0f;

            rand = new Random();
            coroutines = new Coroutine();
        }

        protected override void LoadContent()
        {
            movementSpriteSheet = content.Load<Texture2D>(MOVING_SPRITE_SHEET);
            spawningSpriteSheet = content.Load<Texture2D>(SPAWNING_SPRITE_SHEET);

            spriteTexture = spawningSpriteSheet;
            SheetInfo = new SpriteSheetInfo(SPRITE_SHEET_FRAMES, spriteTexture.Width, spriteTexture.Height, updatesPerFrame);

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
                    if (Speed > LOWEST_SPEED)
                        Speed -= SPEED_CREMENT;
                    else
                        ChangeDirection(totalGameTime);
                    break;

                case State.SpeedUp:
                    if (Speed < HIGHEST_SPEED)
                        Speed += SPEED_CREMENT;
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
