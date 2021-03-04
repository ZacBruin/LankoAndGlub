using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;

namespace GP_Final
{
    //Should have made this inherit from a generalized GameRoundManager
    public class MonogameRoundManager : DrawableSprite
    {
        public int PlayerScore { get{ return Round.Points; }}

        public GameRound Round;
        public LevelBorder Border;
        public bool HasStartedRound, FirstRoundStartHasStarted;

        private bool newHighScore, firstRoundOver;

        private float
            fontScale,
            midpointLeft,
            midpointRight,
            timeLeftInRound;

        private Texture2D placeHolder;
        private SpriteFont font;
        private Color 
            instructionsColor, 
            timerColor;

        private Song roundMusic, waitMusic;
        private SoundEffect NewHigh, EndRound;

        private MusicState musicState = MusicState.RoundTrack;
        public enum MusicState
        {
            RoundTrack,
            RoundToWait,
            WaitToRound,
            WaitTrack
        };

        private int highScore;
        public int HighScore
        {
            get
            {
                if (PlayerScore > highScore)
                {
                    highScore = PlayerScore;
                    newHighScore = true;
                }

                return highScore;
            }
        }

        #region Consts
        private const float INSTRUCTIONS_SPRITE_ON_SCREEN_LENGTH = 10;
        private const float INSTRUCTIONS_SPRITE_SCALE = .76f;

        private const float BASE_FONT_SCALE = 1f;
        private const int RED_TIMER_FONT_THRESHOLD = 10;
        private const int INSTRUCTIONS_COLOR_FADEOUT = 5;

        private const float MEDIA_PLAYER_BASE_VOL = .3f;
        private const float ROUND_TRANSITION_VOL_FADE = .007f;
        private const float NEW_HIGH_SFX_VOL = .3f;
        private const float ROUND_END_SFX_VOL = .5f;

        private const string FONT = "ConsoleFont";
        private const string SPRITE_MARKER = "SpriteMarker";
        private const string SPRITE_INSTRUCTIONS = "Sprites/Instructions";

        private const string MUSIC_ROUND = "Music/Round";
        private const string MUSIC_BETWEEN_ROUND = "Music/BetweenRounds";

        private const string NEW_HIGH_SFX = "SFX/NewHighScore";
        private const string ROUND_END_SFX = "SFX/GameEnd";
        #endregion

        public MonogameRoundManager(Game game) : base(game)
        {
            Round = new GameRound(game);
        }

        protected override void LoadContent()
        {
            font = content.Load<SpriteFont>(FONT);

            placeHolder = content.Load<Texture2D>(SPRITE_INSTRUCTIONS);
            spriteTexture = content.Load<Texture2D>(SPRITE_MARKER);
            roundMusic = content.Load<Song>(MUSIC_ROUND);
            waitMusic = content.Load<Song>(MUSIC_BETWEEN_ROUND);

            NewHigh = content.Load<SoundEffect>(NEW_HIGH_SFX);
            EndRound = content.Load<SoundEffect>(ROUND_END_SFX);

            MediaPlayer.Play(roundMusic);
            MediaPlayer.Volume = MEDIA_PLAYER_BASE_VOL;
            MediaPlayer.IsRepeating = true;          

            FirstRoundStartHasStarted = firstRoundOver = false;
            HasStartedRound = false;

            highScore = 0;
            instructionsColor = (!Lanko_And_Glub.utility.IsShowingInstructions) ? new Color(0, 0, 0, 0) : Color.White;
            scale = 0;

            fontScale = BASE_FONT_SCALE;
            timerColor = Color.White;

            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            if (gameTime.TotalGameTime.TotalSeconds > INSTRUCTIONS_SPRITE_ON_SCREEN_LENGTH)
                FadeOutInstructionsImage();

            if (!Lanko_And_Glub.utility.IsGamePaused)
            {
                if(FirstRoundStartHasStarted)
                    if (instructionsColor.A != 0)
                        instructionsColor = new Color(0, 0, 0, 0);

                updateGameRound(gameTime);
            }

            else if (FirstRoundStartHasStarted)
                instructionsColor = new Color(255, 255, 255, 255);

            timerColor = (timeLeftInRound < RED_TIMER_FONT_THRESHOLD && timeLeftInRound > 0)
                ? Color.Red
                : Color.White;

            HandleMusicState();

            base.Update(gameTime);
        }

        private void updateGameRound(GameTime gameTime)
        {
            Round.Update(gameTime);

            if (FirstRoundStartHasStarted == false && instructionsColor.R == 0)
                StartFirstRound();
            
            if (Round.RoundIsOver == false)
                timeLeftInRound = (Round.MaxRoundLength - Round.CurrentRoundTime);

            else if (HasStartedRound)
            {
                firstRoundOver = true;
                timeLeftInRound = 0;
                HasStartedRound = false;
                Round.ResetRound();
            }
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();
            //Draws Instructions Image
            spriteBatch.Draw(placeHolder, Location, null, instructionsColor, 0, Vector2.Zero, INSTRUCTIONS_SPRITE_SCALE, SpriteEffects.None, 0);

            DrawTimer();
            DrawScoreInfo();           
            DrawCredits();
            DrawTextInstructions();

            spriteBatch.End();
            base.Draw(gameTime);
        }

        private void DrawCredits()
        {
            float fontSize = fontScale / 2.6f;
            float horizPosition = midpointRight - 90;
            float topBorderWall = Border.TopRect.Top;
            Color textColor = Color.Purple;

            spriteBatch.DrawString(font, "Programming:", 
                new Vector2(horizPosition, topBorderWall + 405), textColor, 0f, Vector2.Zero, fontSize, SpriteEffects.None, 0f);

            spriteBatch.DrawString(font, "Zac Bruin",
                new Vector2(horizPosition, topBorderWall + 420), textColor, 0f, Vector2.Zero, fontSize, SpriteEffects.None, 0f);

            spriteBatch.DrawString(font, "Art:",
                new Vector2(horizPosition, topBorderWall + 445), textColor, 0f, Vector2.Zero, fontSize, SpriteEffects.None, 0f);

            spriteBatch.DrawString(font, "Becca Hallstedt",
                new Vector2(horizPosition, topBorderWall + 460), textColor, 0f, Vector2.Zero, fontSize, SpriteEffects.None, 0f);

            spriteBatch.DrawString(font, "Sound:",
                new Vector2(horizPosition, topBorderWall + 485), textColor, 0f, Vector2.Zero, fontSize, SpriteEffects.None, 0f);

            spriteBatch.DrawString(font, "Bret Merritt",
                new Vector2(horizPosition, topBorderWall + 500), textColor, 0f, Vector2.Zero, fontSize, SpriteEffects.None, 0f);
        }

        private void DrawTextInstructions()
        {
            float instructionsFontScale = fontScale / 2;
            Color fontColor = Color.White;

            if (Round.RoundIsOver && firstRoundOver)
            {
                spriteBatch.DrawString(font, "Start Round:",
                    new Vector2(midpointRight - 100, Border.BottomRect.Top - 95), fontColor, 0f, Vector2.Zero, instructionsFontScale, SpriteEffects.None, 0f);

                spriteBatch.DrawString(font, "Right Click",
                    new Vector2(midpointRight - 100, Border.BottomRect.Top - 75), fontColor, 0f, Vector2.Zero, instructionsFontScale, SpriteEffects.None, 0f);
            }

            spriteBatch.DrawString(font, "P: Instructions",
                new Vector2(midpointRight - 145, Border.TopRect.Top + 660), fontColor, 0f, Vector2.Zero, instructionsFontScale, SpriteEffects.None, 0f);

            spriteBatch.DrawString(font, "ESC: Close Game",
                new Vector2(midpointRight - 145, Border.TopRect.Top + 685), fontColor, 0f, Vector2.Zero, instructionsFontScale, SpriteEffects.None, 0f);
        }

        private void DrawTimer()
        {
            float timerHorizOffset = (timeLeftInRound >= 10) ? 100 : 71;

            spriteBatch.DrawString(font, timeLeftInRound.ToString("0.00"),
                new Vector2(midpointRight - timerHorizOffset, Game.GraphicsDevice.Viewport.Bounds.Top + 15),
                timerColor, 0f, Vector2.Zero, fontScale, SpriteEffects.None, 0f);
        }

        private void DrawScoreInfo()
        {
            Color fontColor = Color.White;
            float fontSize = fontScale / 1.5f;

            if (PlayerScore != 0)
            {
                int offset = 0;
                int scoreLength = PlayerScore.ToString().Length;

                offset = scoreLength == 1 ? 100 : 110;

                spriteBatch.DrawString(font, "SCORE: " + PlayerScore.ToString(),
                    new Vector2(midpointRight - offset, Border.BottomRect.Top - 595), fontColor, 0f, Vector2.Zero, fontSize, SpriteEffects.None, 0f);
            }

            if (HighScore != 0 && firstRoundOver)
            {
                spriteBatch.DrawString(font, "High: " + HighScore.ToString(),
                    new Vector2(midpointRight - 67, Border.BottomRect.Top - 420), fontColor, 0f, Vector2.Zero, fontSize, SpriteEffects.None, 0f);
            }
        }

        private void FadeOutInstructionsImage()
        {
            if (instructionsColor.R != 0)
                instructionsColor.R -= INSTRUCTIONS_COLOR_FADEOUT;

            if (instructionsColor.B != 0)
                instructionsColor.B -= INSTRUCTIONS_COLOR_FADEOUT;

            if (instructionsColor.G != 0)
                instructionsColor.G -= INSTRUCTIONS_COLOR_FADEOUT;

            if (instructionsColor.A != 0)
                instructionsColor.A -= INSTRUCTIONS_COLOR_FADEOUT;
        }

        public void FirstTimeSetup()
        {
            float leftEdgeOfScreen = Game.GraphicsDevice.Viewport.Bounds.Left;
            float rightEdgeOfScreen = Game.GraphicsDevice.Viewport.Bounds.Right;
            float rightEdgeOfRightBorder = Border.RightRect.Right;

            midpointLeft = ((Border.LeftRect.Left - leftEdgeOfScreen) / 2) + leftEdgeOfScreen;
            midpointRight = ((rightEdgeOfScreen - rightEdgeOfRightBorder) / 2) + rightEdgeOfRightBorder;

            Location = new Vector2(Border.LeftRect.Right + 115, Border.TopRect.Bottom);
        }

        private void StartFirstRound()
        {
            FirstRoundStartHasStarted = true;

            if (Round.RoundIsOver == true)
            {
                Round.RoundIsOver = false;
                HasStartedRound = true;
            }
        }

        private void HandleMusicState()
        {
            switch (musicState)
            {
                case MusicState.RoundTrack:
                    if (MediaPlayer.Volume < MEDIA_PLAYER_BASE_VOL)
                        MediaPlayer.Volume += ROUND_TRANSITION_VOL_FADE;

                    if (FirstRoundStartHasStarted)
                    {
                        if (Round.RoundIsOver)
                        {
                            musicState = MusicState.RoundToWait;

                            if (!newHighScore)
                                EndRound.Play(ROUND_END_SFX_VOL, 0, 0);
                            else
                                NewHigh.Play(NEW_HIGH_SFX_VOL, 0, 0);
                        }
                    }
                    break;

                case MusicState.WaitTrack:
                    if (MediaPlayer.Volume < MEDIA_PLAYER_BASE_VOL)
                        MediaPlayer.Volume += ROUND_TRANSITION_VOL_FADE;

                    if (!Round.RoundIsOver)
                        musicState = MusicState.WaitToRound;
                    break;

                case MusicState.RoundToWait:
                    if (MediaPlayer.Volume > 0)
                        MediaPlayer.Volume -= ROUND_TRANSITION_VOL_FADE;
                    else
                    {
                        musicState = MusicState.WaitTrack;
                        MediaPlayer.Play(waitMusic);
                    }
                    break;

                case MusicState.WaitToRound:
                    if (MediaPlayer.Volume > 0)
                        MediaPlayer.Volume -= ROUND_TRANSITION_VOL_FADE;
                    else
                    {
                        musicState = MusicState.RoundTrack;
                        MediaPlayer.Play(roundMusic);
                        newHighScore = false;
                    }
                    break;
            }
        }

    }
}
