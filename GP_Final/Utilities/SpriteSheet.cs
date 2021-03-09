using Microsoft.Xna.Framework;

namespace GP_Final
{
    public class SpriteSheetInfo
    {
        public int CurrentFrame;
        public Rectangle SourceFrame;

        public int UpdatesPerFrame { get; }
        public int TotalFrames { get; }

        private int frameHeight, frameWidth;

        public SpriteSheetInfo(int totalFrames, int sheetWidth, int sheetHeight, int updates)
        {
            TotalFrames = totalFrames;
            frameWidth = sheetWidth / totalFrames;
            frameHeight = sheetHeight;
            UpdatesPerFrame = updates;

            CurrentFrame = 0;

            UpdateSourceFrame();
        }

        public void UpdateSourceFrame()
        {
            SourceFrame = new Rectangle(CurrentFrame * frameWidth, 0, frameWidth, frameHeight);
        }
    }
}
