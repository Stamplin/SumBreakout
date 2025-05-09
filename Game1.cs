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

        //object
        Paddle paddle;
        Ball ball;

        //block stuff
        Texture2D brickTexture;
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
        private KeyboardState previousKeyboardState;
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
            paddle.LoadContent(Content, "paddle");
            ball.LoadContent(Content, "ball");
            brickTexture = Content.Load<Texture2D>("brick");

            //load audio
            ballSound = Content.Load<SoundEffect>("reflect");
            paddleSound = Content.Load<SoundEffect>("bounce");
            brickSound = Content.Load<SoundEffect>("break");
            //sound instance
            ballSoundInstance = ballSound.CreateInstance();
            paddleSoundInstance = paddleSound.CreateInstance();
            brickSoundInstance = brickSound.CreateInstance();

            //load background
            for (int i = 1; i <= NumberOfBackgroundFrames; i++)
            {
                backgroundFrames.Add(Content.Load<Texture2D>($"bg/bg({i})"));
            }

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

            //setup 
            previousKeyboardState = keyboardState;
            keyboardState = Keyboard.GetState();
            mouseState = Mouse.GetState();

            //background
            if (backgroundFrames.Count > 0)
            {
                backgroundAnimationTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (backgroundAnimationTimer >= TimePerBackgroundFrame)
                {
                    currentBackgroundFrameIndex = (currentBackgroundFrameIndex + 1) % backgroundFrames.Count;
                    backgroundAnimationTimer -= TimePerBackgroundFrame;
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
                    ball.UpdateMovement();

                    ball.HandleWallCollisions(ballSoundInstance);

                    //paddle collision
                    if (ball.CheckPaddleCollision(paddle.Bounds))
                    {

                        ball.HandlePaddleHit(paddle.Bounds, paddleSoundInstance);
                    }

                    //brick collision logic

                    int hitBrickIndex = ball.CheckBrickCollision(bricks);
                    if (hitBrickIndex != -1)
                    {
                        bricks[hitBrickIndex].Hit(); //mark the Brick object as hit

                        ball.HandleBrickHit(brickSoundInstance);
                    }

                    bricks.RemoveAll(b => !b.IsActive); //remove inactive bricks

                    ball.ApplySpeedLimit();

                    //out of bounds check
                    if (ball.Bounds.Y > _graphics.PreferredBackBufferHeight)
                    {
                        gameStart = false;
                        ball.Reset(paddle.Bounds);
                        InitializeBricks(); //reset bricks when ball goes out
                    }
                }
            }

            //reset if r is pressed
            if (OnPress(keyboardState, previousKeyboardState, Keys.R))
            {
                gameStart = false;
                ball.Reset(paddle.Bounds);
                InitializeBricks();
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