using AIbuilding;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.IO;
using Microsoft.Xna.Framework;
using System.IO.Compression;
using MonoHelper;

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
        public static bool save_graphics = false, debug = true, iterate_b = false, center_drone = false;
        /// <summary>
        /// session_name, routel_name, routes_name
        /// </summary>
        static public Dictionary<string, string> setupProp = new Dictionary<string, string>();
        public static string setuppath = "setup";

        static public void ChangeSetup()
        {
            string str = JsonSerializer.Serialize(setupProp);
            File.WriteAllText(setuppath, str);
        }

        static public void GetSetup()
        {
            string str = File.ReadAllText(setuppath);
            setupProp = JsonSerializer.Deserialize<Dictionary<string, string>>(str);
        }



        [STAThread]
        static void Main()
        {
            if ((File.Exists(setuppath)) && (File.ReadAllText(setuppath) != ""))
            {
                GetSetup();
            }
            else
            {
                setupProp.Add("session_name", "default_ses");
                setupProp.Add("routel_name", "default_route");
                setupProp.Add("routes_name", "default_route");
                ChangeSetup();
            }
            if (!Directory.Exists(setupProp["session_name"])) Directory.CreateDirectory(setupProp["session_name"]);
            using (var game = new Game1())
                 game.Run();
        }
    }
}

