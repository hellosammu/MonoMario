using System;
using Microsoft.Xna.Framework.Input;

namespace SMWEngine.Source.Engine
{
    public class CatInput
    {
        public static KeyboardState lastState;
        public static KeyboardState keyboardState;

        public static CatInputState GetState()
        {
            lastState = keyboardState;
            keyboardState = Keyboard.GetState();
            return new CatInputState(lastState,keyboardState);
        }
    }
}