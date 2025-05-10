using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace SumBreakout
{
    internal class Paddle
    {
        //Paddle stuff
        Texture2D texture;
        //game bounds
        Rectangle bounds;
        private int screenWidth;
        public Rectangle Bounds => bounds;

        public Paddle(int screenWidth, int screenHeight)
        {
            this.screenWidth = screenWidth;
            //Paddle stuff
            bounds = new Rectangle(0, 0, 228 / 2, 46 / 2);
            bounds.Y = 550;
            //center paddle
            bounds.X = screenWidth / 2 - bounds.Width / 2;
        }

        //load assets
        public void LoadContent(ContentManager content, string assetName)
        {
            texture = content.Load<Texture2D>(assetName);
        }

        //Update
        public void Update(MouseState mouseState)
        {
            //paddle logic paddle get mouse pos and follow
            bounds.X = mouseState.X - bounds.Width / 2;
            //stop at edge
            if (bounds.X < 0)
            {
                bounds.X = 0;
            }
            else if (bounds.X + bounds.Width > screenWidth)
            {
                bounds.X = screenWidth - bounds.Width;
            }
        }
        //Draw
        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, bounds, Color.White);
        }

        //reset
        public void Reset()
        {
            bounds.Y = 550;
            bounds.X = screenWidth / 2 - bounds.Width / 2;
        }
    }
}
