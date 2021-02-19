using Microsoft.Xna.Framework;

namespace GP_Final
{
    public abstract class Item : DrawableSprite
    {
        public Vector2 center;

        //These times should be in units of seconds
        public float GameTimeWhenSpawned, CurrentTimeOnScreen, MaxTimeOnScreen;

        protected bool firstUpdate, hasSpawned, isDespawning;

        protected int updates_Between_Frames;

        protected int animationCount;
        public SpriteSheetInfo sheetInfo;

        public enum State { Spawning, DeSpawning, Dying, Moving, SpeedUp, SpeedDown };
        public State state;

        public Item (Game game) : base(game)
        {
            this.firstUpdate = true;
            this.scale = .25f;
            this.Direction = new Vector2(0, 0);
            this.updates_Between_Frames = 7;
            //this.ShowMarkers = true;
            this.color = new Color(0, 0, 0, 0);
            //this.color = new Color(256, 256, 256, 256);
            this.hasSpawned = false;
        }

        protected override void LoadContent()
        {
            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {          
            if(firstUpdate)
            {
                firstUpdate = false;
                GameTimeWhenSpawned = ((float)gameTime.TotalGameTime.TotalMilliseconds / 1000);
            }

            else
            {
                CurrentTimeOnScreen =
                    ((float)gameTime.TotalGameTime.TotalMilliseconds / 1000) - GameTimeWhenSpawned;
            }

            this.center = new Vector2
                (Location.X + (this.spriteTexture.Width * scale / (2 * this.spriteSheetFramesWide)),
                 Location.Y + (this.spriteTexture.Height * scale / 2));

            //Cyan PowerUp does not move
            if (this is CyanGem){ }
            else
                this.Location += (Vector2.Normalize(this.Direction) * (this.Speed) *
                    (float)gameTime.ElapsedGameTime.Milliseconds / 1000);
            
            if (this is GreenGem) { }
            else
            {
                if(this.state != State.Dying)
                    UpdateItemSpriteSheet();
            }

            UpdateHitbox();
            base.Update(gameTime);
        }

        protected virtual void UpdateItemSpriteSheet()
        {
            if (animationCount >= this.sheetInfo.updatesPerFrame)
            {
                this.sheetInfo.currentFrame++;
                animationCount = 0;

                if (this.sheetInfo.currentFrame > this.sheetInfo.totalFrames - 1)
                    this.sheetInfo.currentFrame = 0;

                this.sheetInfo.UpdateSourceFrame();
                this.SourceRectangle = this.sheetInfo.sourceFrame;
            }

            else
            {
                animationCount++;
                return;
            }
        }

        protected virtual void UpdateItemSpriteSheet(SpriteSheetInfo SSI)
        {
            if (animationCount >= SSI.updatesPerFrame)
            {
                SSI.currentFrame++;
                animationCount = 0;
            }

            else
            {
                animationCount++;
                return;
            }

            if (SSI.currentFrame > SSI.totalFrames - 1)
                SSI.currentFrame = 0;

            SSI.UpdateSourceFrame();
            this.SourceRectangle = SSI.sourceFrame;           
        }

        public void UpdateHitbox()
        {
            this.locationRect.Location = this.Location.ToPoint();

            float scaledHeight = sheetInfo.sourceFrame.Height * scale;
            float scaledWidth = sheetInfo.sourceFrame.Width * scale;

            if (this is Target)
            {
                int hitBoxWidthReduction = 12;
                int hitBoxHeightReduction = 5;

                this.Hitbox = new Rectangle(this.LocationRect.X + hitBoxWidthReduction, this.LocationRect.Y + hitBoxHeightReduction,
                    (int)scaledWidth - hitBoxWidthReduction*2, (int)scaledHeight - hitBoxHeightReduction*2);
            }

            else
                this.Hitbox = new Rectangle(this.LocationRect.X, this.LocationRect.Y,
                        (int)scaledWidth, (int)scaledHeight);
        }

        public virtual bool SpawnInAnim()
        {
            if (this.state != State.Spawning)
                return false;

            else
            {
                this.color.R += 7;
                this.color.G += 7;
                this.color.B += 7;
                this.color.A += 7;

                if (this.color.R == 252)
                {
                    this.color = new Color(255, 255, 255, 255);
                    //this.hasSpawned = true;
                    return true;
                }

                else
                    return false;
            }
        }

        public virtual bool SpawnOutAnim()
        {
            if (this.state != State.DeSpawning)
                return false;

            else
            {
                this.color.R -= 5;
                this.color.G -= 5;
                this.color.B -= 5;
                this.color.A -= 5;

                if (this.color.R == 0)
                    return true;

                else
                    return false;
            }
        }

    }
}
