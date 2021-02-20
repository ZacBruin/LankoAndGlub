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
            frameWidth = sheet_Width / totalFrames;
            frameHeight = sheet_Height;
            updatesPerFrame = updates;

            currentFrame = 0;

            UpdateSourceFrame();
        }

        public void UpdateSourceFrame()
        {
            sourceFrame = new Rectangle(currentFrame * frameWidth, 0, frameWidth, frameHeight);
        }
    }
}
