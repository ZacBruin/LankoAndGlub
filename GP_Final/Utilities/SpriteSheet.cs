using Microsoft.Xna.Framework;

namespace GP_Final
{
    public class SpriteSheetInfo
    {
        public int CurrentFrame, 
                   TotalFrames, 
                   FrameHeight, 
                   FrameWidth, 
                   UpdatesPerFrame;

        public Rectangle SourceFrame;

        public SpriteSheetInfo(int totalFrames, int sheetWidth, int sheetHeight, int updates)
        {
            TotalFrames = totalFrames;
            FrameWidth = sheetWidth / totalFrames;
            FrameHeight = sheetHeight;
            UpdatesPerFrame = updates;

            CurrentFrame = 0;

            UpdateSourceFrame();
        }

        public void UpdateSourceFrame()
        {
            SourceFrame = new Rectangle(CurrentFrame * FrameWidth, 0, FrameWidth, FrameHeight);
        }
    }
}
