using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GP_Final
{
    public sealed class Cyan_PowerUp : PowerUp
    {
        public Cyan_PowerUp(Game game) : base (game)
        {
            this.scale = .18f;
            this.MaxTimeOnScreen = 12;
        }

        protected override void LoadContent()
        {
            spriteTexture = content.Load<Texture2D>("Cyan_SpriteSheet");
            sheetInfo = new SpriteSheetInfo(4, spriteTexture.Width, spriteTexture.Height, updates_Between_Frames);

            this.SourceRectangle = sheetInfo.sourceFrame;
            this.spriteSheetFramesWide = sheetInfo.totalFrames;

            SetTranformAndRect();
            UpdateHitbox();

            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {           
            base.Update(gameTime);
        }

    }
}
