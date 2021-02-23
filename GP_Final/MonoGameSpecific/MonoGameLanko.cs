using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;

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
        private SoundEffect blueGemGet, catchGlub, landOnGround;

        private int idleAnimationCount, runAnimationCount, 
            updates_Between_Idle, updates_Between_Run;

        private bool idleAnimCycled, canJumpAgain;

        SoundEffect ThrowSound;

        protected LankoState state;
        public LankoState State
        {
            get { return state; }

            set
            {
                if (state != value)
                {
                    state = gcLanko.Pub_State = value;

                    if (state == LankoState.Standing)
                    {
                        SwapSpriteSheet(idle_sheet, idle_Info);
                        Location.Y += Math.Abs(Hitbox.Bottom - border.Walls[2].LocationRect.Top);
                        UpdateHitbox();
                    }

                    else
                    {                        
                        SwapSpriteSheet(run_sheet, run_Info);
                    }
                }
            }
        }

        protected bool hasJumped, hasGlub, isAiming;

        public bool HasJumped
        {
            get { return hasJumped; }
            set
            {
                if (hasJumped != value)
                {
                    hasJumped = gcLanko.HasJumped = value;
                    if (value == true)
                    {
                        if (state == LankoState.Standing)
                            SwapSpriteSheet(run_sheet, run_Info);

                        run_Info.CurrentFrame = 2;
                        run_Info.UpdateSourceFrame();
                        SourceRectangle = run_Info.SourceFrame;
                    }

                    else if (state == LankoState.Standing)
                    {
                        SwapSpriteSheet(idle_sheet, idle_Info);
                    }

                }

            }
        }
      
        public bool HasGlub
        {
            get { return hasGlub; }
            set
            {
                if (hasGlub != value)
                {
                    hasGlub = gcLanko.HasGlub = value;
                    BoolNotify(HasGlub, "HasGlub");
                }
            }
        }

        public bool IsAiming
        {
            get { return isAiming; }
            set
            {
                if (isAiming != value)
                {
                    isAiming = gcLanko.IsAiming = value;
                    BoolNotify(IsAiming, "IsAiming");
                }
            }
        }

        public MonoGameLanko(Game game) : base (game)
        {
            gcLanko = new GameConsoleLanko((GameConsole)game.Services.GetService<IGameConsole>());
     
            border = new LevelBorder(game);
            game.Components.Add(border);

            glub = new MonoGameGlub(game);
            glub.lanko = this;
            game.Components.Add(glub);

            observers = new List<ILankoObserver>();
            Attach(glub);

            controller = new InputController(game);

            ThrowSound = content.Load<SoundEffect>("SFX/GlubThrow");
        }

        protected override void LoadContent()
        {
            idleAnimationCount = 0;

            updates_Between_Idle = 8;
            updates_Between_Run = 6;


            idle_sheet = content.Load<Texture2D>("SpriteSheets/LankoIdle");
            run_sheet = content.Load<Texture2D>("SpriteSheets/LankoRun");

            blueGemGet = content.Load<SoundEffect>("SFX/BlueGemGet");
            catchGlub = content.Load<SoundEffect>("SFX/CatchGlub");
            landOnGround = content.Load<SoundEffect>("SFX/Land");

            spriteTexture = idle_sheet;

            idle_Info = new SpriteSheetInfo(3, idle_sheet.Width, idle_sheet.Height, updates_Between_Idle);
            run_Info = new SpriteSheetInfo(5, run_sheet.Width, run_sheet.Height, updates_Between_Run);

            current_Info = idle_Info;

            spriteSheetFramesWide = idle_Info.TotalFrames;

            ground = border.Walls[2].LocationRect.Top;

            aim = content.Load<Texture2D>("Sprites/Aim");
            aimScale = .5f;        

            crossHair = content.Load<Texture2D>("Sprites/Crosshair");
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
                ground - Hitbox.Height);
           
            center = new Vector2(Location.X + Hitbox.Width / 2, Location.Y + Hitbox.Height / 2);

            aimDirection = controller.MouseDirection - center;
           
            HasJumped = false;
            HasGlub = true;
            idleAnimCycled = false;

            SetTranformAndRect();
            glub.SetStartLocationAndGround();          

            base.LoadContent();   
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();

            if (IsAiming)
            {
                spriteBatch.Draw(aim, glub.center, null, Color.MonoGameOrange, (float)Math.Atan2(aimDirection.Y, 
                    aimDirection.X) + (float)(Math.PI * .5f),
                    new Vector2(aim.Width / 2, aim.Bounds.Bottom), aimScale, SpriteEffects.None, 0f);
            }

            spriteBatch.Draw(crossHair, controller.MouseDirection, null, Color.White, 0f,
                new Vector2(crossHair.Width / 2, crossHair.Height / 2), crossHairScale, SpriteEffects.None, 0f);
                spriteBatch.End();

            base.Draw(gameTime);
        }

        public override void Update(GameTime gameTime)
        {
            controller.Update();

            if (!Lanko_And_Glub.utility.IsGamePaused)
            {
                UpdateLanko(gameTime.ElapsedGameTime.TotalMilliseconds / 1000);
                base.Update(gameTime);
            }
        }

        private void UpdateLanko(double timeElapsed)
        {
            LevelBorderCollision();

            switch (State)
            {
                case LankoState.Standing:
                    CycleIdleAnim();
                    break;

                case LankoState.Walking:
                    if (hasJumped == false)
                        CycleRunAnim();
                    break;
            }

            if(HasJumped == true)
                Direction += new Vector2(0, gravity);

            if (glub.HasBounced)
            {
                switch(glub.State)
                {
                    case GlubState.Thrown:
                    case GlubState.Falling:
                    case GlubState.Stranded:
                    case GlubState.SeekingLanko:
                    {
                        if (Hitbox.Intersects(glub.Hitbox))
                        {
                            glub.GetCaughtByLanko();                                
                            HasGlub = true;
                            catchGlub.Play(.4f, 0f, 0f);
                        }
                    }
                        break;

                    default:
                        break;
                }

            }

        #region Keyboard
         
        #region Walking
            if (controller.Keys.IsKeyUp(Keys.A) && controller.Keys.IsKeyUp(Keys.D))
            {
                if(HasJumped == false)
                {
                    State = LankoState.Standing;
                    Direction = new Vector2(0, 0);
                }

                else
                {
                    if (Direction.X > 0)
                        Direction.X -= .05f;
                    else if (Direction.X < 0)
                        Direction.X += .05f;
                }
            }

            else if(controller.Keys.IsKeyDown(Keys.A))
            {
                SpriteEffects = SpriteEffects.None;

                Direction.X = 0;

                if (HasJumped == false)
                    Direction.X -= 1;
                else
                    Direction.X -= .8f;
            }

            else if (controller.Keys.IsKeyDown(Keys.D))
            {
                SpriteEffects = SpriteEffects.FlipHorizontally;

                Direction.X = 0;

                if (HasJumped == false)
                    Direction.X += 1;
                else
                    Direction.X += .8f;
            }

        #endregion

            #region Jumping
            if (controller.Keys.IsKeyDown(Keys.Space))
            {
                if (HasJumped)
                {
                    if (Direction.Y <= 0)
                        Direction.Y -= .15f;
                }

                else if(!HasJumped && canJumpAgain)
                {
                    HasJumped = true;
                    Location.Y -= 5;
                    Direction.Y -= jumpForce;

                    idleAnimationCount = 0;
                    runAnimationCount = 0;
                }
            } 

            if (HasJumped == true && controller.Keys.IsKeyUp(Keys.Space))
            {

                if (Direction.Y >= 0)
                {
                    if (Direction.Y <= 1)
                        Direction.Y += .015f;

                    else
                        Direction.Y += .035f;
                }
                    
            }

            if (!HasJumped && controller.Keys.IsKeyUp(Keys.Space))
                canJumpAgain = true;

            #endregion
            #endregion

            #region Mouse

            if (controller.Mouse.LeftButton == ButtonState.Pressed && HasGlub)
            {
                switch (glub.State)
                {
                    case GlubState.Held:
                    case GlubState.Still:
                    case GlubState.AnimCoolDown:
                        IsAiming = true;
                        break;
                }
                 
                aimDirection = controller.MouseDirection - glub.center;
            }

            if (controller.Mouse.LeftButton == ButtonState.Released && IsAiming)
            {
                IsAiming = false;
                HasGlub = false;
                ThrowSound.Play(.3f, 0, 0);
            }

            #endregion

            if (Math.Abs(Direction.Length()) > 0)
            {
                idleAnimationCount = 0;

                if (HasJumped == false)
                {
                    State = LankoState.Walking;
                    Location += (Vector2.Normalize(Direction) * (Speed * speedMod) * (float)timeElapsed);
                }

                else
                {
                    //Clamps X direction
                    if (Direction.X > 1f)
                        Direction.X = 1f;

                    else if (Direction.X < -1f)
                        Direction.X = -1f;

                    Location += ((Direction) * (Speed * speedMod) * (float)timeElapsed);
                }

                center = new Vector2(Location.X + Hitbox.Width/2, Location.Y + Hitbox.Height/2);

                UpdateHitbox();
            }
        }

        public void IncreaseSpeedMod()
        {
            speedMod += .1f;
            blueGemGet.Play(.5f, 0f, 0f);
        }

        public void ResetSpeedMod()
        {
            speedMod = 1;
        }

        #region Animation Methods

        private void SwapSpriteSheet(Texture2D spriteSheet, SpriteSheetInfo info)
        {
            spriteTexture = spriteSheet;

            idle_Info.CurrentFrame = 0;
            spriteSheetFramesWide = info.TotalFrames;

            info.UpdateSourceFrame();
            SourceRectangle = info.SourceFrame;

            if (info == idle_Info)
                runAnimationCount = 0;
            else if (info == run_Info)
                idleAnimationCount = 0;

            current_Info = info;

            UpdateHitbox();
        }

        private void CycleRunAnim()
        {
            if (runAnimationCount >= updates_Between_Run)
            {
                run_Info.CurrentFrame++;
                runAnimationCount = 0;

                if (run_Info.CurrentFrame > run_Info.TotalFrames - 1)
                    run_Info.CurrentFrame = 0;

                run_Info.UpdateSourceFrame();
                SourceRectangle = run_Info.SourceFrame;
            }

            else
            {
                runAnimationCount++;
                return;
            }
        }

        private void CycleIdleAnim()
        {
            if(idleAnimationCount >= updates_Between_Idle)
            {
                switch (idle_Info.CurrentFrame)
                {
                    case 0:
                        idle_Info.CurrentFrame++;
                        idleAnimCycled = false;
                        break;

                    case 1:
                        if(idleAnimCycled)
                            idle_Info.CurrentFrame--;
                        else
                            idle_Info.CurrentFrame++;
                        break;

                    case 2:
                        idle_Info.CurrentFrame--;
                        idleAnimCycled = true;
                        break;
                }

                idleAnimationCount = 0;

                idle_Info.UpdateSourceFrame();
                SourceRectangle = idle_Info.SourceFrame;
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
            locationRect.Location = Location.ToPoint();

            float scaledHeight = current_Info.SourceFrame.Height * scale;
            float scaledWidth = current_Info.SourceFrame.Width * scale;

            Hitbox = new Rectangle(LocationRect.X, LocationRect.Y,
                (int)scaledWidth, (int)scaledHeight);
        }

        private void LevelBorderCollision()
        {
            foreach(Wall w in border.Walls)

            if(Hitbox.Intersects(w.LocationRect))
            {
                Rectangle rect = Intersection(Hitbox, w.LocationRect);
                if (rect.Height < rect.Width)
                {
                    if (rect.Top > center.Y)
                    {
                        if (HasJumped) { landOnGround.Play(.3f, 0, 0); }

                        Location.Y -= (float)rect.Height;
                        HasJumped = false;
                        Location.Y = ground - Hitbox.Height;
                        Direction.Y = 0;
                        

                        if(controller.Keys.IsKeyDown(Keys.Space))
                            canJumpAgain = false;
                    }

                    if (rect.Bottom < center.Y)
                        Location.Y += (float)rect.Height;
                }

                else if (rect.Height > rect.Width)
                {
                    if (rect.Right > center.X)
                    {
                        Location.X -= (float)rect.Width;
                    }

                    if (rect.Left < center.X)
                        Location.X += (float)rect.Width;
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
            observers.Add(observer);
        }

        public void Detach(ILankoObserver observer)
        {
            observers.Remove(observer);
        }

        public void Attach(IObserver observer)
        {

        }
        public void Detach(IObserver observer)
        {

        }

        public void BoolNotify(bool b, string s)
        {
            foreach (ILankoObserver o in observers)
            {
                o.Update(b, s);
            }
        }

        #endregion

    }
}
