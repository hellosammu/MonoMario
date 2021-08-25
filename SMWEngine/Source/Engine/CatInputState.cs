using Microsoft.Xna.Framework.Input;

namespace SMWEngine.Source.Engine
{
    public struct CatInputState
    {
        private KeyboardState lastState;
        private KeyboardState keyboardState;

        public CatInputState(KeyboardState lastState, KeyboardState keyboardState)
        {
            this.lastState = lastState;
            this.keyboardState = keyboardState;
        }

        public bool Pressed(Keys key) => keyboardState.IsKeyDown(key);
        public bool Released(Keys key) => keyboardState.IsKeyUp(key);
        public bool JustPressed(Keys key) => lastState.IsKeyUp(key) && keyboardState.IsKeyDown(key);
        public bool JustReleased(Keys key) => lastState.IsKeyDown(key) && keyboardState.IsKeyUp(key);
        public bool AnyPressed(Keys[] keys)
        {
            foreach (Keys key in keys)
            {
                if (keyboardState.IsKeyDown(key))
                    return true;
            }
            return false;
        }
        public bool AnyJustPressed(Keys[] keys)
        {
            foreach (Keys key in keys)
            {
                if (JustPressed(key))
                    return true;
            }
            return false;
        }
        public bool AnyJustReleased(Keys[] keys)
        {
            foreach (Keys key in keys)
            {
                if (JustReleased(key))
                    return true;
            }
            return false;
        }
    }
}