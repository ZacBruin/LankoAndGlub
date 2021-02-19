using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Graphics;

namespace GP_Final
{
    public abstract class Target : Item
    {
        public int pointValue;
        protected int point_Anim_Count, updates_Between_Point;
        private int numDeathFlickers, currentFlicker, timeBetweenFlickers;

        private Color flickerLow, flickerNorm;
        protected Texture2D spawningSpriteSheet, movementSpriteSheet;

        SoundEffect GetDead;

        public Target(Game game) : base(game)
        {
            this.state = State.Spawning;

            this.currentFlicker = 0;
            this.numDeathFlickers = 20;
            timeBetweenFlickers = 0;
            updates_Between_Point = 4;

            this.color = Color.White;

            flickerLow = new Color(50, 50, 50, 50);
            flickerNorm = new Color(230, 230, 230, 230);

            
        }

        protected override void LoadContent()
        {
            GetDead = content.Load<SoundEffect>("SFX/BatHit");

            base.LoadContent();
        }

        protected void UpdateTarget(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }

        public void PlayHitSound()
        {
            GetDead.Play(.4f, 0, 0);
        }

        public bool DeathAnim()
        {
            if (this.state != State.Dying)
                return false;

            else
            {
                if (this.sheetInfo.currentFrame != 4)
                {
                    this.sheetInfo.currentFrame = 4;
                    this.sheetInfo.UpdateSourceFrame();
                    this.SourceRectangle = this.sheetInfo.sourceFrame;
                }

                if (this.currentFlicker != this.numDeathFlickers )
                {
                    if (timeBetweenFlickers == 1)
                    {
                        if (this.color == this.flickerNorm)
                            this.color = this.flickerLow;

                        else if (this.color == this.flickerLow)
                            this.color = this.flickerNorm;

                        currentFlicker++;
                        timeBetweenFlickers = 0;
                    }

                    else
                        timeBetweenFlickers++;

                    return false;
                }

                else
                    return true;
            }
        }

        protected override void UpdateItemSpriteSheet()
        {
            if (this.state != State.DeSpawning)
            {
                if (animationCount >= this.sheetInfo.updatesPerFrame)
                {
                    if (this.sheetInfo.currentFrame < this.sheetInfo.totalFrames - 1)
                        this.sheetInfo.currentFrame++;

                    animationCount = 0;

                    if (this.spriteTexture == this.movementSpriteSheet)
                    {
                        if (this.sheetInfo.currentFrame > this.sheetInfo.totalFrames - 2)
                            this.sheetInfo.currentFrame = 0;
                    }

                    this.sheetInfo.UpdateSourceFrame();
                    this.SourceRectangle = this.sheetInfo.sourceFrame;

                    if (this.spriteTexture == this.spawningSpriteSheet && this.sheetInfo.currentFrame == 4)
                        this.hasSpawned = true;
                }

                else
                {
                    animationCount++;
                    return;
                }
            }
        }

        public override bool SpawnInAnim()
        {
            if (this.state != State.Spawning)
                return false;
            else
            {
                if (this.hasSpawned)
                {
                    this.updates_Between_Frames = 6;
                    return true;
                }

                return false;
            }
        }

        public override bool SpawnOutAnim()
        {
            if (this.spriteTexture != this.spawningSpriteSheet)
            {
                this.spriteTexture = this.spawningSpriteSheet;
                this.sheetInfo.currentFrame = 4;
                this.sheetInfo.UpdateSourceFrame();
                this.SourceRectangle = this.sheetInfo.sourceFrame;
                this.updates_Between_Frames = 7;
            }

            if (this.state != State.DeSpawning)
                return false;

            else
            {
                if (animationCount >= this.sheetInfo.updatesPerFrame)
                {
                    this.sheetInfo.currentFrame--;
                    animationCount = 0;

                    this.sheetInfo.UpdateSourceFrame();
                    this.SourceRectangle = this.sheetInfo.sourceFrame;
                }

                else
                {
                    animationCount++;
                    return false;
                }

                if (this.sheetInfo.currentFrame == 0)
                    return true;

                else
                    return false;
            }
        }

    }
}
