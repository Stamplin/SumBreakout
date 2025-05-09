using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

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

        //more paddle logic
        float ballHitX;
        float paddleThird;
        float edgeSteerX;
        float edgeBoostY;


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

            //objects
            paddle = new Paddle(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
            ball = new Ball(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);

            //setup
            InitializeBricks();
            ball.Reset(paddle.Bounds);
            keyboardState = Keyboard.GetState();
            PreviousKeyboardState = keyboardState;

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
            //sound instance
            ballSoundInstance = ballSound.CreateInstance();
            paddleSoundInstance = paddleSound.CreateInstance();
            brickSoundInstance = brickSound.CreateInstance();

            // TODO: use this.Content to load your game content here
        }

        protected override List<Rectangle> GetBricks()
        {
            return bricks;
        }

        protected override void Update(GameTime gameTime, List<Rectangle> bricks)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();


            if (gameStart)
            {
                backgroundAnimationTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (backgroundAnimationTimer >= TimePerBackgroundFrame)
                {
                    ballRect.X = paddleRect.X + paddleRect.Width / 2 - ballRect.Width / 2;
                    ballRect.Y = paddleRect.Y - ballRect.Height;

                    ballSpeedX = ((float)random.NextDouble() * 3.0f + 2.0f) * (random.Next(0, 2) == 0 ? -1 : 1);
                    ballSpeedY = -5;
                    
                }
            }

            //paddle
            paddle.Update(mouseState);


            if (!gameStart)
            {
                ball.StickToPaddle(paddle.Bounds);

                if (OnPress(keyboardState, previousKeyboardState, Keys.Space))
                {
                    gameStart = true;
                    ball.Launch(paddle.Bounds, random);
                }
            }
            else
            {
                if (!ball.IsMoving)
                {
                    ball.StickToPaddle(paddle.Bounds);

                    if (OnPress(keyboardState, previousKeyboardState, Keys.Space))
                    {
                        ball.Launch(paddle.Bounds, random);
                    }
                }
                else //ball is moving
                {
                    ballSpeedY = -ballSpeedY; 
                    ballRect.Y = paddleRect.Y - ballRect.Height; 

                    ball.HandleWallCollisions(ballSoundInstance);

                    if (ballHitX < paddleRect.X + paddleThird) 
                    {
                        ballSpeedX = -edgeSteerX;
                        ballSpeedY *= edgeBoostY;
                    }
                    else if (ballHitX > paddleRect.X + 2 * paddleThird) 
                    {

                        ball.HandlePaddleHit(paddle.Bounds, paddleSoundInstance);
                    }
                    

                    paddleSoundInstance.Play();
                }

                    int hitBrickIndex = ball.CheckBrickCollision(bricks);
                    if (hitBrickIndex != -1)
                    {
                        bricks[hitBrickIndex].Hit(); //mark the Brick object as hit

                        ball.HandleBrickHit(brickSoundInstance);
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

            Window.Title = ball.GetSpeedString();


            base.Update(gameTime,

            base.GetBricks());
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
        private void InitializeBricks()
        {
            bricks.Clear();
            int effectiveBrickWidth = brickActualWidth + desiredHorizontalGap;
            int effectiveBrickHeight = brickActualHeight + desiredVerticalGap;

            for (int col = 0; col < numberOfColumns; col++)
            {
                for (int row = 0; row < numberOfRows; row++)
                {
                    int x = leftScreenMargin + col * effectiveBrickWidth;
                    int y = topScreenMargin + row * effectiveBrickHeight;
                    Rectangle brickBounds = new Rectangle(x, y, brickActualWidth, brickActualHeight);
                    bricks.Add(new Brick(brickBounds));
                }
            }
        } 
    }
}