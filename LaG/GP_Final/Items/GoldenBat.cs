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

        private int numTimesChangedDir, maxAllowedDirChanges;
        private float timeChangedDir, minTimeBeforeDirChange, timeBetweenChecks;

        public GoldenBat(Game game) : base(game)
        {
            this.MaxTimeOnScreen = 4f;
            this.pointValue = 3;
            this.Speed = 250;

            timeBetweenChecks = .1f;
            minTimeBeforeDirChange = 1.0f;

            rand = new Random();
            coroutines = new Coroutine();
        }

        protected override void LoadContent()
        {
            this.movementSpriteSheet = content.Load<Texture2D>("SpriteSheets/GoldBat");
            this.spawningSpriteSheet = content.Load<Texture2D>("SpriteSheets/GoldBatSpawn");

            this.spriteTexture = this.spawningSpriteSheet;
            sheetInfo = new SpriteSheetInfo(5, spriteTexture.Width, spriteTexture.Height, updates_Between_Frames);

            this.SourceRectangle = sheetInfo.sourceFrame;
            this.spriteSheetFramesWide = sheetInfo.totalFrames;

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
            if (this.hasSpawned)
            {
                this.spriteTexture = this.movementSpriteSheet;
                this.sheetInfo.currentFrame = 0;
                this.sheetInfo.UpdateSourceFrame();
                this.SourceRectangle = this.sheetInfo.sourceFrame;
                this.hasSpawned = false;
            }

            if (coroutines.Count == 0)
                if(this.CurrentTimeOnScreen <= this.MaxTimeOnScreen)
                    coroutines.Start(DirectionChangeDelay(gameTime));

            switch (this.state)
            {
                case State.SpeedDown:
                    if (this.Speed > 100)
                        this.Speed -= 2;
                    else
                        ChangeDirection(gameTime);
                        break;

                case State.SpeedUp:
                        if (this.Speed < 250)
                            this.Speed += 2;
                        else
                            this.state = State.Moving;
                        break;   
            
                case State.DeSpawning:
                case State.Dying:
                    if(this.coroutines.Running)
                        this.coroutines.StopAll();
                        break;
            }

            if (this.state != State.Dying && this.state != State.DeSpawning)
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
            if (CurrentTimeOnScreen <= this.minTimeBeforeDirChange)
                yield return coroutines.Pause(this.minTimeBeforeDirChange);

            else
            {
                yield return coroutines.Pause(this.timeBetweenChecks);
                CheckForDirectionChange(gameTime);
            }
        }

        private void GetNewDirection(GameTime gameTime)
        {          
            do
            {
                if (this.Direction.X >= 0)
                    this.newDirection.X = rand.Next(-100, -30);
                else
                    this.newDirection.X = rand.Next(30, 100);

                this.newDirection.Y = rand.Next(-20, 20);
            }
            while (Math.Abs(this.newDirection.Y) > Math.Abs(this.newDirection.X));

            this.state = State.SpeedDown;
        }

        private void ChangeDirection(GameTime gameTime)
        {
            this.Direction = this.newDirection;

            if (this.Direction.X > 0)
                this.SpriteEffects = SpriteEffects.FlipHorizontally;
            else
                this.SpriteEffects = SpriteEffects.None;

            this.state = State.SpeedUp;

            timeChangedDir = (float)gameTime.TotalGameTime.TotalMilliseconds / 1000;
            numTimesChangedDir++;
        }

    }
}
