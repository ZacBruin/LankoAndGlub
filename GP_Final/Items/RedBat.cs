using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GP_Final
{
    public sealed class RedBat : Target
    {
        private const int speed = 110;
        private const int pointVal = 1;
        private const int maxTimeOnScreen = 6;

        private const string moveSpriteSheet = "SpriteSheets/RedBat";
        private const string spawnSpriteSheet = "SpriteSheets/RedBatSpawn";

        private const int spriteSheetFrames = 5;

        public RedBat(Game game) : base(game)
        {
            MaxTimeOnScreen = maxTimeOnScreen;
            pointValue = pointVal;
            Speed = speed;
            updatesPerFrame = 6;
        }

        protected override void LoadContent()
        {
            movementSpriteSheet = content.Load<Texture2D>(moveSpriteSheet);
            spawningSpriteSheet = content.Load<Texture2D>(spawnSpriteSheet);

            spriteTexture = spawningSpriteSheet;
            SheetInfo = new SpriteSheetInfo(spriteSheetFrames, spriteTexture.Width, spriteTexture.Height, updatesPerFrame);

            SourceRectangle = SheetInfo.SourceFrame;
            spriteSheetFramesWide = SheetInfo.TotalFrames;

            SetTranformAndRect();
            UpdateHitbox();

            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            if (hasSpawned)
            {
                spriteTexture = movementSpriteSheet;
                SheetInfo.CurrentFrame = 0;
                SheetInfo.UpdateSourceFrame();
                SourceRectangle = SheetInfo.SourceFrame;
                hasSpawned = false;
            }

            base.Update(gameTime);
        }
    }
}
