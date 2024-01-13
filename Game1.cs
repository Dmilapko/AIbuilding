using AIbuilding;
using FormElementsLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonogameLabel;
using MonoHelper;
using System.Collections.Generic;
using System.IO;
using System.Timers;
using System.Xml.Linq;

namespace AIbuilding
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        Session session;
        int fpscnt = 0;
        List<FormElement> elements = new List<FormElement>();
        public Label fpslabel, pauselabel;
        KeySwitch keyf1 = new KeySwitch(Keys.F1);

        bool paused = false, press_p = false;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            uint a = Color.Black.PackedValue;
            // TODO: Add your initialization logic here
            graphics.PreferredBackBufferWidth = 1920;
            graphics.PreferredBackBufferHeight = 1080;
            Window.Position = new Point(0, 0);
            //graphics.ToggleFullScreen();
            graphics.ApplyChanges();
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Program.my_device = GraphicsDevice;
            Program.spriteBatch = spriteBatch;
            base.Initialize();
        }
        private void Fpstimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            fpslabel.text = "FPS " + fpscnt.ToString();
            fpscnt = 0;
        }


        protected override void LoadContent()
        {
            Program.font20 = Content.Load<SpriteFont>(@"font20");
            Program.font10 = Content.Load<SpriteFont>(@"font10");
            Program.font15 = Content.Load<SpriteFont>(@"font15");
            for (int i = 1; i <= 7; i++)
            {
                Program.explosion_a.Add(Texture2D.FromStream(Program.my_device, new FileStream("MyContent\\explosion_a" + i.ToString() + ".png", FileMode.Open)));
            }
            fpslabel = new Label(1820, 20, -1, -1, "FPS", Program.font20, 20, 20);
            fpslabel.textcolor = Color.Red;
            elements.Add(fpslabel);
            pauselabel = new Label(20, 20, -1, -1, "PAUSE", Program.font20, 20, 20);
            pauselabel.textcolor = Color.Red;
            elements.Add(pauselabel);
            session = new Session();
            Timer fpstimer = new Timer();
            fpstimer.Elapsed += Fpstimer_Elapsed;
            fpstimer.Interval = 1000;
            fpstimer.Enabled = true;
            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            keyf1.Run(Keyboard.GetState());
            paused = keyf1.state;

            if (!paused) session.Run(Mouse.GetState(), Keyboard.GetState());
            // TODO: Add your update logic here

            base.Update(gameTime);
            fpscnt++;

        }

        protected override void Draw(GameTime gameTime)
        {
            if (paused) pauselabel.text = "PAUSE [F1]";
            else pauselabel.text = "RUNNING [F1]";
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
            session.Draw();
            foreach (var element in elements)
            {
                element.Draw(spriteBatch);
            }
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}