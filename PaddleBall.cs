using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace SumBreakout
{

    internal class PaddleBall
    {
        //Paddle stuff

        Texture2D _paddleTexture;
        Rectangle _paddleRect = new Rectangle(0, Constants.PaddlePosY, Constants.pRectW, Constants.pRectH);
        float PaddlePosX;
        float _paddleThird;
        float _edgeSteerX;
        float _edgeBoostY;


        //constructor
        public PaddleBall(
            Texture2D paddleTexture
            //Rectangle paddleRect
            //float ballSpeedX, 
            //float ballSpeedY, 
            //float ballHitX, 
            //float paddleThird, 
            //float edgeSteerX, 
            //float edgeBoostY
            )
        {
            _paddleTexture = paddleTexture;
            //_paddleRect = new Rectangle(0, Constants.PaddlePosY, Constants.pRectW, Constants.pRectH);
            //this.ballSpeedX = ballSpeedX;
            //this.ballSpeedY = ballSpeedY;
            //this.ballHitX = ballHitX;
            //this.paddleThird = paddleThird;
            //this.edgeSteerX = edgeSteerX;
            //this.edgeBoostY = edgeBoostY;
        }


        public void Update(MouseState mouseState)
        {
            //paddle logic paddle get mouse pos and follow
            _paddleRect.X = mouseState.X - _paddleRect.Width / 2;
            
        }


        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_paddleTexture, _paddleRect, Color.White);
        }




    }
}
