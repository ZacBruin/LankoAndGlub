using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GP_Final
{
    public class PointSprite : DrawableSprite
    {
        private int pointAnimCount, updatesBeforeMove;

        private bool isMovingDown;

        private Texture2D pointSpriteSheet;
        private SpriteSheetInfo sheetInfo;
        private Vector2 startingPos;

        #region Consts
        //Assets
        private const string RED_BAT_POINT_SHEET = "SpriteSheets/RedBatPoints";
        private const string GOLD_BAT_POINT_SHEET = "SpriteSheets/GoldBatPoints";

        //Numbers
        private const float SPRITE_SCALE = .21f;
        private const int UPDATES_PER_FRAME = 9;
        private const int FRAMES_IN_SHEET = 6;
        private const int UPDATES_BETWEEN_BOUNCES = 7;
        private const int POINTS_BOUNCE_MOVEMENT_AMOUNT = 4;
        #endregion

        public PointSprite (Game game, bool IsOne) : base(game)
        {
            if(IsOne) 
            {
                color = new Color(255, 140, 140);
                pointSpriteSheet = content.Load<Texture2D>(RED_BAT_POINT_SHEET); 
            }

            else 
            {
                color = new Color(245, 215, 81);
                pointSpriteSheet = content.Load<Texture2D>(GOLD_BAT_POINT_SHEET); 
            }

            updatesBeforeMove = 5;

            Scale = SPRITE_SCALE;

            sheetInfo = new SpriteSheetInfo(FRAMES_IN_SHEET, pointSpriteSheet.Width, pointSpriteSheet.Height, UPDATES_PER_FRAME);
            spriteTexture = pointSpriteSheet;

            SetTranformAndRect();

            SourceRectangle = sheetInfo.SourceFrame;
            spriteSheetFramesWide = sheetInfo.TotalFrames;
        }

        protected override void LoadContent()
        {
            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            CycleAnim();

            if (updatesBeforeMove <= 0)
            {
                MakePointsBounce();
                updatesBeforeMove = UPDATES_BETWEEN_BOUNCES;
            }
            else
                updatesBeforeMove--;

            base.Update(gameTime);
        }

        private void CycleAnim()
        {
            if (sheetInfo.CurrentFrame == sheetInfo.TotalFrames - 1)
            {
                FadeOut();
                return;
            }              

            if (pointAnimCount >= UPDATES_PER_FRAME)
            {
                sheetInfo.CurrentFrame++;
                pointAnimCount = 0;

                sheetInfo.UpdateSourceFrame();
                SourceRectangle = sheetInfo.SourceFrame;
            }

            else
                pointAnimCount++;
        }

        public void SetStartPos()
        {
            if(startingPos == Vector2.Zero) 
                startingPos = Location;
        }

        private void FadeOut()
        {
            if (color.A != 0)
            {
                if (color.A > 200)
                {
                    color.R -= 1;
                    color.G -= 1;
                    color.B -= 1;
                    color.A -= 1;
                }

                else
                {
                    if (color.R > 5) color.R -= 5;
                    if (color.G > 5) color.G -= 5;
                    if (color.B > 5) color.B -= 5;
                    if (color.A > 5) color.A -= 5;
                }
            }
        }

        private void MakePointsBounce()
        {
            int movementAmount = POINTS_BOUNCE_MOVEMENT_AMOUNT;

            if (isMovingDown)
            {
                Location.Y += movementAmount;

                if (Location.Y == startingPos.Y + movementAmount)
                    isMovingDown = false;
            }

            else if (!isMovingDown)
            {
                Location.Y -= movementAmount;

                if (Location.Y == startingPos.Y - movementAmount)
                    isMovingDown = true;
            }
        }

    }
}
