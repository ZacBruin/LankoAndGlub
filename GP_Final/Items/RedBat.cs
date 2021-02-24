using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GP_Final
{
    public sealed class RedBat : Target
    {
        #region Consts
        //Assets
        private const string MOVING_SPRITE_SHEET = "SpriteSheets/RedBat";
        private const string SPAWNING_SPRITE_SHEET = "SpriteSheets/RedBatSpawn";

        //Numbers
        private const int SPEED = 110;
        private const int POINTS = 1;
        private const int MAX_SECONDS_ON_SCREEN = 6;
        private const int SPRITE_SHEET_FRAMES = 5;
        private const int UPDATES_PER_FRAME = 6;
        #endregion

        public RedBat(Game game) : base(game)
        {
            MaxTimeOnScreen = MAX_SECONDS_ON_SCREEN;
            PointValue = POINTS;
            Speed = SPEED;
        }

        protected override void LoadContent()
        {
            movementSpriteSheet = content.Load<Texture2D>(MOVING_SPRITE_SHEET);
            spawningSpriteSheet = content.Load<Texture2D>(SPAWNING_SPRITE_SHEET);

            spriteTexture = spawningSpriteSheet;
            SheetInfo = new SpriteSheetInfo(SPRITE_SHEET_FRAMES, spriteTexture.Width, spriteTexture.Height, UPDATES_PER_FRAME);

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
