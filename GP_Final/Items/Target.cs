using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace GP_Final
{
    public abstract class Target : Item
    {
        public int PointValue;
        protected int pointAnimCount, updatesPerPointFrame;

        private int deathFlickerFrames, 
                    currentFlickerFrame, 
                    timePerFlicker;

        private Color flickerLow, flickerNorm;
        protected Texture2D spawningSpriteSheet, movementSpriteSheet;

        private SoundEffect batDeath;
        private const string batDeathSFX = "SFX/BatHit";

        public Target(Game game) : base(game)
        {
            state = State.Spawning;

            currentFlickerFrame = 0;
            deathFlickerFrames = 20;
            timePerFlicker = 0;
            updatesPerPointFrame = 4;

            color = Color.White;

            flickerLow = new Color(50, 50, 50, 50);
            flickerNorm = new Color(230, 230, 230, 230);  
        }

        protected override void LoadContent()
        {
            batDeath = content.Load<SoundEffect>(batDeathSFX);
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
            batDeath.Play(.4f, 0, 0);
        }

        public bool DeathAnim()
        {
            if (state != State.Dying)
                return false;

            else
            {
                if (SheetInfo.CurrentFrame != 4)
                {
                    SheetInfo.CurrentFrame = 4;
                    SheetInfo.UpdateSourceFrame();
                    SourceRectangle = SheetInfo.SourceFrame;
                }

                if (currentFlickerFrame != deathFlickerFrames )
                {
                    if (timePerFlicker == 1)
                    {
                        if (color == flickerNorm)
                            color = flickerLow;

                        else if (color == flickerLow)
                            color = flickerNorm;

                        currentFlickerFrame++;
                        timePerFlicker = 0;
                    }

                    else
                        timePerFlicker++;

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
                if (animationCount >= SheetInfo.UpdatesPerFrame)
                {
                    if (SheetInfo.CurrentFrame < SheetInfo.TotalFrames - 1)
                        SheetInfo.CurrentFrame++;

                    animationCount = 0;

                    if (spriteTexture == movementSpriteSheet)
                    {
                        if (SheetInfo.CurrentFrame > SheetInfo.TotalFrames - 2)
                            SheetInfo.CurrentFrame = 0;
                    }

                    SheetInfo.UpdateSourceFrame();
                    SourceRectangle = SheetInfo.SourceFrame;

                    if (spriteTexture == spawningSpriteSheet && SheetInfo.CurrentFrame == 4)
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
                    updatesPerFrame = 6;
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
                SheetInfo.CurrentFrame = 4;
                SheetInfo.UpdateSourceFrame();
                SourceRectangle = SheetInfo.SourceFrame;
                updatesPerFrame = 7;
            }

            if (state != State.DeSpawning)
                return false;

            else
            {
                if (animationCount >= SheetInfo.UpdatesPerFrame)
                {
                    SheetInfo.CurrentFrame--;
                    animationCount = 0;

                    SheetInfo.UpdateSourceFrame();
                    SourceRectangle = SheetInfo.SourceFrame;
                }

                else
                {
                    animationCount++;
                    return false;
                }

                if (SheetInfo.CurrentFrame == 0)
                    return true;

                else
                    return false;
            }
        }

    }
}
