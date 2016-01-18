using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GP_Final
{
    public class SpriteSheetInfo
    {
        public int currentFrame, totalFrames, frameHeight, frameWidth, updatesPerFrame;

        public Rectangle sourceFrame;

        public SpriteSheetInfo(int totalFrames, int sheet_Width, int sheet_Height, int updates)
        {
            this.totalFrames = totalFrames;
            this.frameWidth = sheet_Width / totalFrames;
            this.frameHeight = sheet_Height;
            this.updatesPerFrame = updates;

            currentFrame = 0;

            UpdateSourceFrame();
        }

        public void UpdateSourceFrame()
        {
            this.sourceFrame = new Rectangle(currentFrame * frameWidth, 0, frameWidth, frameHeight);
        }
    }
}
