using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GP_Final
{
    class InputController
    {
        InputHandler input;
        public Vector2 MouseDirection
        {
            get;
            private set;
        }

        public MouseState Mouse;
        public KeyboardState Keys;

        public InputController(Game game)
        {
            input = (InputHandler)game.Services.GetService<IInputHandler>();

            if (input == null)
                throw new Exception("Controller has no input. Add Input Handler as a service first");
        }

        public void Update()
        {
            Mouse = Microsoft.Xna.Framework.Input.Mouse.GetState();
            Keys = Keyboard.GetState();          
            MouseDirection = new Vector2(Mouse.Position.X, Mouse.Position.Y);
        }
    }
}
