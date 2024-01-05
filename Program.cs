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
using MathNet.Numerics;


namespace AIbuilding
{
    public static class Program
    {
        public static SpriteFont font20;
        public static SpriteFont font10;
        public static SpriteFont font15;
        public static GraphicsDevice my_device;
        public static SpriteBatch spriteBatch;
        public static int overpasscnt = 0;
        /// <summary>
        /// session_name, routel_name, routes_name
        /// </summary>
        static public Dictionary<string, string> setupProp = new Dictionary<string, string>();
        public static string setuppath = "setup";
        public static bool save_graphics = false;
        public static bool only_INS = false;
        public static List<Texture2D> explosion_a = new List<Texture2D>();

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
            List<double> rrpp = new List<double>();
            for (int i = 0; i < 1000; i++)
            {
                double res = 0;
                for (int j = 0; j < 11; j++)
                {
                    res += MHeleper.RandomDouble();
                }
                rrpp.Add(res / 11);
            }
            rrpp.Sort();
            if ((File.Exists(setuppath)) && (File.ReadAllText(setuppath) != ""))
            {
                GetSetup();
            }
            else
            {
                setupProp.Add("session_name", "default_ses");
                setupProp.Add("routel_name", "default_route");
                setupProp.Add("routes_name", "default_route");
                setupProp.Add("dronel_name", "default_drone");
                setupProp.Add("drones_name", "default_drone");
                ChangeSetup();
            }
            if (!Directory.Exists(setupProp["session_name"])) Directory.CreateDirectory(setupProp["session_name"]);
            using (var game = new Game1())
                 game.Run();
        }
    }
}

