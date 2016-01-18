using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GP_Final
{
    class InputController
    {
        InputHandler input;
        public Vector2 mouseDirection
        { get; private set; }

        public MouseState mouse;
        public KeyboardState keys;

        public InputController(Game game)
        {
            input = (InputHandler)game.Services.GetService<IInputHandler>();

            if (input == null)
            {
                throw new Exception("Controller has no input. Add Input Handler as a service first");
            }
        }

        public void Update()
        {            
            mouse = Mouse.GetState();
            keys = Keyboard.GetState();          

            mouseDirection = new Vector2(mouse.Position.X, mouse.Position.Y);
        }
    }
}
