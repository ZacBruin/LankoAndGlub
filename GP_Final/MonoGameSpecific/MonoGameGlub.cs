using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GP_Final
{
    public sealed class MonoGameGlub : DrawableSprite, ILankoObserver
    {
        internal GameConsoleGlub gcGlub { get; private set; }

        public MonoGameLanko Lanko;

        public Vector2 Center
        {
            get { return new Vector2(Location.X + Hitbox.Width / 2, Location.Y + Hitbox.Height / 2); }
        }

        public bool HasBounced;

        private Vector2 cachedLocation;
        private LevelBorder border;

        private Texture2D 
            runSheet, 
            ballSheet, 
            catchBurstSheet;

        private SpriteSheetInfo 
            runSheetInfo, 
            ballSheetInfo,
            burstSheetInfo, 
            currentSheetInfo;

        private float
            ground,
            distanceFromLanko,
            airSpeed,
            timeOnBuffPickup;

        private int
            currentWallBounces,
            currentPostFallFloorBounces,
            runSheetUpdateCount,
            ballSheetUpdateCount,
            burstSheetUpdateCount,
            updatesPerFlickerFrame;

        private bool isAnimatingBurst;

        private Vector2 GlubHeldLocation
        {
            get { return new Vector2(Lanko.Location.X + 15, Lanko.Location.Y - 15); }
        }

        private GlubState state;
        public GlubState State
        {
            get { return state; }

            set
            {
                if (state != value)
                {
                    state = gcGlub.Pub_State = value;

                    if(state == GlubState.Still || state == GlubState.Following)
                    {                        
                        SwapSpriteSheet(runSheet, runSheetInfo);
                        ballSheetInfo.CurrentFrame = 0;
                    }

                    else
                        SwapSpriteSheet(ballSheet, ballSheetInfo);
                }
            }
        }

        private bool withLanko;
        public bool WithLanko
        {
            get { return withLanko; }

            private set
            {
                if (withLanko != value)
                    withLanko = gcGlub.WithLanko = value;
            }
        }

        private bool hasStrongBuff;
        public bool HasStrongBuff
        {
            get { return hasStrongBuff; }
            set
            {
                if (hasStrongBuff != value)
                    hasStrongBuff = gcGlub.HasStrongBuff = value;

                if (value == true)
                {
                    airSpeed = BUFF_SPEED;
                    color = Color.Green;
                }

                else
                { 
                    color = Color.White;
                    updatesPerFlickerFrame = 2;
                    airSpeed = BASE_THROWN_SPEED;
                }
            }
        }

        #region Consts
        //Assets
        private const string GLUB_RUN_SPRITE_SHEET = "SpriteSheets/GlubRun";
        private const string GLUB_BALL_SPRITE_SHEET = "SpriteSheets/GlubBall";
        private const string GLUB_CATCH_BURST_SPRITE_SHEET = "SpriteSheets/GlubBurst";

        private const int RUN_SHEET_FRAMES = 4;
        private const int BALL_SHEET_FRAMES = 5;
        private const int BURST_SHEET_FRAMES = 5;

        private const int UPDATES_PER_RUN_FRAME = 6;
        private const int UPDATES_PER_BALL_FRAME = 4;
        private const int UPDATES_PER_BURST_FRAME = 3;

        //Limits
        private const int MAX_WALL_BOUNCES = 3;
        private const int MAX_POST_FALL_FLOOR_BOUNCES = 6;
        private const float MAX_BUFF_TIME = 10;

        //Speeds
        private const float GROUND_SPEED = 200;
        private const float BASE_THROWN_SPEED = 750;
        private const float SEEK_LANKO_SPEED = 500;
        private const float BUFF_SPEED = 900;
        private const float FALLING_RATE = .03f;

        //Scales
        private const float SPRITE_SCALE = .13f;
        private const float BURST_SHEET_SCALE = .18f;
        #endregion

        public MonoGameGlub(Game game) : base (game)
        {
            gcGlub = new GameConsoleGlub((GameConsole)game.Services.GetService<IGameConsole>());
        }

        protected override void LoadContent()
        {
            updatesPerFlickerFrame = 3;

            runSheet = content.Load<Texture2D>(GLUB_RUN_SPRITE_SHEET);
            runSheetInfo = new SpriteSheetInfo(RUN_SHEET_FRAMES, runSheet.Width, runSheet.Height, UPDATES_PER_RUN_FRAME);

            ballSheet = content.Load<Texture2D>(GLUB_BALL_SPRITE_SHEET);
            ballSheetInfo = new SpriteSheetInfo(BALL_SHEET_FRAMES, ballSheet.Width, ballSheet.Height, UPDATES_PER_BALL_FRAME);

            catchBurstSheet = content.Load<Texture2D>(GLUB_CATCH_BURST_SPRITE_SHEET);
            burstSheetInfo = new SpriteSheetInfo(BURST_SHEET_FRAMES, catchBurstSheet.Width, catchBurstSheet.Height, UPDATES_PER_BURST_FRAME);

            spriteTexture = runSheet;
            spriteSheetFramesWide = RUN_SHEET_FRAMES;

            border = Lanko.Border;

            Scale = SPRITE_SCALE;
            Speed = GROUND_SPEED;
            UpdateHitbox();
                
            Direction = Vector2.Zero;
            WithLanko = true;
            HasStrongBuff = false;

            SetTranformAndRect();
            SourceRectangle = runSheetInfo.SourceFrame;

            base.LoadContent();
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();

            if(isAnimatingBurst)
            {
                spriteBatch.Draw(catchBurstSheet, cachedLocation, burstSheetInfo.SourceFrame, Color.White, 0f, Vector2.Zero, BURST_SHEET_SCALE, SpriteEffects.None, 0f);

                CycleBurstAnimation();

                if (burstSheetInfo.CurrentFrame == BURST_SHEET_FRAMES)
                    ResetBurstAnimation();
            }

            spriteBatch.End();
            base.Draw(gameTime);
        }

        public override void Update(GameTime gameTime)
        {
            if (!Lanko_And_Glub.utility.IsGamePaused)
            {
                UpdateGlub(gameTime.ElapsedGameTime.TotalMilliseconds / 1000);

                if (HasStrongBuff)
                {
                    CheckBuffTime(gameTime);
                    MakeShimmer();
                }
            }

            base.Update(gameTime);
        }

        private void UpdateGlub(double timeElapsed)
        {
            SettleBorderCollision();

            #region State Logic Switch 

            switch (State)
            {
                case GlubState.Following:
                    float distanceFromLanko = Center.X - Lanko.Center.X;

                    if (distanceFromLanko > 0 && (Direction != new Vector2(-1, 0) || SpriteEffects != SpriteEffects.FlipHorizontally))
                    {
                        Direction = new Vector2(-1, 0);
                        SpriteEffects = SpriteEffects.FlipHorizontally;
                    }

                    else if (distanceFromLanko < 0 && (Direction != new Vector2(1, 0) || SpriteEffects != SpriteEffects.None))
                    {
                        Direction = new Vector2(1, 0);
                        SpriteEffects = SpriteEffects.None;
                    }

                    CheckIfFollowingLanko();
                    CycleRunAnimation();
                    break;

                case GlubState.Still:
                    if (Direction != Vector2.Zero)
                    {
                        Direction = Vector2.Zero;
                        runSheetInfo.CurrentFrame = 0;
                        runSheetInfo.UpdateSourceFrame();
                        SourceRectangle = runSheetInfo.SourceFrame;
                    }

                    CheckIfFollowingLanko();
                    break;

                case GlubState.Held:
                    Location = GlubHeldLocation;
                    CycleBallAnimation(true);

                    //Puts Glub back on the ground after Lanko lands (post catch)
                    if (Lanko.IsAiming == false && Lanko.HasJumped == false)
                    {
                        Location = new Vector2(Lanko.Location.X - 40, ground - (SpriteTexture.Height * Scale));
                        State = GlubState.AnimCoolDown;
                    }
                    break;

                case GlubState.Falling:
                    Direction.Y += FALLING_RATE;

                    if (Direction.X != 0)
                    {
                        if (Direction.X < 0)
                            Direction.X += .05f;

                        if (Direction.X > 0)
                            Direction.X -= .05f;

                        if (Direction.X > -.05f && Direction.X < .05f)
                            Direction.X = 0;
                    }

                    Location += ((Direction) * Speed * (float)timeElapsed);

                    if (currentPostFallFloorBounces >= MAX_POST_FALL_FLOOR_BOUNCES)
                        SetUpGlubStrandedState();
                    break;

                case GlubState.AnimCoolDown:
                    CycleBallAnimation(false);

                    if (ballSheetInfo.CurrentFrame == 0)
                        State = GlubState.Still;
                    break;

                case GlubState.SeekingLanko:
                    CycleBallAnimation(true);

                    if(Lanko.HasGlub == false)
                        Direction = SeekLanko();
                    break;

                case GlubState.Thrown:
                    CycleBallAnimation(true);

                    if (currentWallBounces >= MAX_WALL_BOUNCES)
                        SetUpGlubFallingState();
                    break;

                case GlubState.Stranded:
                    CycleBallAnimation(false);
                    break;
            }

            #endregion

            SaveGlubFromDeath();

            if(Math.Abs(Direction.Length()) > 0 && State != GlubState.Falling)
                Location += (Vector2.Normalize(Direction) * Speed * (float)timeElapsed);

            UpdateHitbox();
        }

        private void SetUpGlubFallingState()
        {
            State = GlubState.Falling;
            Direction.Y = 0;
            Direction.X = Direction.X / 500;
            currentWallBounces = 0;
        }

        private void SetUpGlubStrandedState()
        {
            State = GlubState.Stranded;
            Location.Y = ground - (spriteTexture.Height * scale);
            Direction = Vector2.Zero;
            currentPostFallFloorBounces = 0;
        }

        #region Misc. Methods
        private void CheckIfFollowingLanko()
        {
            distanceFromLanko = Math.Abs(Center.X - Lanko.Center.X);
            State = (distanceFromLanko >= 50) ? GlubState.Following : GlubState.Still;
        }

        private void SettleBorderCollision()
        {
            foreach (Wall w in border.Walls)
            {
                if (Hitbox.Intersects(w.LocationRect))
                {
                    if (State == GlubState.Thrown)
                    {
                        HasBounced = true;
                        currentWallBounces++;
                    }

                    else if (State == GlubState.Falling)
                        currentPostFallFloorBounces++;

                    Rectangle rect = Intersection(Hitbox, w.LocationRect);

                    //Top and Bottom borders
                    if (w == border.Walls[0] || w == border.Walls[2])
                    {
                        if (state != GlubState.Following && state != GlubState.Still)
                        {
                            Direction.Y *= -1;

                            if (State == GlubState.Falling)
                                Direction.Y = Direction.Y / 1.5f;

                            if (rect.Top < Center.Y)
                                Location.Y += rect.Height;

                            else if (rect.Bottom > Center.Y)
                                Location.Y -= rect.Height;
                        }
                    }

                    //Left and Right borders
                    else if (w == border.Walls[1] || w == border.Walls[3])
                    {
                        Direction.X *= -1;

                        if (rect.Right > Center.X)
                            Location.X -= rect.Width;

                        else if (rect.Left < Center.X)
                            Location.X += rect.Width;
                    }
                    UpdateHitbox();
                }

            }
        }

        private Vector2 SeekLanko()
        {
            return (Vector2.Normalize(Lanko.Center - Location));
        }

        //Called when Glub hits an item that he can damage
        public void ThrownToSeeking()
        {
            if (State == GlubState.Thrown)
            {
                State = GlubState.SeekingLanko;
                Speed = SEEK_LANKO_SPEED;
            }
        }

        private void UpdateHitbox()
        {
            int hitBoxWidthReduction = 8;
            int hitBoxHeightReduction = 3;

            locationRect.Location = Location.ToPoint();

            float scaledHeight = runSheetInfo.SourceFrame.Height * scale;
            float scaledWidth = runSheetInfo.SourceFrame.Width * scale;

            Hitbox = new Rectangle(
                LocationRect.X + hitBoxWidthReduction, 
                LocationRect.Y + hitBoxHeightReduction,
                (int)scaledWidth - hitBoxWidthReduction * 2, 
                (int)scaledHeight - hitBoxHeightReduction * 2);
        }

        public void GetCaughtByLanko()
        {
            currentWallBounces = 0;
            cachedLocation = new Vector2(LocationRect.X, LocationRect.Y);
            isAnimatingBurst = true;
            Speed = GROUND_SPEED;
            Direction = Vector2.Zero;
            HasBounced = false;

            if (Lanko.HasJumped)
                State = GlubState.Held;

            else
            {
                State = GlubState.AnimCoolDown;
                Location = new Vector2(Lanko.Location.X - 40, ground - locationRect.Height);
            }
        }

        public void CheckBuffTime(GameTime gameTime)
        {
            if ((gameTime.TotalGameTime.TotalMilliseconds / 1000) - timeOnBuffPickup > MAX_BUFF_TIME)
                HasStrongBuff = false;
        }

        public void GetBuffed(GameTime gameTime)
        {
            HasStrongBuff = true;
            timeOnBuffPickup = (float)gameTime.TotalGameTime.TotalMilliseconds / 1000;
        }
        #endregion

        #region Animation Methods

        private void MakeShimmer()
        {
            if(updatesPerFlickerFrame > 0)
                updatesPerFlickerFrame--;

            else
            {
                updatesPerFlickerFrame = 2;
                color = (color == Color.White) ? Color.LightGreen : Color.White;
            }
        }

        private void SwapSpriteSheet(Texture2D spriteSheet, SpriteSheetInfo info)
        {
            spriteTexture = spriteSheet;

            runSheetInfo.CurrentFrame = 0;
            spriteSheetFramesWide = info.TotalFrames;

            info.UpdateSourceFrame();
            SourceRectangle = info.SourceFrame;

            if (info == ballSheetInfo)
                runSheetUpdateCount = 0;
            else if (info == runSheetInfo)
                ballSheetUpdateCount = 0;

            currentSheetInfo = info;
            UpdateHitbox();
        }

        private void CycleRunAnimation()
        {
            if (runSheetUpdateCount >= UPDATES_PER_RUN_FRAME)
            {
                runSheetInfo.CurrentFrame++;
                runSheetUpdateCount = 0;

                if (runSheetInfo.CurrentFrame > RUN_SHEET_FRAMES - 1)
                    runSheetInfo.CurrentFrame = 0;

                runSheetInfo.UpdateSourceFrame();
                SourceRectangle = runSheetInfo.SourceFrame;
            }

            else
                runSheetUpdateCount++;
        }

        private void CycleBallAnimation(bool IsCyclingForward)
        {
            if (IsCyclingForward)
            {
                if (ballSheetInfo.CurrentFrame == BALL_SHEET_FRAMES - 1)
                    return;
            }

            else
                if (ballSheetInfo.CurrentFrame == 0)
                {
                    SwapSpriteSheet(runSheet, runSheetInfo);
                    ballSheetInfo.CurrentFrame = 0;
                    return;
                }

            if (ballSheetUpdateCount >= UPDATES_PER_BALL_FRAME)
            {
                if (IsCyclingForward)
                    ballSheetInfo.CurrentFrame++;
                else
                    ballSheetInfo.CurrentFrame--;

                ballSheetUpdateCount = 0;

                if (ballSheetInfo.CurrentFrame > BALL_SHEET_FRAMES - 1)
                    ballSheetInfo.CurrentFrame = 0;

                ballSheetInfo.UpdateSourceFrame();
                SourceRectangle = ballSheetInfo.SourceFrame;
            }

            else
                ballSheetUpdateCount++;
        }

        private void CycleBurstAnimation()
        {
            if (burstSheetInfo.CurrentFrame == BURST_SHEET_FRAMES)
                return;

            if (burstSheetUpdateCount >= UPDATES_PER_BURST_FRAME)
            {
                burstSheetInfo.CurrentFrame++;
                burstSheetUpdateCount = 0;

                burstSheetInfo.UpdateSourceFrame();           
            }

            else
                burstSheetUpdateCount++;
        }

        private void ResetBurstAnimation()
        {
            isAnimatingBurst = false;
            burstSheetInfo.CurrentFrame = 0;
            burstSheetUpdateCount = 0;
            burstSheetInfo.UpdateSourceFrame();
        }

        #endregion

        #region Hacky Methods
        //HACK: Didn't want to deal with the screwy call orders. Called in Lanko's LoadContent()
        public void SetStartLocationAndGround()
        {
            ground = Lanko.Ground;
            Location = new Vector2(Lanko.Location.X - 40, ground - (SpriteTexture.Height * Scale));

            SetTranformAndRect();
        }

        //HACK: Sometimes Glub gets out of the borders
        private void SaveGlubFromDeath()
        {
            if (Center.X < border.LeftRect.Left ||
                Center.X > border.RightRect.Right ||
                Center.Y < border.TopRect.Top ||
                Center.Y > border.BottomRect.Bottom)
            {
                GetCaughtByLanko();
                Lanko.HasGlub = true;
            }
        }
        #endregion

        #region Observer Pattern Methods
        public void Update()
        {
            
        }

        public void Update (bool b, string s)
        {
            switch (s)
            {
                case "IsAiming":
                    if(b) State = GlubState.Held;
                    break;

                case "HasGlub":
                    if (b == false)
                    {
                        State = GlubState.Thrown;
                        HasBounced = false;
                        Speed = airSpeed;
                        Direction = Lanko.AimDirection;
                    }

                    break;

                default: gcGlub.Log("BoolUpdate Error: Incorrect string input, check code.");
                    return;
            }
        }
        #endregion

    }
}
