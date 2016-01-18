using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GP_Final
{
    public sealed class MonoGameLanko : DrawableSprite, ISubjectLanko
    {
        internal InputController controller { get; private set; }
        internal GameConsoleLanko gcLanko { get; private set; }
        public LevelBorder border;
        public MonoGameGlub glub;

        private List<ILankoObserver> observers;

        public Texture2D aim, crossHair;

        private Texture2D idle_sheet, run_sheet;
        private SpriteSheetInfo idle_Info, run_Info, current_Info;

        private float aimScale, crossHairScale, gravity, jumpForce;
        public float ground, speedMod;

        public Vector2 center, aimDirection;

        private int idleAnimationCount, runAnimationCount, 
            updates_Between_Idle, updates_Between_Run;

        private bool idleAnimCycled, spaceReleased, canJumpAgain;

        protected LankoState state;
        public LankoState State
        {
            get { return this.state; }

            set
            {
                if (this.state != value)
                {
                    this.state = this.gcLanko.Pub_State = value;

                    if (this.state == LankoState.Standing)
                    {
                        this.SwapSpriteSheet(idle_sheet, idle_Info);
                        this.Location.Y += Math.Abs(Hitbox.Bottom - this.border.Walls[2].LocationRect.Top);
                        this.UpdateHitbox();
                    }

                    else
                    {                        
                        this.SwapSpriteSheet(run_sheet, run_Info);
                    }
                }
            }
        }

        protected bool hasJumped, hasGlub, isAiming;

        public bool HasJumped
        {
            get { return this.hasJumped; }
            set
            {
                if (this.hasJumped != value)
                {
                    this.hasJumped = this.gcLanko.HasJumped = value;
                    if (value == true)
                    {
                        if (this.state == LankoState.Standing)
                            this.SwapSpriteSheet(run_sheet, run_Info);

                        run_Info.currentFrame = 2;
                        run_Info.UpdateSourceFrame();
                        this.SourceRectangle = run_Info.sourceFrame;
                    }

                    else if (this.state == LankoState.Standing)
                    {
                        spaceReleased = false;
                        SwapSpriteSheet(idle_sheet, idle_Info);
                    }

                }

            }
        }
      
        public bool HasGlub
        {
            get { return this.hasGlub; }
            set
            {
                if (this.hasGlub != value)
                {
                    this.hasGlub = this.gcLanko.HasGlub = value;
                    this.BoolNotify(HasGlub, "HasGlub");
                }
            }
        }

        public bool IsAiming
        {
            get { return this.isAiming; }
            set
            {
                if (this.isAiming != value)
                {
                    this.isAiming = this.gcLanko.IsAiming = value;
                    this.BoolNotify(this.IsAiming, "IsAiming");
                }
            }
        }

        public MonoGameLanko(Game game) : base (game)
        {
            gcLanko = new GameConsoleLanko((GameConsole)game.Services.GetService<IGameConsole>());
     
            this.border = new LevelBorder(game);
            game.Components.Add(border);

            this.glub = new MonoGameGlub(game);
            this.glub.lanko = this;
            game.Components.Add(glub);

            this.observers = new List<ILankoObserver>();
            this.Attach(glub);

            this.controller = new InputController(game);
        }

        protected override void LoadContent()
        {
            idleAnimationCount = 0;

            updates_Between_Idle = 8;
            updates_Between_Run = 6;


            idle_sheet = content.Load<Texture2D>("Lanko_Idle_SpriteSheet");
            run_sheet = content.Load<Texture2D>("Lanko_Run_SpriteSheet");

            spriteTexture = idle_sheet;

            idle_Info = new SpriteSheetInfo(3, idle_sheet.Width, idle_sheet.Height, updates_Between_Idle);
            run_Info = new SpriteSheetInfo(5, run_sheet.Width, run_sheet.Height, updates_Between_Run);

            current_Info = idle_Info;

            spriteSheetFramesWide = idle_Info.totalFrames;

            ground = this.border.Walls[2].LocationRect.Top;

            aim = content.Load<Texture2D>("Aim");
            aimScale = .5f;        

            crossHair = content.Load<Texture2D>("Crosshair");
            crossHairScale = .45f;            
                       
            Scale = .28f;
            Speed = 300f;
            speedMod = 1;

            gravity = .25f;
            jumpForce = 2.8f;

            canJumpAgain = true;

            UpdateHitbox();

            Direction = new Vector2(0, 0);
            Location = new Vector2(border.Walls[2].LocationRect.Width/2, 
                this.ground - this.Hitbox.Height);
           
            this.center = new Vector2(Location.X + this.Hitbox.Width / 2, Location.Y + this.Hitbox.Height / 2);

            aimDirection = this.controller.mouseDirection - this.center;
           
            HasJumped = false;
            HasGlub = true;
            idleAnimCycled = false;
            spaceReleased = false;

            SetTranformAndRect();
            glub.SetStartLocationAndGround();          

            base.LoadContent();   
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();

            if (this.IsAiming)
            {
                spriteBatch.Draw(aim, this.glub.center, null, Color.MonoGameOrange, (float)Math.Atan2(this.aimDirection.Y, 
                    this.aimDirection.X) + (float)(Math.PI * .5f),
                    new Vector2(aim.Width / 2, aim.Bounds.Bottom), aimScale, SpriteEffects.None, 0f);
            }

            spriteBatch.Draw(crossHair, this.controller.mouseDirection, null, Color.White, 0f,
                new Vector2(crossHair.Width / 2, crossHair.Height / 2), crossHairScale, SpriteEffects.None, 0f);
                spriteBatch.End();

            base.Draw(gameTime);
        }

        public override void Update(GameTime gameTime)
        {
            this.controller.Update();

            if (!Lanko_And_Glub.utility.GamePaused)
            {
                this.UpdateLanko(gameTime.ElapsedGameTime.TotalMilliseconds / 1000);
                base.Update(gameTime);
            }
        }

        private void UpdateLanko(double timeElapsed)
        {
            LevelBorderCollision();

            switch (this.State)
            {
                case LankoState.Standing:
                    CycleIdleAnim();
                    break;

                case LankoState.Walking:
                    if (hasJumped == false)
                        CycleRunAnim();
                    break;
            }

            if(this.HasJumped == true)
                this.Direction += new Vector2(0, gravity);

            if (this.glub.HasBounced)
            {
                switch(this.glub.State)
                {
                    case GlubState.Thrown:
                    case GlubState.Falling:
                    case GlubState.Stranded:
                    case GlubState.SeekingLanko:
                        {
                            if (this.Hitbox.Intersects(this.glub.Hitbox))
                            {
                                this.glub.GetCaughtByLanko();                                
                                this.HasGlub = true;
                            }
                        }
                        break;

                    default:
                        break;
                }

            }

            #region Keyboard
         
            #region Walking
            if (this.controller.keys.IsKeyUp(Keys.A) && this.controller.keys.IsKeyUp(Keys.D))
            {
                if(this.HasJumped == false)
                {
                    this.State = LankoState.Standing;
                    this.Direction = new Vector2(0, 0);
                }

                else
                {
                    if (this.Direction.X > 0)
                        this.Direction.X -= .05f;
                    else if (this.Direction.X < 0)
                        this.Direction.X += .05f;
                }
            }

            else if(controller.keys.IsKeyDown(Keys.A))
            {
                this.SpriteEffects = SpriteEffects.None;

                this.Direction.X = 0;

                if (this.HasJumped == false)
                    this.Direction.X -= 1;
                else
                    this.Direction.X -= .8f;
            }

            else if (controller.keys.IsKeyDown(Keys.D))
            {
                this.SpriteEffects = SpriteEffects.FlipHorizontally;

                this.Direction.X = 0;

                if (this.HasJumped == false)
                    this.Direction.X += 1;
                else
                    this.Direction.X += .8f;
            }

#endregion

            #region Jumping
            if (controller.keys.IsKeyDown(Keys.Space))
            {
                if (this.HasJumped)
                {
                    if (this.Direction.Y <= 0)
                        this.Direction.Y -= .15f;
                }

                else if(!this.HasJumped && canJumpAgain)
                {
                    this.HasJumped = true;
                    this.Location.Y -= 5;
                    this.Direction.Y -= jumpForce;

                    idleAnimationCount = 0;
                    runAnimationCount = 0;
                }
            } 

            if (this.HasJumped == true && controller.keys.IsKeyUp(Keys.Space))
            {
                spaceReleased = true;

                if (this.Direction.Y >= 0)
                {
                    if (this.Direction.Y <= 1)
                        this.Direction.Y += .015f;

                    else
                        this.Direction.Y += .035f;
                }
                    
            }

            if (!this.HasJumped && controller.keys.IsKeyUp(Keys.Space))
                canJumpAgain = true;

            #endregion

            #endregion

            #region Mouse

            if (this.controller.mouse.LeftButton == ButtonState.Pressed && this.HasGlub)
            {
                switch (this.glub.State)
                {
                    case GlubState.Held:
                    case GlubState.Still:
                    case GlubState.AnimCoolDown:
                        this.IsAiming = true;
                        break;
                }
                 
                this.aimDirection = this.controller.mouseDirection - this.glub.center;
            }

            if (this.controller.mouse.LeftButton == ButtonState.Released && this.IsAiming)
            {
                this.IsAiming = false;
                this.HasGlub = false;
            }

            #endregion

            if (Math.Abs(this.Direction.Length()) > 0)
            {
                idleAnimationCount = 0;

                if (this.HasJumped == false)
                {
                    this.State = LankoState.Walking;
                    this.Location += (Vector2.Normalize(this.Direction) * (this.Speed * this.speedMod) * (float)timeElapsed);
                }

                else
                {
                    //Clamps X direction
                    if (this.Direction.X > 1f)
                        this.Direction.X = 1f;

                    else if (this.Direction.X < -1f)
                        this.Direction.X = -1f;

                    this.Location += ((this.Direction) * (this.Speed * this.speedMod) * (float)timeElapsed);
                }

                this.center = new Vector2(Location.X + this.Hitbox.Width/2, Location.Y + this.Hitbox.Height/2);

                UpdateHitbox();
            }
        }

        public void IncreaseSpeedMod()
        {
            this.speedMod += .1f;
        }

        public void ResetSpeedMod()
        {
            this.speedMod = 1;
        }

        #region Animation Methods

        private void SwapSpriteSheet(Texture2D spriteSheet, SpriteSheetInfo info)
        {
            this.spriteTexture = spriteSheet;

            this.idle_Info.currentFrame = 0;
            this.spriteSheetFramesWide = info.totalFrames;

            info.UpdateSourceFrame();
            this.SourceRectangle = info.sourceFrame;

            if (info == this.idle_Info)
                this.runAnimationCount = 0;
            else if (info == this.run_Info)
                this.idleAnimationCount = 0;

            this.current_Info = info;

            UpdateHitbox();
        }

        private void CycleRunAnim()
        {
            if (runAnimationCount >= this.updates_Between_Run)
            {
                run_Info.currentFrame++;
                runAnimationCount = 0;

                if (run_Info.currentFrame > run_Info.totalFrames - 1)
                    run_Info.currentFrame = 0;

                run_Info.UpdateSourceFrame();
                this.SourceRectangle = run_Info.sourceFrame;
            }

            else
            {
                runAnimationCount++;
                return;
            }
        }

        private void CycleIdleAnim()
        {
            if(idleAnimationCount >= this.updates_Between_Idle)
            {
                switch (idle_Info.currentFrame)
                {
                    case 0:
                        idle_Info.currentFrame++;
                        idleAnimCycled = false;
                        break;

                    case 1:
                        if(idleAnimCycled)
                            idle_Info.currentFrame--;
                        else
                            idle_Info.currentFrame++;
                        break;

                    case 2:
                        idle_Info.currentFrame--;
                        idleAnimCycled = true;
                        break;
                }

                idleAnimationCount = 0;

                idle_Info.UpdateSourceFrame();
                this.SourceRectangle = idle_Info.sourceFrame;
            }

            else
            {
                idleAnimationCount++;
                return;
            }
        }

        #endregion

        private void UpdateHitbox()
        {
            this.locationRect.Location = this.Location.ToPoint();

            float scaledHeight = current_Info.sourceFrame.Height * scale;
            float scaledWidth = current_Info.sourceFrame.Width * scale;

            this.Hitbox = new Rectangle(this.LocationRect.X, this.LocationRect.Y,
                (int)scaledWidth, (int)scaledHeight);
        }

        private void LevelBorderCollision()
        {
            foreach(Wall w in this.border.Walls)

            if(this.Hitbox.Intersects(w.LocationRect))
            {
                Rectangle rect = this.Intersection(this.Hitbox, w.LocationRect);
                if (rect.Height < rect.Width)
                {
                    if (rect.Top > this.center.Y)
                    {
                        this.Location.Y -= (float)rect.Height;
                        this.HasJumped = false;
                        this.Location.Y = this.ground - this.Hitbox.Height;
                        this.Direction.Y = 0;

                        if(controller.keys.IsKeyDown(Keys.Space))
                            canJumpAgain = false;
                    }

                    if (rect.Bottom < this.center.Y)
                        this.Location.Y += (float)rect.Height;
                }

                else if (rect.Height > rect.Width)
                {
                    if (rect.Right > this.center.X)
                    {
                        this.Location.X -= (float)rect.Width;
                    }

                    if (rect.Left < this.center.X)
                        this.Location.X += (float)rect.Width;
                }

                UpdateHitbox();
            }
        }

        #region Observer Pattern Methods

        public List<ILankoObserver> Observers
        {
            get { return observers; }
            set { observers = value; }
        }

        public void Attach(ILankoObserver observer)
        {
            this.observers.Add(observer);
        }

        public void Detach(ILankoObserver observer)
        {
            this.observers.Remove(observer);
        }

        public void Attach(IObserver observer)
        {

        }
        public void Detach(IObserver observer)
        {

        }

        public void BoolNotify(bool b, string s)
        {
            foreach (ILankoObserver o in this.observers)
            {
                o.Update(b, s);
            }
        }

        #endregion

    }
}
