using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
namespace SumBreakout
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        //textures
        Texture2D ballTexture, brickTexture, paddleTexture;

        //rectangles
        Rectangle ballRect, paddleRect, brickRect;

        //floats
        float ballSpeedX, ballSpeedY;

        //mousestate
        MouseState mouseState;

        //bricks stuff
        List<Rectangle> bricks = new List<Rectangle>();

        //bool game start
        bool gameStart = false;

        //keyboard
        static KeyboardState keyboardState, PreviousKeyboardState;

        //activates on first press
        public static bool OnPress(KeyboardState KeyboardState, KeyboardState PreviousKeyboardStat, Keys _Key)
        {
            if (KeyboardState.IsKeyDown(Keys.Space) && PreviousKeyboardStat.IsKeyUp(Keys.Space))
            {
                return true;
            }
            return false;
        }

        //random
        Random random = new Random();


        //brick properties 
        int brickActualWidth = 80;
        int brickActualHeight = 20;
        int desiredHorizontalGap = 7;
        int desiredVerticalGap = 5;
        int numberOfColumns = 9;
        int numberOfRows = 5;
        int leftScreenMargin = 10;
        int topScreenMargin = 10;

        //import soundeffects
        SoundEffect ballSound, paddleSound, brickSound;
        SoundEffectInstance ballSoundInstance, paddleSoundInstance, brickSoundInstance;

        //more paddle logic
        float ballHitX;
        float paddleThird;
        float edgeSteerX;
        float edgeBoostY;

        //object
        PaddleBall paddleball;


        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            //set res
            _graphics.PreferredBackBufferWidth = 800;
            _graphics.PreferredBackBufferHeight = 600;
            _graphics.ApplyChanges();

            //rectangles
            ballRect = new Rectangle(0, 0, 40, 40);

            paddleRect = new Rectangle(0, 0, 228 / 2, 46 / 2);

            brickRect = new Rectangle(0, 0, 50, 20);

            //spawn bricks in grid with gap between
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)//rows
                {
                    int x = i * (80 + 11);//width
                    int y = j * (20 + 5); //height
                    bricks.Add(new Rectangle(x, y, 75, 20));
                }
            }

            //keystate
            keyboardState = Keyboard.GetState();

            //paddleball
            paddleball = new PaddleBall(paddleTexture);





            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            //load textures
            ballTexture = Content.Load<Texture2D>("ball");
            brickTexture = Content.Load<Texture2D>("brick");

            //load audio
            ballSound = Content.Load<SoundEffect>("bounce");
            paddleSound = Content.Load<SoundEffect>("reflect");
            brickSound = Content.Load<SoundEffect>("break");

            paddleball = new PaddleBall(Content.Load<Texture2D>("paddle"));
            


            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();




            //mouse state
            mouseState = Mouse.GetState();

            //keystate
            PreviousKeyboardState = keyboardState;
            keyboardState = Keyboard.GetState();


            paddleball.Update(mouseState);

            //stop at edge
            if (paddleRect.X < 0)
            {
                paddleRect.X = 0;
            }
            else if (paddleRect.X + paddleRect.Width > 800)
            {
                paddleRect.X = 800 - paddleRect.Width;
            }

            //sound
            ballSoundInstance = ballSound.CreateInstance();
            paddleSoundInstance = paddleSound.CreateInstance();
            brickSoundInstance = brickSound.CreateInstance();





            //if game start is flase
            if (!gameStart)
            {
                ballRect.X = paddleRect.X + paddleRect.Width / 2 - ballRect.Width / 2;
                ballRect.Y = paddleRect.Y - ballRect.Height;
            }


            //if space is press game start
            if (Keyboard.GetState().IsKeyDown(Keys.Space))
            {
                gameStart = true;
            }



            if (gameStart)
            {
                //space launches ball
                if (OnPress(keyboardState, PreviousKeyboardState, Keys.Space))
                {
                    ballRect.X = paddleRect.X + paddleRect.Width / 2 - ballRect.Width / 2;
                    ballRect.Y = paddleRect.Y - ballRect.Height;

                    ballSpeedX = ((float)random.NextDouble() * 3.0f + 2.0f) * (random.Next(0, 2) == 0 ? -1 : 1);
                    ballSpeedY = -5;

                }

                //ball movement
                ballRect.X += (int)(float)ballSpeedX;
                ballRect.Y += (int)(float)ballSpeedY;

                //ball bounce off edge
                if (ballRect.X < 0)
                {
                    ballSpeedX = -ballSpeedX;
                    //fix
                    ballRect.X = 0;
                    //bounce sound
                    ballSoundInstance.Play();

                }
                else if (ballRect.X + ballRect.Width > 800)
                {
                    ballSpeedX = -ballSpeedX;
                    //fix
                    ballRect.X = 800 - ballRect.Width;
                    //bounce sound
                    ballSoundInstance.Play();
                }

                if (ballRect.Y < 0)
                {
                    ballSpeedY = Math.Abs(ballSpeedY);
                    //fix
                    ballRect.Y = 0;
                    //bounce sound
                    ballSoundInstance.Play();
                }

                //bounce off paddle (W edges now)
                if (ballRect.Intersects(paddleRect))
                {
                    ballSpeedY = -ballSpeedY;
                    ballRect.Y = paddleRect.Y - ballRect.Height;

                    ballHitX = ballRect.X + ballRect.Width / 2.0f;
                    paddleThird = paddleRect.Width / 3.0f;
                    edgeSteerX = 7.0f;
                    edgeBoostY = 1.15f;

                    if (ballHitX < paddleRect.X + paddleThird)
                    {
                        ballSpeedX = -edgeSteerX;
                        ballSpeedY *= edgeBoostY;
                    }
                    else if (ballHitX > paddleRect.X + 2 * paddleThird)
                    {
                        ballSpeedX = edgeSteerX;
                        ballSpeedY *= edgeBoostY;
                    }


                    paddleSoundInstance.Play();
                }

                //if ball hits brick, destroy it
                for (int i = 0; i < bricks.Count; i++)
                {
                    if (ballRect.Intersects(bricks[i]))
                    {
                        ballSpeedY = -ballSpeedY;
                        bricks.RemoveAt(i);

                        ballSpeedX *= 1.02f;
                        ballSpeedY *= 1.02f;

                        //break sound
                        brickSoundInstance.Play();
                    }
                }
            }

            //window title is current speed
            Window.Title = ballSpeedX.ToString() + ", " + ballSpeedY.ToString();

            //speed limit
            float maxSpeed = 11.0f;
            if (Math.Abs(ballSpeedX) > maxSpeed) ballSpeedX = Math.Sign(ballSpeedX) * maxSpeed;
            if (Math.Abs(ballSpeedY) > maxSpeed) ballSpeedY = Math.Sign(ballSpeedY) * maxSpeed;

            //reset if r is pressed
            if (Keyboard.GetState().IsKeyDown(Keys.R))
            {
                gameStart = false;
            }






















            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();

            //paddle
            paddleball.Draw(_spriteBatch);

            //ball
            _spriteBatch.Draw(ballTexture, ballRect, Color.White);

            //bricks
            foreach (Rectangle brick in bricks)
            {
                _spriteBatch.Draw(brickTexture, brick, Color.White);
            }




            _spriteBatch.End();

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}