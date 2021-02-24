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

        #region Consts
        //Assets
        private const string NOT_BROKEN_SPRITE_SHEET = "SpriteSheets/GreenGem";
        private const string BROKEN_SPRITE_SHEET = "SpriteSheets/GreenGemBroken";
        private const string GET_SFX = "SFX/PowerupGet";
        private const string BREAK_SFX = "SFX/PowerupBreak";

        //Numbers
        private const float SPEED = 150;
        private const float MAX_SECONDS_ON_SCREEN = 10;
        private const int SPRITE_SHEET_FRAMES = 4;
        private const float GET_SFX_VOL = .3f;
        private const float BREAK_SFX_VOL = .2f;
        #endregion

        public GreenGem(Game game) : base(game)
        {
            MaxTimeOnScreen = MAX_SECONDS_ON_SCREEN;
            Speed = SPEED;
        }

        protected override void LoadContent()
        {
            not_broken = content.Load<Texture2D>(NOT_BROKEN_SPRITE_SHEET);
            broken = content.Load<Texture2D>(BROKEN_SPRITE_SHEET);

            emeraldGet = content.Load<SoundEffect>(GET_SFX);
            emeraldBreak = content.Load<SoundEffect>(BREAK_SFX);

            SheetInfo = new SpriteSheetInfo(SPRITE_SHEET_FRAMES, not_broken.Width, not_broken.Height, updatesPerFrame);
            sheetInfo_Broken = new SpriteSheetInfo(SPRITE_SHEET_FRAMES, broken.Width, broken.Height, updatesPerFrame);

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
                emeraldGet.Play(GET_SFX_VOL, 0, 0);
                return true;
            }

            else
            {
                emeraldBreak.Play(BREAK_SFX_VOL, 0, 0);
                isDamaged = true;
                animationCount = 0;
                spriteTexture = broken;
                SourceRectangle = sheetInfo_Broken.SourceFrame;
                return false;
            }
        }
    }
}
