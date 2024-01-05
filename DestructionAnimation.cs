using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace AIbuilding
{
    public class DestructionAnimation : Animation
    {
        public PointD position;
        float angle;
        float size = 0.75f;
        MapEngine map;

        internal DestructionAnimation(PointD position, MapEngine mymap) : base(new List<AnimationElement>())
        {
            animation_logic.Add(new AnimationElement(5));
            animation_logic[0].draw_logic = Draw1;
            animation_logic.Add(new AnimationElement(5));
            animation_logic[1].draw_logic = Draw2;
            animation_logic.Add(new AnimationElement(5));
            animation_logic[2].draw_logic = Draw3;
             animation_logic.Add(new AnimationElement(5));
             animation_logic[3].draw_logic = Draw4;
             animation_logic.Add(new AnimationElement(5));
             animation_logic[4].draw_logic = Draw5;
             animation_logic.Add(new AnimationElement(5));
             animation_logic[5].draw_logic = Draw6;
             animation_logic.Add(new AnimationElement(5));
             animation_logic[6].draw_logic = Draw7;
            map = mymap;
        }

        void Draw1() => Program.spriteBatch.Draw(Program.explosion_a[0], MapMath.LongLatToScreen(position, map.center, map.level), null, Color.White, angle, new Vector2(88, 76), MathF.Pow(2, map.level)*0.00002f, SpriteEffects.None, 1);

        void Draw2() => Program.spriteBatch.Draw(Program.explosion_a[1], MapMath.LongLatToScreen(position, map.center, map.level), null, Color.White, angle, new Vector2(142, 147), MathF.Pow(2, map.level) * 0.00002f, SpriteEffects.None, 1);

        void Draw3() => Program.spriteBatch.Draw(Program.explosion_a[2], MapMath.LongLatToScreen(position, map.center, map.level), null, Color.White, angle, new Vector2(165, 148), MathF.Pow(2, map.level) * 0.00002f, SpriteEffects.None, 1);

        void Draw4() => Program.spriteBatch.Draw(Program.explosion_a[3], MapMath.LongLatToScreen(position, map.center, map.level), null, Color.White, angle, new Vector2(188, 203), MathF.Pow(2, map.level) * 0.00002f, SpriteEffects.None, 1);

        void Draw5() => Program.spriteBatch.Draw(Program.explosion_a[4], MapMath.LongLatToScreen(position, map.center, map.level), null, Color.White, angle, new Vector2(203, 196), MathF.Pow(2, map.level) * 0.00002f, SpriteEffects.None, 1);

        void Draw6() => Program.spriteBatch.Draw(Program.explosion_a[5], MapMath.LongLatToScreen(position, map.center, map.level), null, Color.White, angle, new Vector2(156, 151), MathF.Pow(2, map.level) * 0.00002f, SpriteEffects.None, 1);

        void Draw7() => Program.spriteBatch.Draw(Program.explosion_a[6], MapMath.LongLatToScreen(position, map.center, map.level), null, Color.White, angle, new Vector2(179, 152), MathF.Pow(2, map.level) * 0.00002f, SpriteEffects.None, 1);

    }
}
