using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AIbuilding
{
    internal class TileDesc
    {
        public int level;
        public Microsoft.Xna.Framework.Point pos;

        public TileDesc(int level, Microsoft.Xna.Framework.Point pos)
        {
            this.level = level;
            this.pos = pos;
        }

        public TileDesc(int level, int x, int y)
        {
            this.level = level;
            this.pos = new Microsoft.Xna.Framework.Point(x, y);
        }

        public override int GetHashCode()
        {
            int hash = 23;
            hash = hash * 31 + level;
            hash = hash * 31 + pos.X;
            hash = hash * 31 + pos.Y;
            return hash;
        }

        public override bool Equals(object obj)
        {
            return level == ((TileDesc)obj).level && pos == ((TileDesc)obj).pos;
        }
    }

    public abstract class AbstractTile
    {
        public int level { get; private set; }
        public Microsoft.Xna.Framework.Point pos;
        public Texture2D texture { get; private set; }
        public Vector2 realpos { get; private set; }
        public bool loaded { get; private set; }
        internal Thread load_thread;
        internal string tilefile, tileurl;

        internal AbstractTile(int level, Microsoft.Xna.Framework.Point pos, Texture2D texture)
        {
            this.level = level;
            this.pos = pos;
            this.texture = texture;
            realpos = MapMath.PointToCoordinates(pos, level);
            loaded = true;
        }

        internal void LoadTile(string folder)
        {
            texture = GetTileTexture(folder);
            if (texture != null) loaded = true;
        }

        internal abstract Texture2D GetTileTexture(string folder);

        internal AbstractTile(int level, Microsoft.Xna.Framework.Point pos, string folder)
        {
            loaded = false;
            this.level = level;
            this.pos = pos;
            realpos = MapMath.PointToCoordinates(pos, level);
            tilefile = TileFile();
            tileurl = TileURL();
            load_thread = new Thread(() => LoadTile(folder));
            load_thread.Start();
        }

        public Pair<Vector2, float> ScreenPosition(int cur_level, Vector2 center)
        {
            float scale = MathF.Pow(2, 2 + cur_level - level);
            return new Pair<Vector2, float>(MapMath.CoordiantesToScreen(realpos, center, cur_level), scale);
        }

        public abstract string TileFile();

        public abstract string TileURL();

        public virtual void DrawTile(int cur_level, SpriteBatch spriteBatch, Vector2 center)
        {
            var scr_pos = ScreenPosition(cur_level, center);
            spriteBatch.Draw(texture, scr_pos.First, null, Microsoft.Xna.Framework.Color.White, 0f, Vector2.Zero, scr_pos.Second, SpriteEffects.None, 1);
        }
    }
}
