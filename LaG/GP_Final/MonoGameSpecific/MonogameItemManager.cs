using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;

namespace GP_Final
{
    public sealed class MonogameItemManager : ItemManager
    {
        List<Target> TargetsToDelete;
        List<PowerUp> PowerUpsToDelete;
        List<PointSprite> PointSpritesToDelete;

        private Random rand;

        public LevelBorder border;
        public MonoGameGlub glub;
        public MonoGameLanko lanko;
        public GameRound round;

        public float TimeBetweenTargetSpawns, TimeTargetSpawned,
            TimeBetweenPowerUpSpawns, TimePowerUpSpawned;

        private int maxTargets, maxPowerUps;

        public MonogameItemManager(Game game) : base(game)
        {
            TargetsToDelete = new List<Target>();
            PowerUpsToDelete = new List<PowerUp>();
            PointSpritesToDelete = new List<PointSprite>();

            maxTargets = 12;
            maxPowerUps = 2;

            TimeBetweenTargetSpawns = .65f;
            TimeBetweenPowerUpSpawns = 5f;

            rand = new Random();
        }

        public override void Update(GameTime gameTime)
        {
            if (!Lanko_And_Glub.utility.GamePaused)
            {
                updateItemManager(gameTime);
                base.Update(gameTime);
            }
        }

        #region Updates
        private void updatePowerup(GameTime gameTime)
        {
            foreach (PowerUp p in PowerUps)
            {
                switch (p.state)
                {
                    case Item.State.Spawning:
                        if (p.SpawnInAnim())
                            p.state = Target.State.Moving;
                        break;

                    case Item.State.DeSpawning:
                        if (p.SpawnOutAnim())
                            PowerUpsToDelete.Add(p);
                        break;
                }


                if (p.MaxTimeOnScreen < p.CurrentTimeOnScreen || this.round.RoundIsOver)
                {
                    if (p.state == Item.State.Moving)
                        p.state = Item.State.DeSpawning;
                }

                else if (p is Green_PowerUp)
                {
                    if (this.glub.State == GlubState.Thrown && this.glub.Hitbox.Intersects(p.Hitbox))
                    {
                        if (this.glub.HasStrongBuff == false)
                            this.glub.ThrownToSeeking();

                        if (p.CheckDamage())
                        {
                            this.glub.GetBuffed(gameTime);
                            PowerUpsToDelete.Add(p);
                        }

                        this.glub.HasBounced = true;
                    }
                }

                else if (p is Cyan_PowerUp)
                {
                    if (this.lanko.Hitbox.Intersects(p.Hitbox))
                    {
                        PowerUpsToDelete.Add(p);
                        this.lanko.IncreaseSpeedMod();
                    }
                }

                checkCollision(p);
            }

            foreach (Target t in TargetsToDelete)
                this.Targets.Remove(t);

            foreach (PowerUp p in PowerUpsToDelete)
                this.PowerUps.Remove(p);

            foreach (PointSprite ps in PointSpritesToDelete)
                this.Points.Remove(ps);

            TargetsToDelete.Clear();
            PowerUpsToDelete.Clear();
            PointSpritesToDelete.Clear();
        }

        private void updateTarget(GameTime gameTime)
        {
            foreach (Target t in Targets)
            {
                switch (t.state)
                {
                    case Target.State.Spawning:
                        if (t.SpawnInAnim())
                            t.state = Target.State.Moving;
                        break;

                    case Target.State.DeSpawning:
                        if (t.SpawnOutAnim())
                            TargetsToDelete.Add(t);
                        break;

                    case Target.State.Dying:
                        if (t.DeathAnim())
                            TargetsToDelete.Add(t);
                        break;
                }

                if (t.MaxTimeOnScreen < t.CurrentTimeOnScreen || this.round.RoundIsOver)
                {
                    switch (t.state)
                    {
                        case Target.State.Moving:
                        case Target.State.SpeedDown:
                        case Target.State.SpeedUp:
                            t.state = Target.State.DeSpawning;
                            break;
                    }
                }

                else
                {
                    if (this.glub.State == GlubState.Thrown && this.glub.Hitbox.Intersects(t.Hitbox))
                    {
                        PointSprite ps;
                        switch (t.state)
                        {
                            case Target.State.Moving:
                            case Target.State.SpeedDown:
                            case Target.State.SpeedUp:
                            case Target.State.DeSpawning:
                                if(t is Basic_Target) { ps = new PointSprite(this.Game, true); }
                                else { ps = new PointSprite(this.Game, false); }

                                ps.Initialize();
                                ps.Location = new Vector2(t.LocationRect.X + 25, t.LocationRect.Y - 10);
                                ps.SetStartPos();

                                this.AddPointSprite(ps);

                                t.PlayHitSound();

                                this.round.Points += t.pointValue;

                                if (this.glub.HasStrongBuff == false)
                                    this.glub.ThrownToSeeking();

                                this.glub.HasBounced = true;
                                t.state = Target.State.Dying;
                                t.Speed = 10;
                                t.color = new Color(230, 230, 230, 230);
                                break;

                            default:
                                break;
                        }
                    }
                }

                checkCollision(t);
            }
        }

        private void updatePointSprites(GameTime gameTime)
        {
            foreach(PointSprite ps in this.Points)
            {
                ps.Update(gameTime);
                if (ps.color.A < 10) { this.PointSpritesToDelete.Add(ps); }
            }
        }

        private void updateItemManager(GameTime gameTime)
        {
            if (this.round.RoundIsOver == false)
                this.generateItem(gameTime);

            else
            {
                this.lanko.ResetSpeedMod();
                this.glub.HasStrongBuff = false;
            }

            updatePowerup(gameTime);
            updateTarget(gameTime);
            updatePointSprites(gameTime);
        }
        #endregion

        #region Item Spawning
        private void positionItem(Item i)
        {
            int spawnEdgeBuffer = 5;

            if (i is Basic_Target)
            {
                i.Location = new Vector2(rand.Next((int)(this.border.Walls[3].LocationRect.Right + spawnEdgeBuffer),
                    ((int)this.border.Walls[1].LocationRect.Left - i.Hitbox.Width - spawnEdgeBuffer)),
                    rand.Next((int)(this.border.Walls[0].LocationRect.Bottom + spawnEdgeBuffer),
                    (int)(this.border.Walls[0].LocationRect.Bottom + 400)));
            }

            else if (i is Golden_Target)
            {
                i.Location = new Vector2(rand.Next((this.border.Walls[3].LocationRect.Right + spawnEdgeBuffer),
                    (this.border.Walls[1].LocationRect.Left - i.Hitbox.Width - spawnEdgeBuffer)),
                    rand.Next((this.border.Walls[0].LocationRect.Bottom + spawnEdgeBuffer),
                    this.border.Walls[0].LocationRect.Bottom + 150));
            }

            else if (i is Cyan_PowerUp)
            {
                i.Location = new Vector2(rand.Next((this.border.Walls[3].LocationRect.Right + spawnEdgeBuffer),
                    (this.border.Walls[1].LocationRect.Left - i.Hitbox.Width - spawnEdgeBuffer)),
                     rand.Next((this.border.Walls[0].LocationRect.Bottom + 300),
                     this.border.Walls[2].LocationRect.Top - 50));
            }

            else if (i is Green_PowerUp)
            {
                i.Location = new Vector2(rand.Next((this.border.Walls[3].LocationRect.Right + spawnEdgeBuffer),
                    (this.border.Walls[1].LocationRect.Left - i.Hitbox.Width - spawnEdgeBuffer)),
                    rand.Next((this.border.Walls[0].LocationRect.Bottom + spawnEdgeBuffer),
                    this.border.Walls[0].LocationRect.Bottom + 400));
            }

            i.SetTranformAndRect();
            i.UpdateHitbox();
        }       

        private bool checkForOverlap(Item i)
        {
            foreach (Target targ in Targets)
            {
                if(i.Hitbox.Intersects(targ.Hitbox))
                    return true;
            }

            foreach (PowerUp pow in PowerUps)
            {
                if(i.Hitbox.Intersects(pow.Hitbox))
                    return true;
            }

            if(i.Hitbox.Intersects(this.lanko.LocationRect) || i.Hitbox.Intersects(this.glub.Hitbox))
                return true;

            return false;
        }

        private void spawnTarget(bool IsBasicTarget)
        {
            Target targ;

            if(IsBasicTarget) targ = new Basic_Target(this.Game);    
            else targ = new Golden_Target(this.Game);

            targ.Initialize();
            this.positionItem(targ);

            while (checkForOverlap(targ)) { this.positionItem(targ); }

            do
            {                
                targ.Direction.X = rand.Next(-100, 100);
                targ.Direction.Y = rand.Next(-20, 20);
            }
            while (Math.Abs(targ.Direction.Y) > Math.Abs(targ.Direction.X));

            if (targ.Direction.X > 0)
                targ.SpriteEffects = SpriteEffects.FlipHorizontally;

            this.AddTarget(targ);
        }

        private void spawnPowerUp(bool IsTriangle)
        {
            if (IsTriangle)
            {
                Green_PowerUp tp = new Green_PowerUp(this.Game);
                tp.Initialize();
                this.positionItem(tp);

                while (checkForOverlap(tp))
                {
                    this.positionItem(tp);
                }

                tp.Direction = new Vector2(rand.Next(-100, 100), rand.Next(-30, 30));
                this.AddPowerUp(tp);
            }

            else
            {
                Cyan_PowerUp dp = new Cyan_PowerUp(this.Game);
                dp.Initialize();
                this.positionItem(dp);

                while (checkForOverlap(dp))
                {
                    this.positionItem(dp);
                }

                this.AddPowerUp(dp);
            }
        }

        private void generateItem(GameTime gametime)
        {
            if (this.Targets.Count == 0 ||
                (gametime.TotalGameTime.TotalMilliseconds / 1000 - this.TimeTargetSpawned > this.TimeBetweenTargetSpawns))
            {

                if (this.Targets.Count < maxTargets)
                {
                    int odds = rand.Next(0, 50);

                    switch (odds)
                    {
                        case 1:
                        case 44:
                        case 19:
                            this.spawnTarget(true);
                            this.TimeTargetSpawned = (float)gametime.TotalGameTime.TotalMilliseconds / 1000;
                            break;

                        case 14:
                            this.spawnTarget(false);
                            this.TimeTargetSpawned = (float)gametime.TotalGameTime.TotalMilliseconds / 1000;
                            break;

                        default:
                            break;
                    }
                }
            }

            if (this.round.CurrentRoundTime > 5)
            {
                if ((gametime.TotalGameTime.TotalMilliseconds / 1000 - this.TimePowerUpSpawned > this.TimeBetweenPowerUpSpawns))
                {
                    if (this.PowerUps.Count < maxPowerUps)
                    {
                        int odds = rand.Next(60, 90);

                        switch (odds)
                        {
                            case 62:
                            case 87:
                            case 84:
                                this.spawnPowerUp(false);
                                this.TimePowerUpSpawned = (float)gametime.TotalGameTime.TotalMilliseconds / 1000;
                                break;

                            case 71:
                                this.spawnPowerUp(true);
                                this.TimePowerUpSpawned = (float)gametime.TotalGameTime.TotalMilliseconds / 1000;
                                break;

                            default:
                                break;
                        }
                    }
                }
            }

        }
        #endregion

        #region "Physics"
        private void checkCollision(Item i)
        {
            foreach (Wall w in this.border.Walls)
            {
                if (i.Hitbox.Intersects(w.LocationRect))
                {
                    settleBounce(i, w, 0);
                    return;
                }
            }
        }

        private void settleBounce(Item i, Sprite o, int collisionSeparation)
        {
            Rectangle rect = i.Intersection(i.Hitbox, o.LocationRect);

            if (rect.Height < rect.Width)
            {
                i.Direction.Y *= -1;

                if (i.center.Y < rect.Bottom)
                    i.Location.Y -= (rect.Height + collisionSeparation);
                else if (i.center.Y > rect.Top)
                    i.Location.Y += (rect.Height + collisionSeparation);
            }

            else if (rect.Height > rect.Width)
            {
                i.Direction.X *= -1;

                if (i.center.X < rect.Left)
                    i.Location.X -= (rect.Width + collisionSeparation);
                else if (i.center.X > rect.Right)
                    i.Location.X += (rect.Width + collisionSeparation);

                if (i is Target)
                {
                    if (i.SpriteEffects == SpriteEffects.None)
                        i.SpriteEffects = SpriteEffects.FlipHorizontally;
                    else
                        i.SpriteEffects = SpriteEffects.None;
                }
            }

            i.SetTranformAndRect();
        }
        #endregion
    }
}
