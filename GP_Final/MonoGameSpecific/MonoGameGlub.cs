using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GP_Final
{
    public sealed class MonoGameGlub : DrawableSprite, ILankoObserver
    {
        internal GameConsoleGlub gcGlub { get; private set; }

        public MonoGameLanko lanko;
        public LevelBorder border;

        private Texture2D run_sheet, ball_sheet, catch_burst_sheet;

        private SpriteSheetInfo run_info, ball_info, burst_info, current_info;

        public float ground, groundSpeed, distanceFromLanko, 
            airSpeed, seekSpeed, buffSpeed, maxTimeAllowedBuffed, timeOnBuffPickup, burst_scale;

        public Vector2 center;
        private Vector2 cached_Location;

        private int updatesBetweenFlicker;

        public bool HasBounced;
        private bool isAnimatingBurst;
        public int numBounces, numBouncesAfterFalling, maxBounces, maxBouncesAfterFalling,
            run_Anim_Count, updates_Between_Run, ball_Anim_Count, updates_Between_Ball, 
            burst_Anim_Count, updates_Between_Burst;

        protected GlubState state;
        public GlubState State
        {
            get {return state;}

            set
            {
                if (state != value)
                {
                    state = gcGlub.Pub_State = value;

                    if(state == GlubState.Still || state == GlubState.Following)
                    {                        
                        SwapSpriteSheet(run_sheet, run_info);
                        ball_info.currentFrame = 0;
                    }

                    else
                    {
                        SwapSpriteSheet(ball_sheet, ball_info);
                    }
                }
            }
        }

        private bool withLanko, hasStrongBuff;

        public bool WithLanko
        {
            get { return withLanko; }

            private set
            {
                if (withLanko != value)
                    withLanko = gcGlub.WithLanko = value;
            }
        }

        public bool HasStrongBuff
        {
            get { return hasStrongBuff; }
            set
            {
                if (hasStrongBuff != value)
                    hasStrongBuff = gcGlub.HasStrongBuff = value;

                if (value == true)
                {
                    airSpeed = buffSpeed;
                    color = Color.Green;
                }

                else
                { 
                    color = Color.White;
                    updatesBetweenFlicker = 2;
                    airSpeed = 750;
                }
            }
        }

        public MonoGameGlub(Game game) : base (game)
        {
            gcGlub = new GameConsoleGlub((GameConsole)game.Services.GetService<IGameConsole>());
        }

        protected override void LoadContent()
        {
            updates_Between_Run = 6;
            updates_Between_Burst = 3;
            updates_Between_Ball = 4;
            updatesBetweenFlicker = 2;

            run_sheet = content.Load<Texture2D>("SpriteSheets/GlubRun");
            run_info = new SpriteSheetInfo(4, run_sheet.Width, run_sheet.Height, updates_Between_Run);

            ball_sheet = content.Load<Texture2D>("SpriteSheets/GlubBall");
            ball_info = new SpriteSheetInfo(5, ball_sheet.Width, ball_sheet.Height, updates_Between_Ball);

            catch_burst_sheet = content.Load<Texture2D>("SpriteSheets/GlubBurst");
            burst_info = new SpriteSheetInfo(5, catch_burst_sheet.Width, catch_burst_sheet.Height, updates_Between_Burst);

            spriteTexture = run_sheet;
            spriteSheetFramesWide = run_info.totalFrames;

            border = lanko.border;

            maxBounces = 3;
            maxBouncesAfterFalling = 6;

            maxTimeAllowedBuffed = 10;         

            Scale = .13f;
            burst_scale = .18f;
            groundSpeed = 200f;
            airSpeed = 750f;
            seekSpeed = 500f;
            buffSpeed = 900;

            Speed = groundSpeed;

            UpdateHitbox();
                
            Direction = new Vector2(0, 0);

            center = new Vector2(Location.X + Hitbox.Width / 2, Location.Y + Hitbox.Height / 2);

            WithLanko = true;
            HasStrongBuff = false;

            SetTranformAndRect();
            SourceRectangle = run_info.sourceFrame;

            base.LoadContent();
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();

            if(isAnimatingBurst)
            {
                spriteBatch.Draw(catch_burst_sheet, cached_Location, burst_info.sourceFrame, Color.White, 0f,
                    new Vector2(0,0), burst_scale, SpriteEffects.None, 0f);

                CycleBurstAnim();
                CheckBurstAnimComplete();
            }

            spriteBatch.End();
            base.Draw(gameTime);
        }

        public override void Update(GameTime gameTime)
        {
            if (!Lanko_And_Glub.utility.GamePaused)
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
                    if (center.X - lanko.center.X > 0)
                    {
                        Direction = new Vector2(-1, 0);
                        SpriteEffects = SpriteEffects.FlipHorizontally;
                    }

                    else if (center.X - lanko.center.X < 0)
                    {
                        Direction = new Vector2(1, 0);
                        SpriteEffects = SpriteEffects.None;
                    }

                    CheckIfFollowingLanko();
                    CycleRunAnim();
                    break;

                case GlubState.Still:
                    run_info.currentFrame = 0;
                    run_info.UpdateSourceFrame();
                    SourceRectangle = run_info.sourceFrame;
                    
                    Direction = new Vector2(0, 0);
                    CheckIfFollowingLanko();
                    break;

                case GlubState.Held:
                    Location = new Vector2(lanko.Location.X + 15, lanko.Location.Y - 15);

                    CycleBallAnim(true);

                    //Puts Glub back on the ground after Lanko lands (post catch)
                    if (lanko.IsAiming == false && lanko.HasJumped == false)
                    {
                        Location =
                            new Vector2(lanko.Location.X - 40, ground - (SpriteTexture.Height * Scale));

                        State = GlubState.AnimCoolDown;
                    }

                    break;

                case GlubState.Falling:
                    Direction.Y += .03f;

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

                    if (numBouncesAfterFalling >= maxBouncesAfterFalling)
                    {
                        State = GlubState.Stranded;
                        Location.Y = ground - (spriteTexture.Height * scale);
                        Direction = new Vector2(0, 0);
                        numBouncesAfterFalling = 0;
                    }

                    break;

                case GlubState.AnimCoolDown:
                    CycleBallAnim(false);

                    if (ball_info.currentFrame == 0)
                        State = GlubState.Still;

                    break;

                case GlubState.SeekingLanko:

                    CycleBallAnim(true);

                    if(lanko.HasGlub == false)
                        Direction = SeekLanko();

                    break;

                case GlubState.Thrown:
                        CycleBallAnim(true);

                    if (numBounces >= maxBounces)
                    {
                        State = GlubState.Falling;
                        Direction.Y = 0;
                        Direction.X = Direction.X / 500;
                        numBounces = 0;
                    }
                    break;

                case GlubState.Stranded:
                    CycleBallAnim(false);
                    break;
            }

            #endregion

            SaveGlubFromDeath();

            if(Math.Abs(Direction.Length()) > 0 && State != GlubState.Falling)
                Location += (Vector2.Normalize(Direction) * Speed * (float)timeElapsed);

            center = new Vector2(Location.X + Hitbox.Width / 2, Location.Y + Hitbox.Height / 2);

            UpdateHitbox();
        }

        #region Misc. Methods

        private void CheckIfFollowingLanko()
        {
            distanceFromLanko = Math.Abs(center.X - lanko.center.X);

            if (distanceFromLanko >= 50)
                State = GlubState.Following;

            else
                State = GlubState.Still;
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
                        numBounces++;
                    }

                    else if (State == GlubState.Falling)
                        numBouncesAfterFalling++;

                    Rectangle rect = Intersection(Hitbox, w.LocationRect);

                    if (w == border.Walls[0] || w == border.Walls[2])
                    {
                        if (state != GlubState.Following && state != GlubState.Still)
                        {
                            Direction.Y *= -1;

                            if (State == GlubState.Falling)
                            {
                                Direction.Y = Direction.Y / 1.5f;
                            }

                            if (rect.Top < center.Y)
                                Location.Y += (float)rect.Height;

                            else if (rect.Bottom > center.Y)
                                Location.Y -= (float)rect.Height;
                        }
                    }

                    else if (w == border.Walls[1] || w == border.Walls[3])
                    {
                        Direction.X *= -1;

                        if (rect.Right > center.X)
                            Location.X -= (float)rect.Width;

                        else if (rect.Left < center.X)
                            Location.X += (float)rect.Width;
                    }
                    UpdateHitbox();
                    return;
                }

            }
        }

        Vector2 SeekLanko()
        {
            Vector2 desiredDirection = (Vector2.Normalize(lanko.center - Location));
            return (desiredDirection);
        }

        //Called when Glub hits an item that he can damage
        public void ThrownToSeeking()
        {
            if (State == GlubState.Thrown)
            {
                State = GlubState.SeekingLanko;
                Speed = seekSpeed;
            }
        }

        private void UpdateHitbox()
        {
            int hitBoxWidthReduction = 8;
            int hitBoxHeightReduction = 3;

            locationRect.Location = Location.ToPoint();

            float scaledHeight = run_info.sourceFrame.Height * scale;
            float scaledWidth = run_info.sourceFrame.Width * scale;

            Hitbox = new Rectangle(LocationRect.X + hitBoxWidthReduction, LocationRect.Y + hitBoxHeightReduction,
                (int)scaledWidth - hitBoxWidthReduction*2, (int)scaledHeight - hitBoxHeightReduction*2);
        }

        public void GetCaughtByLanko()
        {
            numBounces = 0;
            cached_Location = new Vector2(LocationRect.X, LocationRect.Y);
            isAnimatingBurst = true;
            Speed = groundSpeed;
            Direction = new Vector2(0, 0);
            HasBounced = false;

            if (lanko.HasJumped)
                State = GlubState.Held;

            else
            {
                State = GlubState.AnimCoolDown;
                Location =
                    new Vector2(lanko.Location.X - 40, ground - locationRect.Height);
            }
        }

        public void CheckBuffTime(GameTime gameTime)
        {
            if ((gameTime.TotalGameTime.TotalMilliseconds / 1000) - timeOnBuffPickup > maxTimeAllowedBuffed)
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
            if(updatesBetweenFlicker > 0)
            {
                updatesBetweenFlicker--;
            }

            else
            {
                updatesBetweenFlicker = 2;

                if (color == Color.White)
                    color = Color.LightGreen;
                else
                    color = Color.White;
            }
        }

        private void SwapSpriteSheet(Texture2D spriteSheet, SpriteSheetInfo info)
        {
            spriteTexture = spriteSheet;

            run_info.currentFrame = 0;
            spriteSheetFramesWide = info.totalFrames;

            info.UpdateSourceFrame();
            SourceRectangle = info.sourceFrame;

            if (info == ball_info)
                run_Anim_Count = 0;
            else if (info == run_info)
                ball_Anim_Count = 0;

            current_info = info;

            UpdateHitbox();
        }

        private void CycleRunAnim()
        {
            if (run_Anim_Count >= updates_Between_Run)
            {
                run_info.currentFrame++;
                run_Anim_Count = 0;

                if (run_info.currentFrame > run_info.totalFrames - 1)
                    run_info.currentFrame = 0;

                run_info.UpdateSourceFrame();
                SourceRectangle = run_info.sourceFrame;
            }

            else
            {
                run_Anim_Count++;
                return;
            }
        }

        private void CycleBallAnim(bool IsCyclingForward)
        {
            if (IsCyclingForward)
            {
                if (ball_info.currentFrame == ball_info.totalFrames - 1)
                    return;

                if (ball_Anim_Count >= updates_Between_Ball)
                {
                    ball_info.currentFrame++;
                    ball_Anim_Count = 0;

                    if (ball_info.currentFrame > ball_info.totalFrames - 1)
                        ball_info.currentFrame = 0;

                    ball_info.UpdateSourceFrame();
                    SourceRectangle = ball_info.sourceFrame;
                }

                else
                {
                    ball_Anim_Count++;
                    return;
                }
            }

            else
            {
                if (ball_info.currentFrame == 0)
                {
                    SwapSpriteSheet(run_sheet, run_info);
                    ball_info.currentFrame = 0;
                    return;
                }

                if (ball_Anim_Count >= updates_Between_Ball)
                {
                    ball_info.currentFrame--;
                    ball_Anim_Count = 0;

                    if (ball_info.currentFrame > ball_info.totalFrames - 1)
                        ball_info.currentFrame = 0;

                    ball_info.UpdateSourceFrame();
                    SourceRectangle = ball_info.sourceFrame;
                }

                else
                {
                    ball_Anim_Count++;
                    return;
                }
            }
        }

        private void CycleBurstAnim()
        {
            if (burst_info.currentFrame == burst_info.totalFrames)
                return;

            if (burst_Anim_Count >= updates_Between_Ball)
            {
                burst_info.currentFrame++;
                burst_Anim_Count = 0;

                burst_info.UpdateSourceFrame();           
            }

            else
            {
                burst_Anim_Count++;
                return;
            }
        }

        private void CheckBurstAnimComplete()
        {
            if (burst_info.currentFrame == 5)
            {
                isAnimatingBurst = false;
                burst_info.currentFrame = 0;
                burst_Anim_Count = 0;
                burst_info.UpdateSourceFrame();
            }
        }

        #endregion

        #region Hacky Methods
        //HACK: Didn't want to deal with the screwy call orders. Called in Lanko's LoadContent()
        public void SetStartLocationAndGround()
        {
            ground = lanko.ground;

            Location =
                  new Vector2(lanko.Location.X - 40, ground - (SpriteTexture.Height * Scale));

            SetTranformAndRect();

            center = new Vector2(Location.X + Hitbox.Width / 2, Location.Y + Hitbox.Height / 2);
        }

        //HACK: Sometimes Glub gets out of the borders
        private void SaveGlubFromDeath()
        {
            if (center.X < border.Walls[3].LocationRect.Left ||
                center.X > border.Walls[1].LocationRect.Right ||
                center.Y < border.Walls[0].LocationRect.Top ||
                center.Y > border.Walls[2].LocationRect.Bottom)
            {
                GetCaughtByLanko();
                lanko.HasGlub = true;
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
                        Direction = lanko.aimDirection;
                    }

                    break;

                default: gcGlub.Log("BoolUpdate Error: Incorrect string input, check code.");
                    return;
            }
        }
        #endregion

    }
}
