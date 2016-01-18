using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GP_Final
{
    public sealed class Basic_Target : Target
    {
        public Basic_Target(Game game) : base(game)
        {
            this.MaxTimeOnScreen = 6;
            this.pointValue = 1;
            this.Speed = 110;
        }

        protected override void LoadContent()
        {
            this.movementSpriteSheet = content.Load<Texture2D>("Red_Bat_SpriteSheet");
            this.spawningSpriteSheet = content.Load<Texture2D>("Red_Bat_SpawnAnim_SpriteSheet");
            this.pointSpriteSheet = content.Load<Texture2D>("Red_Bat_Point_SpriteSheet");

            this.spriteTexture = this.spawningSpriteSheet;
            sheetInfo = new SpriteSheetInfo(5, spriteTexture.Width, spriteTexture.Height, 6);

            point_info = new SpriteSheetInfo(6, pointSpriteSheet.Width, pointSpriteSheet.Height, 5);

            this.SourceRectangle = sheetInfo.sourceFrame;
            this.spriteSheetFramesWide = sheetInfo.totalFrames;

            SetTranformAndRect();
            UpdateHitbox();

            base.LoadContent();
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();

            if (this.state == State.Dying)
            {
                spriteBatch.Draw(this.pointSpriteSheet, new Vector2(this.center.X, (this.center.Y - 10)), 
                    this.point_info.sourceFrame, Color.White, 0f, new Vector2(0, 0), this.point_Scale, SpriteEffects.None, 0f);

                CyclepointAnim();
            }

            spriteBatch.End();
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

            this.Draw(gameTime);

            base.Update(gameTime);
        }

        protected void CyclepointAnim()
        {
            if (point_info.currentFrame == point_info.totalFrames)
                return;

            if (point_Anim_Count >= this.updates_Between_Point)
            {
                point_info.currentFrame++;
                point_Anim_Count = 0;

                point_info.UpdateSourceFrame();
            }

            else
            {
                point_Anim_Count++;
                return;
            }
        }



    }
}
