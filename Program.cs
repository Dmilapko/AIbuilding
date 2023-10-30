using AIbuilding;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Threading;

namespace AIlanding
{
    public static class Program
    {
        public static SpriteFont font20;
        public static SpriteFont font10;
        public static SpriteFont font15;
        public static GraphicsDevice my_device;
        public static SpriteBatch spriteBatch;
        public static int overpasscnt = 0;


        [STAThread]
        static void Main()
        {
            using var game = new AIbuilding.Game1();
            game.Run();
        }
    }
}

