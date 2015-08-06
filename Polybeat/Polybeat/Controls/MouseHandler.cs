using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Polybeat
{
    public static class MouseHandler
    {
        public static bool LeftMouseDown { get; private set; }
        public static bool RightMouseDown { get; private set; }

        public static bool LeftMouseClicked { get; private set; }
        public static bool RightMouseClicked { get; private set; }

        public static Vector2 MousePosition { get; private set; }

        private static bool leftMouseDownOnLastUpdate;
        private static bool rightMouseDownOnLastUpdate;

        public static void Update(GameTime time)
        {
            MouseState state = Mouse.GetState();

            if (state.LeftButton == ButtonState.Pressed)
            {
                LeftMouseDown = true;

                if (leftMouseDownOnLastUpdate == false)
                {
                    LeftMouseClicked = true;
                }
                else
                {
                    LeftMouseClicked = false;
                }

                leftMouseDownOnLastUpdate = true;
            }
            else
            {
                LeftMouseDown = false;
                LeftMouseClicked = false;
                leftMouseDownOnLastUpdate = false;
            }

            if (state.RightButton == ButtonState.Pressed)
            {
                RightMouseDown = true;

                if (rightMouseDownOnLastUpdate == false)
                {
                    RightMouseClicked = true;
                }
                else
                {
                    RightMouseClicked = false;
                }

                rightMouseDownOnLastUpdate = true;
            }
            else
            {
                RightMouseDown = false;
                RightMouseClicked = false;
                rightMouseDownOnLastUpdate = false;
            }

            MousePosition = state.Position.ToVector2();
        }
    }
}