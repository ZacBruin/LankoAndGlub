using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GP_Final
{
    public sealed class CyanGem : PowerUp
    {
        private const float spriteScale = .18f;
        private const float maxTimeOnScreen = 12;
        private const int cyanSheetFrames = 4;
        private const string spriteSheet = "SpriteSheets/CyanGem";

        public CyanGem(Game game) : base (game)
        {
            scale = spriteScale;
            MaxTimeOnScreen = maxTimeOnScreen;
        }

        protected override void LoadContent()
        {
            spriteTexture = content.Load<Texture2D>(spriteSheet);
            SheetInfo = new SpriteSheetInfo(cyanSheetFrames, spriteTexture.Width, spriteTexture.Height, updatesPerFrame);

            SourceRectangle = SheetInfo.SourceFrame;
            spriteSheetFramesWide = SheetInfo.TotalFrames;

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
