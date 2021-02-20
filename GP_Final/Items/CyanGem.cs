using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GP_Final
{
    public sealed class CyanGem : PowerUp
    {
        public CyanGem(Game game) : base (game)
        {
            scale = .18f;
            MaxTimeOnScreen = 12;
        }

        protected override void LoadContent()
        {
            spriteTexture = content.Load<Texture2D>("SpriteSheets/CyanGem");
            sheetInfo = new SpriteSheetInfo(4, spriteTexture.Width, spriteTexture.Height, updatesBetweenFrames);

            SourceRectangle = sheetInfo.sourceFrame;
            spriteSheetFramesWide = sheetInfo.totalFrames;

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
