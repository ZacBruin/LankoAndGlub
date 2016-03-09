using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GP_Final
{
    public sealed class Green_PowerUp : PowerUp
    {
        public bool IsDamaged;
        public SpriteSheetInfo sheetInfo_Broken;

        private Texture2D broken, not_broken;

        public Green_PowerUp(Game game) : base(game)
        {
            this.MaxTimeOnScreen = 10;
            this.Speed = 150;
        }

        protected override void LoadContent()
        {
            not_broken = content.Load<Texture2D>("Green_SpriteSheet");
            broken = content.Load<Texture2D>("Green_Broken_SpriteSheet");

            sheetInfo = new SpriteSheetInfo(4, not_broken.Width, not_broken.Height, updates_Between_Frames);
            sheetInfo_Broken = new SpriteSheetInfo(4, broken.Width, broken.Height, updates_Between_Frames);

            spriteTexture = not_broken;

            this.SourceRectangle = sheetInfo.sourceFrame;
            this.spriteSheetFramesWide = sheetInfo.totalFrames;

            SetTranformAndRect();
            UpdateHitbox();

            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            if (this.IsDamaged == false)
                UpdateItemSpriteSheet();
            else
                UpdateItemSpriteSheet(sheetInfo_Broken);

            base.Update(gameTime);
        }

        public override bool CheckDamage()
        {
            if (IsDamaged)          
                return true;

            else
            {
                this.IsDamaged = true;
                this.animationCount = 0;
                spriteTexture = broken;
                this.SourceRectangle = sheetInfo_Broken.sourceFrame;
                return false;
            }
        }
    }
}
