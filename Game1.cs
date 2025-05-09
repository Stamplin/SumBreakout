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
        
        //activates on first press
        public static bool OnPress(KeyboardState KeyboardState, KeyboardState PreviousKeyboardStat, Keys _Key)
        {
            if (KeyboardState.IsKeyDown(Keys.Space) && PreviousKeyboardStat.IsKeyUp(Keys.Space))
            {
                return true;
            }
            return false;
        }

        
        //Paddle stuff

        Texture2D paddleTexture;
        Rectangle paddleRect;
        float ballSpeedX, ballSpeedY;
        //more paddle logic
        float ballHitX;
        float paddleThird;
        float edgeSteerX;
        float edgeBoostY;


        //Ball stuff

        Texture2D ballTexture;
        Rectangle ballRect;

        //Block stuff

        Texture2D brickTexture;
        Rectangle brickRect;
        List<Rectangle> bricks = new List<Rectangle>();
        //brick properties 
        int brickActualWidth = 80;
        int brickActualHeight = 20;
        int desiredHorizontalGap = 7;
        int desiredVerticalGap = 5;
        int numberOfColumns = 9;
        int numberOfRows = 5;
        int leftScreenMargin = 10;
        int topScreenMargin = 10;

        //Shared stuff

        MouseState mouseState;
        Random random = new Random();
        bool gameStart = false;
        static KeyboardState keyboardState, PreviousKeyboardState;
        //import soundeffects
        SoundEffect ballSound, paddleSound, brickSound;
        SoundEffectInstance ballSoundInstance, paddleSoundInstance, brickSoundInstance;


        //background
        List<Texture2D> backgroundFrames;
        int currentBackgroundFrameIndex;
        float backgroundAnimationTimer;
        const float TimePerBackgroundFrame = 1.0f / 24.0f;
        const int NumberOfBackgroundFrames = 81;



        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            //Paddle stuff
            paddleRect = new Rectangle(0, 0, 228 / 2, 46 / 2);

            //Ball stuff
            ballRect = new Rectangle(0, 0, 40, 40);

            //Block stuff
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

            //Shared stuff
            //keystate
            keyboardState = Keyboard.GetState();
            //set res
            _graphics.PreferredBackBufferWidth = 800;
            _graphics.PreferredBackBufferHeight = 600;
            _graphics.ApplyChanges();

            //background
            backgroundFrames = new List<Texture2D>();
            currentBackgroundFrameIndex = 0;
            backgroundAnimationTimer = 0f;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            //load textures
            ballTexture = Content.Load<Texture2D>("ball");
            paddleTexture = Content.Load<Texture2D>("paddle");
            brickTexture = Content.Load<Texture2D>("brick");

            //load audio
            ballSound = Content.Load<SoundEffect>("reflect");
            paddleSound = Content.Load<SoundEffect>("bounce");
            brickSound = Content.Load<SoundEffect>("break");

            //load background
            for (int i = 1; i <= NumberOfBackgroundFrames; i++)
            {
                backgroundFrames.Add(Content.Load<Texture2D>($"bg/bg({i})"));
            }

            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            //play background animation
            if (backgroundFrames.Count > 0)
            {
                backgroundAnimationTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (backgroundAnimationTimer >= TimePerBackgroundFrame)
                {
                    currentBackgroundFrameIndex = (currentBackgroundFrameIndex + 1) % backgroundFrames.Count;
                    backgroundAnimationTimer -= TimePerBackgroundFrame; // Subtract to maintain accuracy
                }
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

            //Paddle stuff

            //paddle logic paddle get mouse pos and follow
            paddleRect.X = mouseState.X - paddleRect.Width / 2;
            paddleRect.Y = 550;
            //stop at edge
            if (paddleRect.X < 0)
            {
                paddleRect.X = 0;
            }
            else if (paddleRect.X + paddleRect.Width > 800)
            {
                paddleRect.X = 800 - paddleRect.Width;
            }
            //if game start is flase
            if (!gameStart)
            {
                ballRect.X = paddleRect.X + paddleRect.Width / 2 - ballRect.Width / 2;
                ballRect.Y = paddleRect.Y - ballRect.Height;
            }

            //Ball stuff

            //speed limit
            float maxSpeed = 11.0f;
            if (Math.Abs(ballSpeedX) > maxSpeed) ballSpeedX = Math.Sign(ballSpeedX) * maxSpeed;
            if (Math.Abs(ballSpeedY) > maxSpeed) ballSpeedY = Math.Sign(ballSpeedY) * maxSpeed;

            //Block stuff




            //Shared stuff

            //mouse state
            mouseState = Mouse.GetState();
            //keystate
            PreviousKeyboardState = keyboardState;
            keyboardState = Keyboard.GetState();
            //sound
            ballSoundInstance = ballSound.CreateInstance();
            paddleSoundInstance = paddleSound.CreateInstance();
            brickSoundInstance = brickSound.CreateInstance();
            //window title is current speed
            Window.Title = ballSpeedX.ToString() + ", " + ballSpeedY.ToString();
            //reset if r is pressed
            if (Keyboard.GetState().IsKeyDown(Keys.R))
            {
                gameStart = false;
            }
            //if space is press game start
            if (Keyboard.GetState().IsKeyDown(Keys.Space))
            {
                gameStart = true;
            }






            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();

            if (backgroundFrames.Count > 0)
            {
                _spriteBatch.Draw(backgroundFrames[currentBackgroundFrameIndex],
                                  new Rectangle(0, 0, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight),
                                  Color.White);
            }

            //paddle
            _spriteBatch.Draw(paddleTexture, paddleRect, Color.White);

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