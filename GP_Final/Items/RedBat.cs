using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GP_Final
{
    public sealed class RedBat : Target
    {
        public RedBat(Game game) : base(game)
        {
            MaxTimeOnScreen = 6;
            pointValue = 1;
            Speed = 110;
        }

        protected override void LoadContent()
        {
            movementSpriteSheet = content.Load<Texture2D>("SpriteSheets/RedBat");
            spawningSpriteSheet = content.Load<Texture2D>("SpriteSheets/RedBatSpawn");

            spriteTexture = spawningSpriteSheet;
            sheetInfo = new SpriteSheetInfo(5, spriteTexture.Width, spriteTexture.Height, 6);

            SourceRectangle = sheetInfo.sourceFrame;
            spriteSheetFramesWide = sheetInfo.totalFrames;

            SetTranformAndRect();
            UpdateHitbox();

            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            if (hasSpawned)
            {
                spriteTexture = movementSpriteSheet;
                sheetInfo.currentFrame = 0;
                sheetInfo.UpdateSourceFrame();
                SourceRectangle = sheetInfo.sourceFrame;
                hasSpawned = false;
            }

            base.Update(gameTime);
        }
    }
}
