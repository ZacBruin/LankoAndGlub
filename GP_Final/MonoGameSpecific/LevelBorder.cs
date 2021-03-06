﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GP_Final
{
    /// <summary>
    /// Wall Guide: 0->Top 1->Right 2->Bottom 3->Left
    /// </summary>
    public sealed class LevelBorder : DrawableSprite
    {
        public Wall[] Walls;

        public Rectangle TopRect { get { return Walls[0].LocationRect; } }
        public Rectangle RightRect { get { return Walls[1].LocationRect; } }
        public Rectangle BottomRect { get { return Walls[2].LocationRect; } }
        public Rectangle LeftRect { get { return Walls[3].LocationRect; } }

        private Texture2D background;

        private const float BACKGROUND_SPRITE_SCALE = .563f;
        private const string BACKGROUND_SPRITE = "Sprites/Background";
        private const string INNER_BORDER_SPRITE = "Sprites/BorderInside";

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
            Walls[0].Initialize();
            Walls[0].Location = new Vector2(35, 0);
            Walls[0].SetTranformAndRect();

            Walls[1].Initialize();
            Walls[1].Location = new Vector2(TopRect.Right - Walls[1].SpriteTexture.Width, TopRect.Bottom);

            Walls[2].Initialize();
            Walls[2].Location = new Vector2(Walls[0].Location.X, Game.GraphicsDevice.Viewport.Bounds.Bottom - Walls[2].SpriteTexture.Height);

            Walls[3].Initialize();
            Walls[3].Location = new Vector2(Walls[0].Location.X, TopRect.Bottom);

            foreach (Wall w in Walls)
            {
                w.SetTranformAndRect();
            }

            spriteTexture = content.Load<Texture2D>(INNER_BORDER_SPRITE);
            background = content.Load<Texture2D>(BACKGROUND_SPRITE);

            Location = new Vector2(LeftRect.Right, TopRect.Bottom);

            base.LoadContent();
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();
            Vector2 backgroundPosition = new Vector2(LeftRect.Left, TopRect.Bottom);

            spriteBatch.Draw(background, backgroundPosition, null, Color.White, 0, Vector2.Zero, BACKGROUND_SPRITE_SCALE, SpriteEffects.None, 0);

            foreach (Wall w in Walls)
            {
                Rectangle destinationRect = new Rectangle(
                    (int)w.Location.X, 
                    (int)w.Location.Y,
                    (int)(w.SpriteTexture.Width * Scale), 
                    (int)(w.SpriteTexture.Height * Scale));

                spriteBatch.Draw(w.SpriteTexture, destinationRect, null, Color.White, 0f, w.Origin, SpriteEffects, 0);
            }

            spriteBatch.End();
            base.Draw(gameTime);
        }

        public override void Update(GameTime gameTime)
        {
            foreach (Wall w in Walls)
                w.Draw(gameTime);

            base.Update(gameTime);
        }

    }
}
