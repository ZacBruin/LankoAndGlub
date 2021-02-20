using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
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
            state = State.Spawning;

            currentFlicker = 0;
            numDeathFlickers = 20;
            timeBetweenFlickers = 0;
            updates_Between_Point = 4;

            color = Color.White;

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
            if (state != State.Dying)
                return false;

            else
            {
                if (sheetInfo.currentFrame != 4)
                {
                    sheetInfo.currentFrame = 4;
                    sheetInfo.UpdateSourceFrame();
                    SourceRectangle = sheetInfo.sourceFrame;
                }

                if (currentFlicker != numDeathFlickers )
                {
                    if (timeBetweenFlickers == 1)
                    {
                        if (color == flickerNorm)
                            color = flickerLow;

                        else if (color == flickerLow)
                            color = flickerNorm;

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
            if (state != State.DeSpawning)
            {
                if (animationCount >= sheetInfo.updatesPerFrame)
                {
                    if (sheetInfo.currentFrame < sheetInfo.totalFrames - 1)
                        sheetInfo.currentFrame++;

                    animationCount = 0;

                    if (spriteTexture == movementSpriteSheet)
                    {
                        if (sheetInfo.currentFrame > sheetInfo.totalFrames - 2)
                            sheetInfo.currentFrame = 0;
                    }

                    sheetInfo.UpdateSourceFrame();
                    SourceRectangle = sheetInfo.sourceFrame;

                    if (spriteTexture == spawningSpriteSheet && sheetInfo.currentFrame == 4)
                        hasSpawned = true;
                }

                else
                {
                    animationCount++;
                    return;
                }
            }
        }

        public override bool SpawnAnimation()
        {
            if (state != State.Spawning)
                return false;
            else
            {
                if (hasSpawned)
                {
                    updatesBetweenFrames = 6;
                    return true;
                }

                return false;
            }
        }

        public override bool DespawnAnimation()
        {
            if (spriteTexture != spawningSpriteSheet)
            {
                spriteTexture = spawningSpriteSheet;
                sheetInfo.currentFrame = 4;
                sheetInfo.UpdateSourceFrame();
                SourceRectangle = sheetInfo.sourceFrame;
                updatesBetweenFrames = 7;
            }

            if (state != State.DeSpawning)
                return false;

            else
            {
                if (animationCount >= sheetInfo.updatesPerFrame)
                {
                    sheetInfo.currentFrame--;
                    animationCount = 0;

                    sheetInfo.UpdateSourceFrame();
                    SourceRectangle = sheetInfo.sourceFrame;
                }

                else
                {
                    animationCount++;
                    return false;
                }

                if (sheetInfo.currentFrame == 0)
                    return true;

                else
                    return false;
            }
        }

    }
}
