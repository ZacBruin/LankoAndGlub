using Microsoft.Xna.Framework;

namespace GP_Final
{
    public abstract class Item : DrawableSprite
    {
        public Vector2 center;

        //These times should be in units of seconds
        public float GameTimeWhenSpawned, CurrentTimeOnScreen, MaxTimeOnScreen;

        protected bool firstUpdate, hasSpawned, isDespawning;

        protected int updatesBetweenFrames;

        protected int animationCount;
        public SpriteSheetInfo sheetInfo;

        public enum State { Spawning, DeSpawning, Dying, Moving, SpeedUp, SpeedDown };
        public State state;

        public Item (Game game) : base(game)
        {
            firstUpdate = true;
            scale = .25f;
            Direction = new Vector2(0, 0);
            updatesBetweenFrames = 7;
            color = new Color(0, 0, 0, 0);
            hasSpawned = false;
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

            center = new Vector2
                (Location.X + (spriteTexture.Width * scale / (2 * spriteSheetFramesWide)),
                 Location.Y + (spriteTexture.Height * scale / 2));

            //Cyan PowerUp does not move
            if (this is CyanGem){ }
            else
                Location += (Vector2.Normalize(Direction) * (Speed) *
                    gameTime.ElapsedGameTime.Milliseconds / 1000);
            
            if (this is GreenGem) { }
            else
            {
                if(state != State.Dying)
                    UpdateItemSpriteSheet();
            }

            UpdateHitbox();
            base.Update(gameTime);
        }

        protected virtual void UpdateItemSpriteSheet()
        {
            if (animationCount >= sheetInfo.updatesPerFrame)
            {
                sheetInfo.currentFrame++;
                animationCount = 0;

                if (sheetInfo.currentFrame > sheetInfo.totalFrames - 1)
                    sheetInfo.currentFrame = 0;

                sheetInfo.UpdateSourceFrame();
                SourceRectangle = sheetInfo.sourceFrame;
            }

            else
            {
                animationCount++;
                return;
            }
        }

        protected virtual void UpdateItemSpriteSheet(SpriteSheetInfo ssi)
        {
            if (animationCount >= ssi.updatesPerFrame)
            {
                ssi.currentFrame++;
                animationCount = 0;
            }

            else
            {
                animationCount++;
                return;
            }

            if (ssi.currentFrame > ssi.totalFrames - 1)
                ssi.currentFrame = 0;

            ssi.UpdateSourceFrame();
            SourceRectangle = ssi.sourceFrame;           
        }

        public void UpdateHitbox()
        {
            locationRect.Location = Location.ToPoint();

            float scaledHeight = sheetInfo.sourceFrame.Height * scale;
            float scaledWidth = sheetInfo.sourceFrame.Width * scale;

            if (this is Target)
            {
                int hitBoxWidthReduction = 12;
                int hitBoxHeightReduction = 5;

                Hitbox = new Rectangle(LocationRect.X + hitBoxWidthReduction, LocationRect.Y + hitBoxHeightReduction,
                    (int)scaledWidth - hitBoxWidthReduction*2, (int)scaledHeight - hitBoxHeightReduction*2);
            }

            else
                Hitbox = new Rectangle(LocationRect.X, LocationRect.Y,
                        (int)scaledWidth, (int)scaledHeight);
        }

        public virtual bool SpawnAnimation()
        {
            if (state != State.Spawning)
                return false;

            else
            {
                color.R += 7;
                color.G += 7;
                color.B += 7;
                color.A += 7;

                if (color.R == 252)
                {
                    color = new Color(255, 255, 255, 255);
                    return true;
                }

                else
                    return false;
            }
        }

        public virtual bool DespawnAnimation()
        {
            if (state != State.DeSpawning)
                return false;

            else
            {
                color.R -= 5;
                color.G -= 5;
                color.B -= 5;
                color.A -= 5;

                if (color.R == 0)
                    return true;

                else
                    return false;
            }
        }

    }
}
