using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GP_Final
{
    public sealed class RedBat : Target
    {
        public RedBat(Game game) : base(game)
        {
            this.MaxTimeOnScreen = 6;
            this.pointValue = 1;
            this.Speed = 110;
        }

        protected override void LoadContent()
        {
            this.movementSpriteSheet = content.Load<Texture2D>("SpriteSheets/RedBat");
            this.spawningSpriteSheet = content.Load<Texture2D>("SpriteSheets/RedBatSpawn");

            this.spriteTexture = this.spawningSpriteSheet;
            sheetInfo = new SpriteSheetInfo(5, spriteTexture.Width, spriteTexture.Height, 6);

            this.SourceRectangle = sheetInfo.sourceFrame;
            this.spriteSheetFramesWide = sheetInfo.totalFrames;

            SetTranformAndRect();
            UpdateHitbox();

            base.LoadContent();
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

            base.Update(gameTime);
        }
    }
}
