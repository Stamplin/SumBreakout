using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SumBreakout
{
    public class Brick
    {
        public Rectangle Bounds { get; private set; }
        public bool IsActive { get; private set; }

        public Brick(Rectangle bounds)
        {
            Bounds = bounds;
            IsActive = true;
        }

        public void Hit()
        {
            IsActive = false;
        }

        //draw
        public void Draw(SpriteBatch spriteBatch, Texture2D texture)
        {
            if (IsActive)
            {
                spriteBatch.Draw(texture, Bounds, Color.White);
            }
        }
    }
}
