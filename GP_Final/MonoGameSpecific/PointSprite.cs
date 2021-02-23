using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GP_Final
{
    public class PointSprite : DrawableSprite
    {
        public int updatesBetweenFrames;
        public int pointAnimCount;
        private bool IsMovingDown;
        private int updatesBeforeMove = 5;

        private Texture2D pointSpriteSheet;
        private SpriteSheetInfo sheetInfo;
        private Vector2 startingPos;

        public PointSprite (Game game, bool IsOne) : base(game)
        {
            if(IsOne) 
            {
                color = new Color(255, 140, 140);
                pointSpriteSheet = content.Load<Texture2D>("SpriteSheets/RedBatPoints"); 
            }

            else 
            {
                color = new Color(245, 215, 81);
                pointSpriteSheet = content.Load<Texture2D>("SpriteSheets/GoldBatPoints"); 
            }

            Scale = .21f;
            updatesBetweenFrames = 9;

            sheetInfo = new SpriteSheetInfo(6, pointSpriteSheet.Width, pointSpriteSheet.Height, updatesBetweenFrames);
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

            if (updatesBeforeMove <= 0) { Bounce(4); updatesBeforeMove = 7; }
            else updatesBeforeMove--;

            base.Update(gameTime);
        }

        private void CycleAnim()
        {
            if (sheetInfo.CurrentFrame == sheetInfo.TotalFrames - 1)
            {
                FadeOut();
                return;
            }              

            if (pointAnimCount >= updatesBetweenFrames)
            {
                sheetInfo.CurrentFrame++;
                pointAnimCount = 0;

                sheetInfo.UpdateSourceFrame();
                SourceRectangle = sheetInfo.SourceFrame;
            }

            else
            {
                pointAnimCount++;
                return;
            }
        }

        public void SetStartPos()
        {
            if(startingPos == new Vector2(0,0)) 
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

        private void Bounce(int MovementAmt)
        {
            if(IsMovingDown)
            {
                Location.Y += MovementAmt;

                if (Location.Y == startingPos.Y + MovementAmt)
                    IsMovingDown = false;
            }

            else if (!IsMovingDown)
            {
                Location.Y -= MovementAmt;

                if (Location.Y == startingPos.Y - MovementAmt)
                    IsMovingDown = true;
            }
        }

    }
}
