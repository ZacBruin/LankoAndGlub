using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Graphics;

namespace GP_Final
{
    public sealed class GreenGem : PowerUp
    {
        public bool IsDamaged;
        public SpriteSheetInfo sheetInfo_Broken;

        private Texture2D broken, not_broken;

        SoundEffect EmeraldGet, EmeraldBreak;

        public GreenGem(Game game) : base(game)
        {
            this.MaxTimeOnScreen = 10;
            this.Speed = 150;
        }

        protected override void LoadContent()
        {
            not_broken = content.Load<Texture2D>("SpriteSheets/GreenGem");
            broken = content.Load<Texture2D>("SpriteSheets/GreenGemBroken");

            EmeraldGet = content.Load<SoundEffect>("SFX/PowerupGet");
            EmeraldBreak = content.Load<SoundEffect>("SFX/PowerupBreak");

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
            {
                EmeraldGet.Play(.3f, 0, 0);
                return true;
            }

            else
            {
                EmeraldBreak.Play(.2f, 0, 0);
                this.IsDamaged = true;
                this.animationCount = 0;
                spriteTexture = broken;
                this.SourceRectangle = sheetInfo_Broken.sourceFrame;
                return false;
            }
        }
    }
}
