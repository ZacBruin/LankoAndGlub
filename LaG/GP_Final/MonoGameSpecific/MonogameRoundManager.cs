using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;

namespace GP_Final
{
    //Should have made this inherit from a generalized GameRoundManager
    public class MonogameRoundManager : DrawableSprite
    {
        public GameRound round;
        public LevelBorder border;

        bool newHighScore;

        private float RoundTransitionCrement = .007f;
        private float timeInstructionsStayAround;

        Texture2D placeHolder;
        Song roundMusic, waitMusic;
        SpriteFont font;
        Color fontColor, instructionsColor, timerColor;

        SoundEffect NewHigh, EndRound;

        public enum MusicState { RoundTrack, RoundToWait, WaitToRound, WaitTrack}
        public MusicState musicState = MusicState.RoundTrack;

        public int PlayerScore
        {
            get
            {
                return this.round.Points;
            }
        }
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

        public bool HasStartedRound, FirstRoundStartHasStarted;

        private bool firstRoundOver;
        
        private float fontScale, midpointLeft, midpointRight, timeLeftInRound;     

        public MonogameRoundManager(Game game) : base(game)
        {
            this.round = new GameRound(game);
        }

        protected override void LoadContent()
        {
            this.font = content.Load<SpriteFont>("ConsoleFont");

            this.placeHolder = content.Load<Texture2D>("Sprites/Instructions");
            this.spriteTexture = content.Load<Texture2D>("SpriteMarker");
            this.roundMusic = content.Load<Song>("Music/Round");
            this.waitMusic = content.Load<Song>("Music/BetweenRounds");

            NewHigh = content.Load<SoundEffect>("SFX/NewHighScore");
            EndRound = content.Load<SoundEffect>("SFX/GameEnd");

            timeInstructionsStayAround = 10;

            MediaPlayer.Play(roundMusic);
            MediaPlayer.Volume = .3f;
            MediaPlayer.IsRepeating = true;          

            this.FirstRoundStartHasStarted = this.firstRoundOver = false;
            this.HasStartedRound = false;

            this.highScore = 0;

            if (!Lanko_And_Glub.utility.ShowInstructions)
                this.instructionsColor = new Color(0, 0, 0, 0);
            else
                this.instructionsColor = Color.White;

            this.scale = 0;

            this.fontScale = 1f;
            this.fontColor = timerColor = Color.White;

            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            if (!Lanko_And_Glub.utility.GamePaused)
            {
                updateGameRound(gameTime);
            }

            if (Lanko_And_Glub.utility.GamePaused && this.FirstRoundStartHasStarted)
                this.instructionsColor = new Color(255, 255, 255, 255);
            else if (!Lanko_And_Glub.utility.GamePaused && this.FirstRoundStartHasStarted)
            {
                if (this.instructionsColor.A != 0)
                    this.instructionsColor = new Color(0, 0, 0, 0);
            }

            switch(musicState)
            {
                case MusicState.RoundTrack:
                    if (MediaPlayer.Volume < .3f)
                        MediaPlayer.Volume += RoundTransitionCrement;

                    if (FirstRoundStartHasStarted)
                    {
                        if (round.RoundIsOver)
                        {
                            musicState = MusicState.RoundToWait;

                            if(!newHighScore)
                                EndRound.Play(.5f, 0, 0);

                            if(newHighScore)
                                NewHigh.Play(.3f, 0, 0);
                        }
                    }
                    break;

                case MusicState.WaitTrack:
                    if(MediaPlayer.Volume < .3f)
                        MediaPlayer.Volume += RoundTransitionCrement;

                    if (!round.RoundIsOver)
                        musicState = MusicState.WaitToRound;
                    break;

                case MusicState.RoundToWait:
                    if (MediaPlayer.Volume > 0)
                        MediaPlayer.Volume -= RoundTransitionCrement;
                    else
                    {
                        musicState = MusicState.WaitTrack;
                        MediaPlayer.Play(waitMusic);
                    }
                    break;

                case MusicState.WaitToRound:
                    if (MediaPlayer.Volume > 0)
                        MediaPlayer.Volume -= RoundTransitionCrement;
                    else
                    {
                        musicState = MusicState.RoundTrack;
                        MediaPlayer.Play(roundMusic);
                        newHighScore = false;
                    }
                    break;
            }


            base.Update(gameTime);
        }

        private void updateGameRound(GameTime gameTime)
        {
            this.round.Update(gameTime);

            if (timeLeftInRound < 10 && timeLeftInRound > 0)
                timerColor = Color.Red;
            else
                timerColor = Color.White;

            if (gameTime.TotalGameTime.TotalSeconds > timeInstructionsStayAround)
                FadeOutInstructionsImage();

            if (FirstRoundStartHasStarted == false && instructionsColor.R == 0)
                StartFirstRound();
            
            if (round.RoundIsOver == false)
                timeLeftInRound = (round.MaxRoundLength - round.CurrentRoundTime);

            else if (round.RoundIsOver && HasStartedRound)
            {
                firstRoundOver = true;
                timeLeftInRound = 0;
                HasStartedRound = false;
                round.ResetRound();
            }
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();

            if (PlayerScore != 0)
            {
                int offset = 0;
                int scoreLength = PlayerScore.ToString().Length;

                offset = scoreLength == 1 ? 100 : 110;

                spriteBatch.DrawString(font, "SCORE: " + PlayerScore.ToString(),
                    new Vector2(midpointRight - offset, border.Walls[2].LocationRect.Top - 595),
                    fontColor, 0f, new Vector2(0, 0), fontScale / 1.5f, SpriteEffects.None, 0f);
            }

            //Draws the amount of time left in a round
            float timerHorizOffset = 0;
            timerHorizOffset = timeLeftInRound >= 10 ? 100 : 71;

            spriteBatch.DrawString(font, timeLeftInRound.ToString("0.00"),
                new Vector2(midpointRight - timerHorizOffset, Game.GraphicsDevice.Viewport.Bounds.Top + 15),
                timerColor, 0f, new Vector2(0, 0), fontScale, SpriteEffects.None, 0f);

            if (HighScore != 0 && firstRoundOver)
            {
                spriteBatch.DrawString(font, "High: " + HighScore.ToString(),
                    new Vector2(midpointRight - 67, border.Walls[2].LocationRect.Top - 420),
                    Color.White, 0f, new Vector2(0, 0), fontScale/1.5f, SpriteEffects.None, 0f);
            }

            spriteBatch.Draw(placeHolder, Location, null, instructionsColor, 0, new Vector2(0, 0), .76f,
                SpriteEffects.None, 0);

            DrawCredits();
            DrawInstructions();

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawCredits()
        {
            float fontSize = fontScale / 2.6f;
            float horizPosition = midpointRight - 90;

            spriteBatch.DrawString(font, "Programming:",
                new Vector2(horizPosition, this.border.Walls[0].LocationRect.Top + 405),
                Color.Purple, 0f, new Vector2(0, 0), fontSize, SpriteEffects.None, 0f);

            spriteBatch.DrawString(font, "Zac Bruin",
                new Vector2(horizPosition, this.border.Walls[0].LocationRect.Top + 420),
                Color.Purple, 0f, new Vector2(0, 0), fontSize, SpriteEffects.None, 0f);

            spriteBatch.DrawString(font, "Art:",
                new Vector2(horizPosition, this.border.Walls[0].LocationRect.Top + 445),
                Color.Purple, 0f, new Vector2(0, 0), fontSize, SpriteEffects.None, 0f);

            spriteBatch.DrawString(font, "Becca Hallstedt",
                new Vector2(horizPosition, this.border.Walls[0].LocationRect.Top + 460),
                Color.Purple, 0f, new Vector2(0, 0), fontSize, SpriteEffects.None, 0f);

            spriteBatch.DrawString(font, "Sound:",
                new Vector2(horizPosition, this.border.Walls[0].LocationRect.Top + 485),
                Color.Purple, 0f, new Vector2(0, 0), fontSize, SpriteEffects.None, 0f);

            spriteBatch.DrawString(font, "Bret Merritt",
                new Vector2(horizPosition, this.border.Walls[0].LocationRect.Top + 500),
                Color.Purple, 0f, new Vector2(0, 0), fontSize, SpriteEffects.None, 0f);
        }

        private void DrawInstructions()
        {
            if (round.RoundIsOver && firstRoundOver)
            {
                spriteBatch.DrawString(font, "Start Round:",
                    new Vector2(midpointRight - 100, this.border.Walls[2].LocationRect.Top - 95),
                    fontColor, 0f, new Vector2(0, 0), fontScale / 2, SpriteEffects.None, 0f);

                spriteBatch.DrawString(font, "Right Click",
                    new Vector2(midpointRight - 100, this.border.Walls[2].LocationRect.Top - 75),
                    fontColor, 0f, new Vector2(0, 0), fontScale / 2, SpriteEffects.None, 0f);
            }

            spriteBatch.DrawString(font, "P: Instructions",
                new Vector2(midpointRight - 145, this.border.Walls[0].LocationRect.Top + 660),
                Color.White, 0f, new Vector2(0, 0), fontScale / 2, SpriteEffects.None, 0f);

            spriteBatch.DrawString(font, "ESC: Close Game",
                new Vector2(midpointRight - 145, this.border.Walls[0].LocationRect.Top + 685),
                Color.White, 0f, new Vector2(0, 0), fontScale / 2, SpriteEffects.None, 0f);
        }
        
        private void FadeOutInstructionsImage()
        {
            if (instructionsColor.R != 0)
                instructionsColor.R -= 5;

            if (instructionsColor.B != 0)
                instructionsColor.B -= 5;

            if (instructionsColor.G != 0)
                instructionsColor.G -= 5;

            if (instructionsColor.A != 0)
                instructionsColor.A -= 5;
        }

        //Calculated for the midpoints between the left and right edges of screen
        //and the left and right Level borders to help with text placement
        private void CalculateMidpoints()
        {
            this.midpointLeft = 
                ((this.border.Walls[3].LocationRect.Left - this.Game.GraphicsDevice.Viewport.Bounds.Left) / 2) +
                this.Game.GraphicsDevice.Viewport.Bounds.Left;

            this.midpointRight = 
                ((this.Game.GraphicsDevice.Viewport.Bounds.Right - this.border.Walls[1].LocationRect.Right) / 2) +
                this.border.Walls[1].LocationRect.Right;
        }

        public void FirstTimeSetup()
        {
            CalculateMidpoints();

            this.Location =
                new Vector2(this.border.Walls[3].LocationRect.Right + 115,
                this.border.Walls[0].LocationRect.Bottom);
        }

        private void StartFirstRound()
        {
            this.FirstRoundStartHasStarted = true;

            if (this.round.RoundIsOver == true)
            {
                this.round.RoundIsOver = false;
                this.HasStartedRound = true;
            }
        }

    }
}
