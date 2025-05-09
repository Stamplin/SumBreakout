using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SumBreakout
{
    public class Ball
    {
        //Ball stuff
        Texture2D texture;
        Rectangle bounds;
        float speedX, speedY;

        //screen area for bounce
        private int screenWidth;
        private int screenHeight;

        //speed limits
        const float MaxSpeed = 11.0f;

        public Rectangle Bounds => bounds;
        public bool IsMoving => speedX != 0 || speedY != 0;

        public Ball(int screenWidth, int screenHeight)
        {
            this.screenWidth = screenWidth;
            this.screenHeight = screenHeight;
            //Ball stuff
            bounds = new Rectangle(0, 0, 40, 40); //ball size
        }

        public void LoadContent(ContentManager content, string assetName)
        {
            texture = content.Load<Texture2D>(assetName);
        }

        public void StickToPaddle(Rectangle paddleRect)
        {
            bounds.X = paddleRect.X + paddleRect.Width / 2 - bounds.Width / 2;
            bounds.Y = paddleRect.Y - bounds.Height;
            speedX = 0;
            speedY = 0;
        }

        public void Launch(Rectangle paddleRect, Random random)
        {
            //stay on paddle till launched
            bounds.X = paddleRect.X + paddleRect.Width / 2 - bounds.Width / 2;
            bounds.Y = paddleRect.Y - bounds.Height;

            speedX = ((float)random.NextDouble() * 3.0f + 2.0f) * (random.Next(0, 2) == 0 ? -1 : 1);
            speedY = -5; //launch speed
        }

        public void UpdateMovement()
        {
            //ball movement
            bounds.X += (int)speedX;
            bounds.Y += (int)speedY;
        }

        public void HandleWallCollisions(SoundEffectInstance wallSoundInstance)
        {
            //ball bounce from edge
            if (bounds.X < 0)
            {
                speedX = -speedX;
                //fix
                bounds.X = 0;
                //bounce sound
                wallSoundInstance.Play();
            }
            else if (bounds.X + bounds.Width > screenWidth)
            {
                speedX = -speedX;
                //fix
                bounds.X = screenWidth - bounds.Width;
                //bounce sound
                wallSoundInstance.Play();
            }

            if (bounds.Y < 0)
            {
                speedY = Math.Abs(speedY);
                bounds.Y = 0;
                //bounce sound
                wallSoundInstance.Play();
            }


        }
        public bool CheckPaddleCollision(Rectangle paddleRect)
        {
            return bounds.Intersects(paddleRect);
        }

        public void HandlePaddleHit(Rectangle paddleRect, SoundEffectInstance paddleSoundInstance)
        {
            float ballHitX_local;
            float paddleThird_local;
            float edgeSteerX_local;
            float edgeBoostY_local;

            speedY = -speedY;
            bounds.Y = paddleRect.Y - bounds.Height; //sticking fix
            ballHitX_local = bounds.X + bounds.Width / 2.0f;
            paddleThird_local = paddleRect.Width / 3.0f;
            edgeSteerX_local = 7.0f; 
            edgeBoostY_local = 1.15f; 

            //edge paddle bounce

            if (ballHitX_local < paddleRect.X + paddleThird_local)
            {
                speedX = -edgeSteerX_local;
                speedY *= edgeBoostY_local;
            }
            else if (ballHitX_local > paddleRect.X + 2 * paddleThird_local)
            {
                speedX = edgeSteerX_local;
                speedY *= edgeBoostY_local;
            }

            paddleSoundInstance.Play();
        }

        public int CheckBrickCollision(List<Brick> bricks)
        {
            for (int i = 0; i < bricks.Count; i++)
            {
                if (bricks[i].IsActive && bounds.Intersects(bricks[i].Bounds))
                {
                    return i;
                }
            }
            return -1;
        }

        //speed up on hit
        public void HandleBrickHit(SoundEffectInstance brickSoundInstance)
        {
            speedY = -speedY;

            speedX *= 1.02f;
            speedY *= 1.02f;

            brickSoundInstance.Play();
        }

        public void ApplySpeedLimit()
        {
            //speed limit
            if (Math.Abs(speedX) > MaxSpeed) speedX = Math.Sign(speedX) * MaxSpeed;
            if (Math.Abs(speedY) > MaxSpeed) speedY = Math.Sign(speedY) * MaxSpeed;
        }

        public void Reset(Rectangle paddleRect)
        {
            StickToPaddle(paddleRect);
        }
        //Draw
        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, bounds, Color.White);
        }

        //speed testing on window title
        public string GetSpeedString()
        {
            return $"{speedX:F2}, {speedY:F2}";
        }


    }
}
