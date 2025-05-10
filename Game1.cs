using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SumBreakout
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        //enums for gamestates
        public enum GameState
        {
            Intro,
            Game,
            Win,
            Lose
        }

        //screens for gamestates
        private Texture2D introScreen, winScreen, loseScreen;

        //activates on first press
        public static bool OnPress(KeyboardState KeyboardState, KeyboardState PreviousKeyboardStat, Keys _Key)
        {
            if (KeyboardState.IsKeyDown(_Key) && PreviousKeyboardStat.IsKeyUp(_Key)) 
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
        List<Brick> bricks = new List<Brick>();

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
        KeyboardState keyboardState, previousKeyboardState;

        //import soundeffects
        SoundEffect ballSound, paddleSound, brickSound;
        SoundEffectInstance ballSoundInstance, paddleSoundInstance, brickSoundInstance;

        //music
        private SoundEffect backgroundMusicEffect; 
        private SoundEffectInstance backgroundMusicInstance;
        //voice line
        private SoundEffect introAudio, Gameaudio, WinAudio, LoseAudio;
        private SoundEffectInstance introAudioInstance, GameaudioInstance, WinAudioInstance, LoseAudioInstance;

        //background
        List<Texture2D> backgroundFrames;
        int currentBackgroundFrameIndex;
        float backgroundAnimationTimer;
        const float TimePerBackgroundFrame = 1.0f / 24.0f;
        const int NumberOfBackgroundFrames = 81;

        //gamestate
        private GameState currentGameState;



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

            //reset game
            currentGameState = GameState.Intro;
            ResetGame();

            //setup
            InitializeBricks();
            ball.Reset(paddle.Bounds);
            keyboardState = Keyboard.GetState();
            previousKeyboardState = keyboardState;

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

            //lod music
            backgroundMusicEffect = Content.Load<SoundEffect>("bgMusic");

            //load voice lines
            introAudio = Content.Load<SoundEffect>("voice/intro");
            Gameaudio = Content.Load<SoundEffect>("voice/game");
            WinAudio = Content.Load<SoundEffect>("voice/win");
            LoseAudio = Content.Load<SoundEffect>("voice/loss");

            //load background
            for (int i = 1; i <= NumberOfBackgroundFrames; i++)
            {
                backgroundFrames.Add(Content.Load<Texture2D>($"bg/bg({i})"));
            }

            //load screens
            introScreen = Content.Load<Texture2D>("gamescreen/intro");
            winScreen = Content.Load<Texture2D>("gamescreen/win");
            loseScreen = Content.Load<Texture2D>("gamescreen/loss");

            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            //setup 
            previousKeyboardState = keyboardState;
            keyboardState = Keyboard.GetState();
            mouseState = Mouse.GetState();



            //play and loop music
            if (backgroundMusicInstance == null)
            {
                backgroundMusicInstance = backgroundMusicEffect.CreateInstance();
                backgroundMusicInstance.IsLooped = true;
                backgroundMusicInstance.Volume = 0.2f;
                backgroundMusicInstance.Play();


            }

            //if screen intro
            if (currentGameState == GameState.Intro)
            {
                if (introAudioInstance == null)
                {

                    introAudioInstance = introAudio.CreateInstance();
                    introAudioInstance.IsLooped = false;
                    introAudioInstance.Play();
                }

                if (OnPress(keyboardState, previousKeyboardState, Keys.Enter))
                {
                    introAudioInstance?.Stop();
                    introAudioInstance?.Dispose();
                    introAudioInstance = null;
                    currentGameState = GameState.Game;
                }
            }
            //if screen game
            if (currentGameState == GameState.Game)
            {

                //play voice line
                if (GameaudioInstance == null)
                {
                    backgroundMusicInstance.Volume = 0.1f;
                    GameaudioInstance = Gameaudio.CreateInstance();
                    GameaudioInstance.IsLooped = false;
                    GameaudioInstance.Play();
                    backgroundMusicInstance.Volume = 0.2f;
                }
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
                            //go to lose screen
                            currentGameState = GameState.Lose;
                        }

                        //if all bricks are hit
                        if (bricks.Count == 0)
                        {
                            gameStart = false;
                            //go to win screen
                            currentGameState = GameState.Win;
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
            }

            //if screen win
            else if (currentGameState == GameState.Win)
            {
                if (WinAudioInstance == null)
                {
                    //stop bugging audio
                    GameaudioInstance.Stop();
                    GameaudioInstance.Dispose();
                    GameaudioInstance = null;

                    //lower volume
                    if (backgroundMusicInstance != null)
                    {
                        backgroundMusicInstance.Volume = 0.1f;
                    }

                    WinAudioInstance = WinAudio.CreateInstance();
                    WinAudioInstance.IsLooped = false;
                    WinAudioInstance.Play();
                }
                //reset game
                bool resetToIntro = false;

                if (OnPress(keyboardState, previousKeyboardState, Keys.Space)) //space pressed
                {
                    resetToIntro = true;
                }
                else if (WinAudioInstance != null && WinAudioInstance.State == SoundState.Stopped) //audio stopped
                {
                    resetToIntro = true;
                }

                if (resetToIntro) //reset
                {
                    {
                        WinAudioInstance.Stop();
                        WinAudioInstance.Dispose();
                    }
                    WinAudioInstance = null;

                    if (backgroundMusicInstance != null)
                    {
                        backgroundMusicInstance.Volume = 0.2f;
                    }

                    currentGameState = GameState.Intro;
                    ResetGame();
                }
            }

            //if screen lose
            else if (currentGameState == GameState.Lose)
            {
                if (LoseAudioInstance == null)
                {
                    //stop bugging audio
                    GameaudioInstance?.Stop();
                    GameaudioInstance?.Dispose();
                    GameaudioInstance = null;

                    //lower volume
                    if (backgroundMusicInstance != null)
                    {
                        backgroundMusicInstance.Volume = 0.1f;
                    }


                    LoseAudioInstance = LoseAudio.CreateInstance();
                    LoseAudioInstance.IsLooped = false;
                    LoseAudioInstance.Play();
                }
                //reset game
                bool resetToIntro = false;

                if (OnPress(keyboardState, previousKeyboardState, Keys.Space)) //space pressed
                {
                    resetToIntro = true;
                }
                else if (LoseAudioInstance != null && LoseAudioInstance.State == SoundState.Stopped) //audio stopped
                {
                    resetToIntro = true;
                }

                if (resetToIntro) //reset
                {
                    LoseAudioInstance.Stop();
                    LoseAudioInstance.Dispose();
                    LoseAudioInstance = null;

                    if (backgroundMusicInstance != null)
                    {
                        backgroundMusicInstance.Volume = 0.2f;
                    }

                    currentGameState = GameState.Intro;
                    ResetGame();
                }
                base.Update(gameTime);
            }
        }
            
        

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();

            //if screen intro
            if (currentGameState == GameState.Intro)
            {
                _spriteBatch.Draw(introScreen, new Rectangle(0, 0, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight), Color.White);
            }


            //if screen game
            if (currentGameState == GameState.Game)
            {
                if (backgroundFrames.Count > 0 && currentBackgroundFrameIndex < backgroundFrames.Count)
                {
                    _spriteBatch.Draw(backgroundFrames[currentBackgroundFrameIndex],
                                      new Rectangle(0, 0, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight),
                                      Color.White);
                }

                //paddle
                paddle.Draw(_spriteBatch);

                //ball
                ball.Draw(_spriteBatch);

                //bricks
                foreach (Brick brick in bricks)
                {
                    brick.Draw(_spriteBatch, brickTexture);
                }
            }

            //if screen win
            if (currentGameState == GameState.Win)
            {
                _spriteBatch.Draw(winScreen, new Rectangle(0, 0, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight), Color.White);
            }

            //if screen lose
            if (currentGameState == GameState.Lose)
            {
                _spriteBatch.Draw(loseScreen, new Rectangle(0, 0, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight), Color.White);
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

        private void ResetGame()
        {
            paddle.Reset();
            ball.Reset(paddle.Bounds);
            InitializeBricks();
        }
    }
}