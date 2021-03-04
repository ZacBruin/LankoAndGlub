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
        public LevelBorder Border;
        public MonoGameGlub Glub;

        public float Ground, SpeedBoostViaPowerUp;
        public Vector2 Center, AimDirection;

        internal InputController controller { get; private set; }
        internal GameConsoleLanko consoleLanko { get; private set; }

        private List<ILankoObserver> observers;

        private Texture2D 
            aimDots, 
            crossHair,
            idleSheet, 
            runSheet;

        private SpriteSheetInfo 
            idleInfo, 
            runInfo, 
            currentInfo;

        private SoundEffect 
            blueGemGet,
            catchGlub,
            landOnGround,
            throwGlub;

        private int
            idleAnimationUpdateCount,
            runAnimationUpdateCount;

        private bool idleAnimCycled, canJumpAgain;

        private LankoState state;
        public LankoState State
        {
            get { return state; }

            set
            {
                if (state != value)
                {
                    state = consoleLanko.Pub_State = value;

                    if (state == LankoState.Standing)
                    {
                        SwapSpriteSheet(idleSheet, idleInfo);
                        Location.Y += Math.Abs(Hitbox.Bottom - Border.BottomRect.Top);
                        UpdateHitbox();
                    }

                    else
                        SwapSpriteSheet(runSheet, runInfo);
                }
            }
        }

        private bool hasJumped;
        public bool HasJumped
        {
            get { return hasJumped; }
            set
            {
                if (hasJumped != value)
                {
                    hasJumped = consoleLanko.HasJumped = value;
                    if (value == true)
                    {
                        if (state == LankoState.Standing)
                            SwapSpriteSheet(runSheet, runInfo);

                        runInfo.CurrentFrame = 2;
                        runInfo.UpdateSourceFrame();
                        SourceRectangle = runInfo.SourceFrame;
                    }

                    else if (state == LankoState.Standing)
                    {
                        SwapSpriteSheet(idleSheet, idleInfo);
                    }

                }

            }
        }

        private bool hasGlub;
        public bool HasGlub
        {
            get { return hasGlub; }
            set
            {
                if (hasGlub != value)
                {
                    hasGlub = consoleLanko.HasGlub = value;
                    BoolNotify(HasGlub, "HasGlub");
                }
            }
        }

        private bool isAiming;
        public bool IsAiming
        {
            get { return isAiming; }
            set
            {
                if (isAiming != value)
                {
                    isAiming = consoleLanko.IsAiming = value;
                    BoolNotify(IsAiming, "IsAiming");
                }
            }
        }

        #region Consts
        //Asset Names
        const string THROW_GLUB_SFX = "SFX/GlubThrow";
        const string CYAN_GEM_GET_SFX = "SFX/BlueGemGet";
        const string CATCH_GLUB_SFX = "SFX/CatchGlub";
        const string LANKO_LAND_SFX = "SFX/Land";

        const string LANKO_IDLE_SPRITE_SHEET = "SpriteSheets/LankoIdle";
        const string LANKO_RUN_SPRITE_SHEET = "SpriteSheets/LankoRun";

        const string AIM_DOT_SPRITE = "Sprites/Aim";
        const string CROSSHAIR_SPRITE = "Sprites/Crosshair";

        //Numeric Values
        const float LANKO_SPRITE_SCALE = .28f;
        const float AIM_DOTS_SPRITE_SCALE = .5f;
        const float CROSSHAIR_SPRITE_SCALE = .45f;

        const float LANKO_BASE_SPEED = 300f;
        const float GRAVITY = .25f;
        const float LANKO_JUMP_FORCE = 2.8f;

        const float START_SPEED_MODIFIER = 1f;
        const float SPEED_BOOST_PER_POWERUP = .1f;

        const int IDLE_SHEET_FRAMES = 3;
        const int UPDATES_PER_IDLE_FRAME = 8;
        const int RUN_SHEET_FRAMES = 5;
        const int UPDATES_PER_RUN_FRAME = 6;

        //SFX Volumes
        const float CYAN_GEM_GET_SFX_VOL = .5f;
        const float CATCH_GLUB_SFX_VOL = .4f;
        const float THROW_GLUB_SFX_VOL = .3f;
        const float LANKO_LAND_SFX_VOL = .3f;

        //Controls
        const Keys MOVE_LEFT = Keys.A;
        const Keys MOVE_RIGHT = Keys.D;
        const Keys JUMP = Keys.Space;

        const MouseButton THROW_GLUB = MouseButton.Left;
        #endregion

        public MonoGameLanko(Game game) : base (game)
        {
            consoleLanko = new GameConsoleLanko((GameConsole)game.Services.GetService<IGameConsole>());
     
            Border = new LevelBorder(game);
            game.Components.Add(Border);

            Glub = new MonoGameGlub(game);
            Glub.lanko = this;
            game.Components.Add(Glub);

            observers = new List<ILankoObserver>();
            Attach(Glub);

            controller = new InputController(game);

            throwGlub = content.Load<SoundEffect>(THROW_GLUB_SFX);
        }

        protected override void LoadContent()
        {
            idleSheet = content.Load<Texture2D>(LANKO_IDLE_SPRITE_SHEET);
            idleInfo = new SpriteSheetInfo(IDLE_SHEET_FRAMES, idleSheet.Width, idleSheet.Height, UPDATES_PER_IDLE_FRAME);

            runSheet = content.Load<Texture2D>(LANKO_RUN_SPRITE_SHEET);
            runInfo = new SpriteSheetInfo(RUN_SHEET_FRAMES, runSheet.Width, runSheet.Height, UPDATES_PER_RUN_FRAME);

            aimDots = content.Load<Texture2D>(AIM_DOT_SPRITE);
            crossHair = content.Load<Texture2D>(CROSSHAIR_SPRITE);

            blueGemGet = content.Load<SoundEffect>(CYAN_GEM_GET_SFX);
            catchGlub = content.Load<SoundEffect>(CATCH_GLUB_SFX);
            landOnGround = content.Load<SoundEffect>(LANKO_LAND_SFX);

            idleAnimationUpdateCount = 0;
            spriteTexture = idleSheet;
            currentInfo = idleInfo;
            spriteSheetFramesWide = idleInfo.TotalFrames;

            Ground = Border.BottomRect.Top;

            Scale = LANKO_SPRITE_SCALE;
            UpdateHitbox();
        
            Speed = LANKO_BASE_SPEED;
            SpeedBoostViaPowerUp = START_SPEED_MODIFIER;
            Direction = Vector2.Zero;
            Location = new Vector2(Border.BottomRect.Width/2, Ground - Hitbox.Height);        
            Center = new Vector2(Location.X + Hitbox.Width / 2, Location.Y + Hitbox.Height / 2);
            AimDirection = controller.MouseDirection - Center;

            HasJumped = false;
            HasGlub = true;
            idleAnimCycled = false;
            canJumpAgain = true;

            SetTranformAndRect();
            Glub.SetStartLocationAndGround();

            base.LoadContent();   
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();

            if (IsAiming)
            {
                Vector2 aimDotsCenter = new Vector2(aimDots.Width / 2, aimDots.Bounds.Bottom);
                float aimDotsRotation = (float)Math.Atan2(AimDirection.Y, AimDirection.X) + (float)(Math.PI * .5f);

                spriteBatch.Draw(aimDots, Glub.center, null, Color.MonoGameOrange, aimDotsRotation, aimDotsCenter, AIM_DOTS_SPRITE_SCALE, SpriteEffects.None, 0f);
            }

            Vector2 crossHairCenter = new Vector2(crossHair.Width / 2, crossHair.Height / 2);
            spriteBatch.Draw(crossHair, controller.MouseDirection, null, Color.White, 0f, crossHairCenter, CROSSHAIR_SPRITE_SCALE, SpriteEffects.None, 0f);

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

            if (HasJumped == true)
                Direction += new Vector2(0, GRAVITY);

            if (Glub.HasBounced)
            {
                switch(Glub.State)
                {
                    case GlubState.Thrown:
                    case GlubState.Falling:
                    case GlubState.Stranded:
                    case GlubState.SeekingLanko:
                    {
                        if (Hitbox.Intersects(Glub.Hitbox))
                        {
                            Glub.GetCaughtByLanko();                                
                            HasGlub = true;
                            catchGlub.Play(CATCH_GLUB_SFX_VOL, 0f, 0f);
                        }
                        break;
                    }
                    default:
                        break;
                }
            }

            #region Keyboard        
            #region Walking
            if (controller.Keys.IsKeyUp(MOVE_LEFT) && controller.Keys.IsKeyUp(MOVE_RIGHT))
            {
                if(HasJumped == false)
                {
                    State = LankoState.Standing;
                    Direction = Vector2.Zero;
                }

                else
                {
                    if (Direction.X > 0)
                        Direction.X -= .05f;
                    else if (Direction.X < 0)
                        Direction.X += .05f;
                }
            }

            else if(controller.Keys.IsKeyDown(MOVE_LEFT))
            {
                SpriteEffects = SpriteEffects.None;

                Direction.X = 0;

                if (HasJumped == false)
                    Direction.X -= 1;
                else
                    Direction.X -= .8f;
            }

            else if (controller.Keys.IsKeyDown(MOVE_RIGHT))
            {
                SpriteEffects = SpriteEffects.FlipHorizontally;
                Direction.X = 0;
                Direction.X += (HasJumped == false) ? 1 : .8f;
            }
            #endregion

            #region Jumping
            if (controller.Keys.IsKeyDown(JUMP))
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
                    Direction.Y -= LANKO_JUMP_FORCE;

                    idleAnimationUpdateCount = 0;
                    runAnimationUpdateCount = 0;
                }
            } 

            if (HasJumped == true && controller.Keys.IsKeyUp(JUMP))
            {
                if (Direction.Y >= 0)
                    Direction.Y += (Direction.Y <= 1) ? .015f : .035f;                  
            }

            if (!HasJumped && controller.Keys.IsKeyUp(JUMP))
                canJumpAgain = true;

            #endregion
            #endregion

            #region Mouse
            if (controller.GetMouseButtonState(THROW_GLUB) == ButtonState.Pressed && HasGlub)
            {
                switch (Glub.State)
                {
                    case GlubState.Held:
                    case GlubState.Still:
                    case GlubState.AnimCoolDown:
                        IsAiming = true;
                        break;
                }
                 
                AimDirection = controller.MouseDirection - Glub.center;
            }

            if (controller.GetMouseButtonState(THROW_GLUB) == ButtonState.Released && IsAiming)
            {
                IsAiming = false;
                HasGlub = false;
                throwGlub.Play(THROW_GLUB_SFX_VOL, 0, 0);
            }
            #endregion

            if (Math.Abs(Direction.Length()) > 0)
            {
                idleAnimationUpdateCount = 0;

                if (HasJumped == false)
                {
                    State = LankoState.Walking;
                    Location += (Vector2.Normalize(Direction) * (Speed * SpeedBoostViaPowerUp) * (float)timeElapsed);
                }

                else
                {
                    //Clamps X direction
                    if (Direction.X > 1f)
                        Direction.X = 1f;

                    else if (Direction.X < -1f)
                        Direction.X = -1f;

                    Location += ((Direction) * (Speed * SpeedBoostViaPowerUp) * (float)timeElapsed);
                }

                Center = new Vector2(Location.X + Hitbox.Width / 2, Location.Y + Hitbox.Height / 2);
                UpdateHitbox();
            }
        }

        public void IncreaseSpeedMod()
        {
            SpeedBoostViaPowerUp += SPEED_BOOST_PER_POWERUP;
            blueGemGet.Play(CYAN_GEM_GET_SFX_VOL, 0f, 0f);
        }

        public void ResetSpeedMod()
        {
            SpeedBoostViaPowerUp = START_SPEED_MODIFIER;
        }

        #region Animation Methods
        private void SwapSpriteSheet(Texture2D spriteSheet, SpriteSheetInfo info)
        {
            spriteTexture = spriteSheet;

            idleInfo.CurrentFrame = 0;
            spriteSheetFramesWide = info.TotalFrames;

            info.UpdateSourceFrame();
            SourceRectangle = info.SourceFrame;

            if (info == idleInfo)
                runAnimationUpdateCount = 0;
            else if (info == runInfo)
                idleAnimationUpdateCount = 0;

            currentInfo = info;

            UpdateHitbox();
        }

        private void CycleRunAnim()
        {
            if (runAnimationUpdateCount >= UPDATES_PER_RUN_FRAME)
            {
                runInfo.CurrentFrame++;
                runAnimationUpdateCount = 0;

                if (runInfo.CurrentFrame > runInfo.TotalFrames - 1)
                    runInfo.CurrentFrame = 0;

                runInfo.UpdateSourceFrame();
                SourceRectangle = runInfo.SourceFrame;
            }

            else
            {
                runAnimationUpdateCount++;
                return;
            }
        }

        private void CycleIdleAnim()
        {
            if (idleAnimationUpdateCount >= UPDATES_PER_IDLE_FRAME)
            {
                switch (idleInfo.CurrentFrame)
                {
                    case 0:
                        idleInfo.CurrentFrame++;
                        idleAnimCycled = false;
                        break;

                    case 1:
                        if(idleAnimCycled)
                            idleInfo.CurrentFrame--;
                        else
                            idleInfo.CurrentFrame++;
                        break;

                    case 2:
                        idleInfo.CurrentFrame--;
                        idleAnimCycled = true;
                        break;
                }

                idleAnimationUpdateCount = 0;

                idleInfo.UpdateSourceFrame();
                SourceRectangle = idleInfo.SourceFrame;
            }

            else
                idleAnimationUpdateCount++;
        }
        #endregion

        private void UpdateHitbox()
        {
            locationRect.Location = Location.ToPoint();

            float scaledHeight = currentInfo.SourceFrame.Height * scale;
            float scaledWidth = currentInfo.SourceFrame.Width * scale;

            Hitbox = new Rectangle(LocationRect.X, LocationRect.Y, (int)scaledWidth, (int)scaledHeight);
        }

        private void LevelBorderCollision()
        {
            foreach(Wall w in Border.Walls)

            if(Hitbox.Intersects(w.LocationRect))
            {
                Rectangle rect = Intersection(Hitbox, w.LocationRect);
                if (rect.Height < rect.Width)
                {
                    if (rect.Top > Center.Y)
                    {
                        if (HasJumped)
                            landOnGround.Play(LANKO_LAND_SFX_VOL, 0, 0);

                        Location.Y -= rect.Height;
                        HasJumped = false;
                        Location.Y = Ground - Hitbox.Height;
                        Direction.Y = 0;
                        
                        if(controller.Keys.IsKeyDown(Keys.Space))
                            canJumpAgain = false;
                    }

                    if (rect.Bottom < Center.Y)
                        Location.Y += rect.Height;
                }

                else if (rect.Height > rect.Width)
                {
                    if (rect.Right > Center.X)
                        Location.X -= rect.Width;

                    if (rect.Left < Center.X)
                        Location.X += rect.Width;
                }

                UpdateHitbox();
            }
        }

        #region Observer Pattern Method
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
