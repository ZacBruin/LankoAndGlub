using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GP_Final
{
    public sealed class CyanGem : PowerUp
    {
        private const float SPRITE_SCALE = .18f;
        private const float MAX_SECONDS_ON_SCREEN = 12;
        private const int SPRITE_SHEET_FRAMES = 4;
        private const string SPRITE_SHEET = "SpriteSheets/CyanGem";

        public CyanGem(Game game) : base (game)
        {
            scale = SPRITE_SCALE;
            MaxTimeOnScreen = MAX_SECONDS_ON_SCREEN;
        }

        protected override void LoadContent()
        {
            spriteTexture = content.Load<Texture2D>(SPRITE_SHEET);
            SheetInfo = new SpriteSheetInfo(SPRITE_SHEET_FRAMES, spriteTexture.Width, spriteTexture.Height, updatesPerFrame);

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
