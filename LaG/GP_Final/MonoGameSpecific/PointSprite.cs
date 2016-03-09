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
                this.color = new Color(255, 140, 140);
                this.pointSpriteSheet = content.Load<Texture2D>("Red_Bat_Point_SpriteSheet"); 
            }

            else 
            {
                this.color = new Color(245, 215, 81);
                this.pointSpriteSheet = content.Load<Texture2D>("Gold_Bat_Point_SpriteSheet"); 
            }

            Scale = .21f;
            updatesBetweenFrames = 9;

            this.sheetInfo = new SpriteSheetInfo(6, pointSpriteSheet.Width, pointSpriteSheet.Height, updatesBetweenFrames);
            spriteTexture = pointSpriteSheet;

            SetTranformAndRect();

            this.SourceRectangle = sheetInfo.sourceFrame;
            this.spriteSheetFramesWide = sheetInfo.totalFrames;
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
            if (sheetInfo.currentFrame == sheetInfo.totalFrames - 1)
            {
                FadeOut();
                return;
            }              

            if (pointAnimCount >= this.updatesBetweenFrames)
            {
                sheetInfo.currentFrame++;
                pointAnimCount = 0;

                sheetInfo.UpdateSourceFrame();
                this.SourceRectangle = sheetInfo.sourceFrame;
            }

            else
            {
                pointAnimCount++;
                return;
            }
        }

        public void SetStartPos()
        {
            if(this.startingPos == new Vector2(0,0)) 
                this.startingPos = this.Location;
        }

        private void FadeOut()
        {
            if (this.color.A != 0)
            {
                if (this.color.A > 200)
                {
                    this.color.R -= 1;
                    this.color.G -= 1;
                    this.color.B -= 1;
                    this.color.A -= 1;
                }

                else
                {
                    if (color.R > 5) this.color.R -= 5;
                    if (color.G > 5) this.color.G -= 5;
                    if (color.B > 5) this.color.B -= 5;
                    if (color.A > 5) this.color.A -= 5;
                }
            }
        }

        private void Bounce(int MovementAmt)
        {
            if(IsMovingDown)
            {
                this.Location.Y += MovementAmt;

                if (Location.Y == startingPos.Y + MovementAmt)
                    IsMovingDown = false;
            }

            else if (!IsMovingDown)
            {
                this.Location.Y -= MovementAmt;

                if (Location.Y == startingPos.Y - MovementAmt)
                    IsMovingDown = true;
            }
        }

    }
}
