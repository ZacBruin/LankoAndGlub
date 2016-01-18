using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GP_Final
{
    public abstract class Target : Item
    {
        public int pointValue;
        protected int point_Anim_Count, updates_Between_Point;
        private int numDeathFlickers, currentFlicker, timeBetweenFlickers;

        private Color flickerLow, flickerNorm;
        protected Texture2D spawningSpriteSheet, movementSpriteSheet, pointSpriteSheet;

        protected float point_Scale;
        protected SpriteSheetInfo point_info;

        public Target(Game game) : base(game)
        {
            this.state = State.Spawning;

            this.currentFlicker = 0;
            this.numDeathFlickers = 20;
            timeBetweenFlickers = 0;
            updates_Between_Point = 4;

            this.color = Color.White;

            this.point_Scale = .2f;

            flickerLow = new Color(50, 50, 50, 50);
            flickerNorm = new Color(230, 230, 230, 230);
        }

        protected void UpdateTarget(GameTime gameTime)
        {
            base.Update(gameTime);
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

                        //this.Location += new Vector2(.85f, .45f);
                        //this.scale -= .008f;

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

        //protected void CyclepointAnim()
        //{
        //    if (point_info.currentFrame == point_info.totalFrames)
        //        return;

        //    if (point_Anim_Count >= this.updates_Between_Point)
        //    {
        //        point_info.currentFrame++;
        //        point_Anim_Count = 0;

        //        point_info.UpdateSourceFrame();
        //    }

        //    else
        //    {
        //        point_Anim_Count++;
        //        return;
        //    }
        //}

    }
}
