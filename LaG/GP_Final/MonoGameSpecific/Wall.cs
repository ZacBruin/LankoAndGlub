﻿using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GP_Final
{
    public class Wall : DrawableSprite
    {
        public enum WallType { Top, Bottom, Left, Right };
        WallType currentWallType;

        public Wall(Game game, WallType type) : base(game)
        {
            this.Location = Location;
            this.currentWallType = type;
        }

        protected override void LoadContent()
        {
            switch(this.currentWallType)
            {
                case WallType.Top:
                    this.spriteTexture = content.Load<Texture2D>("Sprites/BorderTop");
                    break;

                case WallType.Bottom: 
                    this.spriteTexture = content.Load<Texture2D>("Sprites/BorderBottom");
                    break;

                case WallType.Left:
                    this.spriteTexture = content.Load<Texture2D>("Sprites/BorderLeft");
                    break;

                case WallType.Right:
                    this.spriteTexture = content.Load<Texture2D>("Sprites/BorderRight");
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
