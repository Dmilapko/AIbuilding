using Microsoft.Xna.Framework.Graphics;
using System;

namespace AIlanding
{
    public static class Program
    {
        public static SpriteFont font20;
        public static SpriteFont font10;
        public static SpriteFont font15;

        [STAThread]
        static void Main()
        {
            using var game = new AIbuilding.Game1();
            game.Run();
        }
    }
}

