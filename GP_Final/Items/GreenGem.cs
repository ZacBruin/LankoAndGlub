using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace GP_Final
{
    public sealed class GreenGem : PowerUp
    {
        private bool isDamaged;

        private SpriteSheetInfo sheetInfo_Broken;
        private Texture2D broken, not_broken;
        private SoundEffect emeraldGet, emeraldBreak;

        private const float speed = 150;
        private const float maxTimeOnScreen = 10;
        private const int numSheetFrames = 4;
        private const float getVolume = .3f;
        private const float breakVolume = .2f;

        private const string notBrokenSpriteSheet = "SpriteSheets/GreenGem";
        private const string brokenSpriteSheet = "SpriteSheets/GreenGemBroken";
        private const string getSFX = "SFX/PowerupGet";
        private const string breakSFX = "SFX/PowerupBreak";

        public GreenGem(Game game) : base(game)
        {
            MaxTimeOnScreen = maxTimeOnScreen;
            Speed = speed;
        }

        protected override void LoadContent()
        {
            not_broken = content.Load<Texture2D>(notBrokenSpriteSheet);
            broken = content.Load<Texture2D>(brokenSpriteSheet);

            emeraldGet = content.Load<SoundEffect>(getSFX);
            emeraldBreak = content.Load<SoundEffect>(breakSFX);

            SheetInfo = new SpriteSheetInfo(numSheetFrames, not_broken.Width, not_broken.Height, updatesPerFrame);
            sheetInfo_Broken = new SpriteSheetInfo(numSheetFrames, broken.Width, broken.Height, updatesPerFrame);

            spriteTexture = not_broken;

            SourceRectangle = SheetInfo.SourceFrame;
            spriteSheetFramesWide = SheetInfo.TotalFrames;

            SetTranformAndRect();
            UpdateHitbox();

            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            if (isDamaged == false)
                UpdateItemSpriteSheet();
            else
                UpdateItemSpriteSheet(sheetInfo_Broken);

            base.Update(gameTime);
        }

        public override bool CheckDamage()
        {
            if (isDamaged)
            {
                emeraldGet.Play(getVolume, 0, 0);
                return true;
            }

            else
            {
                emeraldBreak.Play(breakVolume, 0, 0);
                isDamaged = true;
                animationCount = 0;
                spriteTexture = broken;
                SourceRectangle = sheetInfo_Broken.SourceFrame;
                return false;
            }
        }
    }
}
