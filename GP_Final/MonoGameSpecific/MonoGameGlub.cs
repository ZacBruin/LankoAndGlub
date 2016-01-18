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

        private Texture2D normalSprite, buffedSprite, run_sheet, ball_sheet, catch_burst_sheet;

        private SpriteSheetInfo run_info, ball_info, burst_info, current_info;

        public float ground, groundSpeed, distanceFromLanko, 
            airSpeed, seekSpeed, buffSpeed, maxTimeAllowedBuffed, timeOnBuffPickup, burst_scale;

        public Vector2 center;
        private Vector2 cached_Location;

        public bool HasBounced;
        private bool isAnimatingBurst;
        public int numBounces, numBouncesAfterFalling, maxBounces, maxBouncesAfterFalling,
            run_Anim_Count, updates_Between_Run, ball_Anim_Count, updates_Between_Ball, 
            burst_Anim_Count, updates_Between_Burst;

        protected GlubState state;
        public GlubState State
        {
            get {return this.state;}

            set
            {
                if (this.state != value)
                {
                    this.state = this.gcGlub.Pub_State = value;

                    if(this.state == GlubState.Still || this.state == GlubState.Following)
                    {                        
                        SwapSpriteSheet(this.run_sheet, this.run_info);
                        this.ball_info.currentFrame = 0;
                    }

                    else
                    {
                        SwapSpriteSheet(this.ball_sheet, this.ball_info);
                    }
                }
            }
        }

        private bool withLanko, hasStrongBuff;

        public bool WithLanko
        {
            get { return this.withLanko; }

            private set
            {
                if (this.withLanko != value)
                    this.withLanko = this.gcGlub.WithLanko = value;
            }
        }

        public bool HasStrongBuff
        {
            get { return this.hasStrongBuff; }
            set
            {
                if (this.hasStrongBuff != value)
                    this.hasStrongBuff = this.gcGlub.HasStrongBuff = value;

                if (value == true)
                {
                    //this.spriteTexture = this.buffedSprite;
                    this.airSpeed = this.buffSpeed;
                }

                else
                {
                    //this.spriteTexture = this.normalSprite;
                    this.airSpeed = 750;
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

            run_sheet = content.Load<Texture2D>("Glub_Run_SpriteSheet");
            run_info = new SpriteSheetInfo(4, run_sheet.Width, run_sheet.Height, updates_Between_Run);

            ball_sheet = content.Load<Texture2D>("Glub_BallAnim_SpriteSheet");
            ball_info = new SpriteSheetInfo(5, ball_sheet.Width, ball_sheet.Height, updates_Between_Ball);

            catch_burst_sheet = content.Load<Texture2D>("Glub_Burst_SpriteSheet");
            burst_info = new SpriteSheetInfo(5, catch_burst_sheet.Width, catch_burst_sheet.Height, updates_Between_Burst);

            spriteTexture = run_sheet;
            spriteSheetFramesWide = run_info.totalFrames;

            border = this.lanko.border;

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

            this.center = new Vector2(Location.X + this.Hitbox.Width / 2, Location.Y + this.Hitbox.Height / 2);

            WithLanko = true;
            HasStrongBuff = false;

            SetTranformAndRect();
            this.SourceRectangle = run_info.sourceFrame;

            base.LoadContent();
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();

            if(isAnimatingBurst)
            {
                spriteBatch.Draw(this.catch_burst_sheet, this.cached_Location, this.burst_info.sourceFrame, Color.White, 0f,
                    new Vector2(0,0), burst_scale, SpriteEffects.None, 0f);

                this.CycleBurstAnim();
                if(this.burst_info.currentFrame == 5)
                {
                    isAnimatingBurst = false;
                    burst_info.currentFrame = 0;
                    burst_Anim_Count = 0;
                    burst_info.UpdateSourceFrame();
                }
            }

            spriteBatch.End();
            base.Draw(gameTime);
        }

        public override void Update(GameTime gameTime)
        {
            if (!Lanko_And_Glub.utility.GamePaused)
            {
                this.UpdateGlub(gameTime.ElapsedGameTime.TotalMilliseconds / 1000);

                if (this.HasStrongBuff)
                    CheckBuffTime(gameTime);
            }

            base.Update(gameTime);
        }

        private void UpdateGlub(double timeElapsed)
        {
            SettleBorderCollision();

            #region State Logic Switch 

            switch (this.State)
            {
                case GlubState.Following:
                    if (this.center.X - this.lanko.center.X > 0)
                    {
                        this.Direction = new Vector2(-1, 0);
                        this.SpriteEffects = SpriteEffects.FlipHorizontally;
                    }

                    else if (this.center.X - this.lanko.center.X < 0)
                    {
                        this.Direction = new Vector2(1, 0);
                        this.SpriteEffects = SpriteEffects.None;
                    }

                    CheckIfFollowingLanko();
                    CycleRunAnim();
                    break;

                case GlubState.Still:
                    run_info.currentFrame = 0;
                    run_info.UpdateSourceFrame();
                    this.SourceRectangle = run_info.sourceFrame;
                    
                    this.Direction = new Vector2(0, 0);
                    CheckIfFollowingLanko();
                    break;

                case GlubState.Held:
                    this.Location = new Vector2(this.lanko.Location.X + 15, this.lanko.Location.Y - 15);

                    CycleBallAnim(true);

                    //Puts Glub back on the ground after Lanko lands (post catch)
                    if (this.lanko.IsAiming == false && this.lanko.HasJumped == false)
                    {
                        this.Location =
                            new Vector2(this.lanko.Location.X - 40, this.ground - (this.SpriteTexture.Height * this.Scale));

                        this.State = GlubState.AnimCoolDown;
                    }

                    break;

                case GlubState.Falling:
                    this.Direction.Y += .03f;

                    if (this.Direction.X != 0)
                    {
                        if (this.Direction.X < 0)
                            this.Direction.X += .05f;

                        if (this.Direction.X > 0)
                            this.Direction.X -= .05f;

                        if (this.Direction.X > -.05f && this.Direction.X < .05f)
                            this.Direction.X = 0;
                    }

                    this.Location += ((this.Direction) * this.Speed * (float)timeElapsed);

                    if (this.numBouncesAfterFalling >= this.maxBouncesAfterFalling)
                    {
                        this.State = GlubState.Stranded;
                        this.Location.Y = this.ground - (this.spriteTexture.Height * this.scale);
                        this.Direction = new Vector2(0, 0);
                        this.numBouncesAfterFalling = 0;
                    }

                    break;

                case GlubState.AnimCoolDown:
                    CycleBallAnim(false);

                    if (ball_info.currentFrame == 0)
                        this.State = GlubState.Still;

                    break;

                case GlubState.SeekingLanko:

                    CycleBallAnim(true);

                    if(this.lanko.HasGlub == false)
                        this.Direction = SeekLanko();

                    break;

                case GlubState.Thrown:
                        CycleBallAnim(true);

                    if (this.numBounces >= this.maxBounces)
                    {
                        this.State = GlubState.Falling;
                        this.Direction.Y = 0;
                        this.Direction.X = this.Direction.X / 500;
                        this.numBounces = 0;
                    }
                    break;

                case GlubState.Stranded:
                    CycleBallAnim(false);
                    break;
            }

            #endregion

            SaveGlubFromDeath();

            if(Math.Abs(this.Direction.Length()) > 0 && this.State != GlubState.Falling)
                this.Location += (Vector2.Normalize(this.Direction) * this.Speed * (float)timeElapsed);

            this.center = new Vector2(Location.X + this.Hitbox.Width / 2, Location.Y + this.Hitbox.Height / 2);

            UpdateHitbox();
        }

        #region Misc. Methods

        private void CheckIfFollowingLanko()
        {
            distanceFromLanko = Math.Abs(this.center.X - this.lanko.center.X);

            if (distanceFromLanko >= 50)
                this.State = GlubState.Following;

            else
                this.State = GlubState.Still;
        }

        private void SettleBorderCollision()
        {
            foreach (Wall w in this.border.Walls)
            {
                if (this.Hitbox.Intersects(w.LocationRect))
                {
                    if (this.State == GlubState.Thrown)
                    {
                        this.HasBounced = true;
                        this.numBounces++;
                    }

                    else if (this.State == GlubState.Falling)
                        this.numBouncesAfterFalling++;

                    Rectangle rect = this.Intersection(this.Hitbox, w.LocationRect);

                    if (w == this.border.Walls[0] || w == this.border.Walls[2])
                    {
                        if (this.state != GlubState.Following && this.state != GlubState.Still)
                        {
                            this.Direction.Y *= -1;

                            if (this.State == GlubState.Falling)
                            {
                                this.Direction.Y = this.Direction.Y / 1.5f;
                            }

                            if (rect.Top < this.center.Y)
                                this.Location.Y += (float)rect.Height;

                            else if (rect.Bottom > this.center.Y)
                                this.Location.Y -= (float)rect.Height;
                        }
                    }

                    else if (w == this.border.Walls[1] || w == this.border.Walls[3])
                    {
                        this.Direction.X *= -1;

                        if (rect.Right > this.center.X)
                            this.Location.X -= (float)rect.Width;

                        else if (rect.Left < this.center.X)
                            this.Location.X += (float)rect.Width;
                    }
                    UpdateHitbox();
                    return;
                }

            }
        }

        Vector2 SeekLanko()
        {
            Vector2 desiredDirection = (Vector2.Normalize(this.lanko.center - this.Location));
            return (desiredDirection);
        }

        //Called when Glub hits an item that he can damage
        public void ThrownToSeeking()
        {
            if (this.State == GlubState.Thrown)
            {
                this.State = GlubState.SeekingLanko;
                this.Speed = this.seekSpeed;
            }
        }

        private void UpdateHitbox()
        {
            int hitBoxWidthReduction = 8;
            int hitBoxHeightReduction = 3;

            this.locationRect.Location = this.Location.ToPoint();

            float scaledHeight = run_info.sourceFrame.Height * scale;
            float scaledWidth = run_info.sourceFrame.Width * scale;

            this.Hitbox = new Rectangle(this.LocationRect.X + hitBoxWidthReduction, this.LocationRect.Y + hitBoxHeightReduction,
                (int)scaledWidth - hitBoxWidthReduction*2, (int)scaledHeight - hitBoxHeightReduction*2);
        }

        public void GetCaughtByLanko()
        {
            this.numBounces = 0;
            this.cached_Location = new Vector2(this.LocationRect.X, this.LocationRect.Y);
            this.isAnimatingBurst = true;
            this.Speed = this.groundSpeed;
            this.Direction = new Vector2(0, 0);
            this.HasBounced = false;

            if (this.lanko.HasJumped)
                this.State = GlubState.Held;

            else
            {
                this.State = GlubState.AnimCoolDown;
                this.Location =
                    new Vector2(this.lanko.Location.X - 40, this.ground - this.locationRect.Height);
            }
        }

        public void CheckBuffTime(GameTime gameTime)
        {
            if ((gameTime.TotalGameTime.TotalMilliseconds / 1000) - this.timeOnBuffPickup > this.maxTimeAllowedBuffed)
                this.HasStrongBuff = false;
        }

        public void GetBuffed(GameTime gameTime)
        {
            this.HasStrongBuff = true;
            this.timeOnBuffPickup = (float)gameTime.TotalGameTime.TotalMilliseconds / 1000;
        }
        #endregion

        #region Animation Methods

        private void SwapSpriteSheet(Texture2D spriteSheet, SpriteSheetInfo info)
        {
            this.spriteTexture = spriteSheet;

            this.run_info.currentFrame = 0;
            this.spriteSheetFramesWide = info.totalFrames;

            info.UpdateSourceFrame();
            this.SourceRectangle = info.sourceFrame;

            if (info == this.ball_info)
                this.run_Anim_Count = 0;
            else if (info == this.run_info)
                this.ball_Anim_Count = 0;

            this.current_info = info;

            this.UpdateHitbox();
        }

        private void CycleRunAnim()
        {
            if (run_Anim_Count >= this.updates_Between_Run)
            {
                run_info.currentFrame++;
                run_Anim_Count = 0;

                if (run_info.currentFrame > run_info.totalFrames - 1)
                    run_info.currentFrame = 0;

                run_info.UpdateSourceFrame();
                this.SourceRectangle = run_info.sourceFrame;
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

                if (ball_Anim_Count >= this.updates_Between_Ball)
                {
                    ball_info.currentFrame++;
                    ball_Anim_Count = 0;

                    if (ball_info.currentFrame > ball_info.totalFrames - 1)
                        ball_info.currentFrame = 0;

                    ball_info.UpdateSourceFrame();
                    this.SourceRectangle = ball_info.sourceFrame;
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

                if (ball_Anim_Count >= this.updates_Between_Ball)
                {
                    ball_info.currentFrame--;
                    ball_Anim_Count = 0;

                    if (ball_info.currentFrame > ball_info.totalFrames - 1)
                        ball_info.currentFrame = 0;

                    ball_info.UpdateSourceFrame();
                    this.SourceRectangle = ball_info.sourceFrame;
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

            if (burst_Anim_Count >= this.updates_Between_Ball)
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

        #endregion

        #region Hacky Methods
        //HACK: Didn't want to deal with the screwy call orders. Called in Lanko's LoadContent()
        public void SetStartLocationAndGround()
        {
            this.ground = this.lanko.ground;
            //this.Location = new Vector2(300, this.ground - this.spriteTexture.Height * this.scale);
            this.Location =
                  new Vector2(this.lanko.Location.X - 40, this.ground - (this.SpriteTexture.Height * this.Scale));

            this.SetTranformAndRect();

            this.center = new Vector2(Location.X + this.Hitbox.Width / 2, Location.Y + this.Hitbox.Height / 2);
        }

        //HACK: Sometimes Glub gets out of the borders
        private void SaveGlubFromDeath()
        {
            if (this.center.X < this.border.Walls[3].LocationRect.Left ||
                this.center.X > this.border.Walls[1].LocationRect.Right ||
                this.center.Y < this.border.Walls[0].LocationRect.Top ||
                this.center.Y > this.border.Walls[2].LocationRect.Bottom)
            {
                GetCaughtByLanko();
                this.lanko.HasGlub = true;
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
                    if(b) this.State = GlubState.Held;
                    break;

                case "HasGlub":
                    if (b == false)
                    {
                        this.State = GlubState.Thrown;
                        this.HasBounced = false;
                        this.Speed = this.airSpeed;
                        this.Direction = this.lanko.aimDirection;
                    }

                    break;

                default: this.gcGlub.Log("BoolUpdate Error: Incorrect string input, check code.");
                    return;
            }
        }
        #endregion

    }
}
