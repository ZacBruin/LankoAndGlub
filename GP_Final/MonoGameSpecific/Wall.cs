using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GP_Final
{
    public class Wall : DrawableSprite
    {
        public enum WallType { Top, Bottom, Left, Right };
        private WallType currentWallType;

        private const string TOP_BORDER_SPRITE = "Sprites/BorderTop";
        private const string BOTTOM_BORDER_SPRITE = "Sprites/BorderBottom";
        private const string LEFT_BORDER_SPRITE = "Sprites/BorderLeft";
        private const string RIGHT_BORDER_SPRITE = "Sprites/BorderRight";

        public Wall(Game game, WallType type) : base(game)
        {
            currentWallType = type;
        }

        protected override void LoadContent()
        {
            switch(currentWallType)
            {
                case WallType.Top:
                    spriteTexture = content.Load<Texture2D>(TOP_BORDER_SPRITE);
                    break;

                case WallType.Bottom: 
                    spriteTexture = content.Load<Texture2D>(BOTTOM_BORDER_SPRITE);
                    break;

                case WallType.Left:
                    spriteTexture = content.Load<Texture2D>(LEFT_BORDER_SPRITE);
                    break;

                case WallType.Right:
                    spriteTexture = content.Load<Texture2D>(RIGHT_BORDER_SPRITE);
                    break;

            }

            SetTranformAndRect();
            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }
    }
}
