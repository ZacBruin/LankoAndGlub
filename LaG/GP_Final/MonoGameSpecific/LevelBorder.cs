using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GP_Final
{
    /// <summary>
    /// Wall Guide: 0->Top  1->Right 2->Bottom  3->Left
    /// </summary>
    public sealed class LevelBorder : DrawableSprite
    {
        public Wall[] Walls;
        Texture2D background;

        public LevelBorder(Game game) : base(game)
        {
            Walls = new Wall[4];

            Walls[0] = new Wall(game, Wall.WallType.Top);
            Walls[1] = new Wall(game, Wall.WallType.Right);
            Walls[2] = new Wall(game, Wall.WallType.Bottom);
            Walls[3] = new Wall(game, Wall.WallType.Left);
        }

        protected override void LoadContent()
        {
            Walls[0].Initialize(); Walls[1].Initialize(); Walls[2].Initialize(); Walls[3].Initialize();

            Walls[0].Location = new Vector2(35, 0);
            Walls[0].SetTranformAndRect();

            Walls[1].Location = new Vector2(Walls[0].LocationRect.Right - Walls[1].SpriteTexture.Width,
                Walls[0].LocationRect.Bottom);

            Walls[2].Location = new Vector2(Walls[0].Location.X,
                this.Game.GraphicsDevice.Viewport.Bounds.Bottom - Walls[2].SpriteTexture.Height);

            Walls[3].Location = new Vector2(Walls[0].Location.X,
                Walls[0].LocationRect.Bottom);

            foreach(Wall w in this.Walls)
            {
                w.SetTranformAndRect();
            }

            this.spriteTexture = content.Load<Texture2D>("Sprites/BorderInside");
            this.background = content.Load<Texture2D>("Sprites/Background");

            this.Location = new Vector2(this.Walls[3].LocationRect.Right, this.Walls[0].LocationRect.Bottom);

            base.LoadContent();
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();

            spriteBatch.Draw(background, new Vector2(this.Walls[3].LocationRect.Left,
                this.Walls[0].LocationRect.Bottom), null, Color.White, 0, new Vector2(0, 0), .563f,
                SpriteEffects.None, 0);

            foreach (Wall w in Walls)
            {
                spriteBatch.Draw(w.SpriteTexture, new Rectangle((int)w.Location.X, (int)w.Location.Y,
                  (int)(w.SpriteTexture.Width * this.Scale), (int)(w.SpriteTexture.Height * this.Scale)),
                   null, Color.White, 0f, w.Origin, SpriteEffects, 0);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }

        public override void Update(GameTime gameTime)
        {
            foreach (Wall w in Walls)
            {
                w.Draw(gameTime);
            }

            base.Update(gameTime);
        }

    }
}
