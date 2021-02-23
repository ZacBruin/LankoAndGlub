using Microsoft.Xna.Framework;

namespace GP_Final
{
    public abstract class Item : DrawableSprite
    {
        public Vector2 Center;
        public SpriteSheetInfo SheetInfo;

        public enum State
        {
            Spawning,
            DeSpawning,
            Dying,
            Moving,
            SpeedUp,
            SpeedDown
        };
        public State state;

        //These times should be in units of seconds
        public float GameTimeWhenSpawned, 
                     CurrentTimeOnScreen, 
                     MaxTimeOnScreen;

        protected bool firstUpdate, 
                       hasSpawned, 
                       isDespawning;

        protected int animationCount;

        protected int updatesPerFrame;

        public Item (Game game) : base(game)
        {
            firstUpdate = true;
            scale = .25f;
            Direction = new Vector2(0, 0);
            updatesPerFrame = 7;
            color = new Color(0, 0, 0, 0);
            hasSpawned = false;
        }

        protected override void LoadContent()
        {
            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            float totalGameTime = (float)gameTime.TotalGameTime.TotalMilliseconds;          

            if (firstUpdate)
            {
                firstUpdate = false;
                GameTimeWhenSpawned = (totalGameTime / 1000);
            }

            else
                CurrentTimeOnScreen = (totalGameTime / 1000) - GameTimeWhenSpawned;

            Center = new Vector2
            (
                Location.X + (spriteTexture.Width * scale / (2 * spriteSheetFramesWide)),
                Location.Y + (spriteTexture.Height * scale / 2)
            );

            //Cyan PowerUp does not move
            if (!(this is CyanGem))
                Location += (Vector2.Normalize(Direction) * (Speed) * gameTime.ElapsedGameTime.Milliseconds / 1000);           

            if (!(this is GreenGem) && state != State.Dying)
                UpdateItemSpriteSheet();

            UpdateHitbox();
            base.Update(gameTime);
        }

        protected virtual void UpdateItemSpriteSheet()
        {
            if (animationCount >= SheetInfo.UpdatesPerFrame)
            {
                SheetInfo.CurrentFrame++;
                animationCount = 0;

                if (SheetInfo.CurrentFrame > SheetInfo.TotalFrames - 1)
                    SheetInfo.CurrentFrame = 0;

                SheetInfo.UpdateSourceFrame();
                SourceRectangle = SheetInfo.SourceFrame;
            }

            else
                animationCount++;
        }

        protected virtual void UpdateItemSpriteSheet(SpriteSheetInfo info)
        {
            if (animationCount >= info.UpdatesPerFrame)
            {
                info.CurrentFrame++;
                animationCount = 0;
            }

            else
            {
                animationCount++;
                return;
            }

            if (info.CurrentFrame > info.TotalFrames - 1)
                info.CurrentFrame = 0;

            info.UpdateSourceFrame();
            SourceRectangle = info.SourceFrame;           
        }

        public void UpdateHitbox()
        {
            locationRect.Location = Location.ToPoint();

            float scaledHeight = SheetInfo.SourceFrame.Height * scale;
            float scaledWidth = SheetInfo.SourceFrame.Width * scale;

            if (this is Target)
            {
                int hitBoxWidthReduction = 12;
                int hitBoxHeightReduction = 5;

                Hitbox = new Rectangle(
                    LocationRect.X + hitBoxWidthReduction, 
                    LocationRect.Y + hitBoxHeightReduction,
                    (int)scaledWidth - hitBoxWidthReduction*2, 
                    (int)scaledHeight - hitBoxHeightReduction*2
                );
            }

            else
                Hitbox = new Rectangle(LocationRect.X, LocationRect.Y, (int)scaledWidth, (int)scaledHeight);
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
