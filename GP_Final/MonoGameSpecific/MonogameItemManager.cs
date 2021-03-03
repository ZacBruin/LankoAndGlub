﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GP_Final
{
    public sealed class MonogameItemManager : ItemManager
    {
        public LevelBorder Border;
        public MonoGameGlub Glub;
        public MonoGameLanko Lanko;
        public GameRound Round;

        private float timeTargetSpawned, timePowerUpSpawned;

        private List<Target> TargetsToDelete;
        private List<PowerUp> PowerUpsToDelete;
        private List<PointSprite> PointSpritesToDelete;

        private Random rand;

        private const float TIME_PER_TARGET_SPAWN = .65f;
        private const float TIME_PER_POWERUP_SPAWN = 2;
        private const int MAX_TARGETS = 12;
        private const int MAX_POWERUPS = 2;

        public MonogameItemManager(Game game) : base(game)
        {
            TargetsToDelete = new List<Target>();
            PowerUpsToDelete = new List<PowerUp>();
            PointSpritesToDelete = new List<PointSprite>();
            rand = new Random();
        }

        public override void Update(GameTime gameTime)
        {
            if (!Lanko_And_Glub.utility.IsGamePaused)
            {
                Color transparent = new Color(0, 0, 0, 0);

                foreach (PowerUp p in PowerUps)
                    if (p.color == transparent)
                        p.color = Color.White;

                foreach (Target t in Targets)
                    if (t.color == transparent)
                        t.color = Color.White;

                updateItemManager(gameTime);
                base.Update(gameTime);
            }

            else
            {
                foreach (PowerUp p in PowerUps)
                    p.color = new Color(0, 0, 0, 0);
                foreach (Target t in Targets)
                    t.color = new Color(0, 0, 0, 0);
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
                        if (p.SpawnAnimation())
                            p.state = Target.State.Moving;
                        break;

                    case Item.State.DeSpawning:
                        if (p.DespawnAnimation())
                            PowerUpsToDelete.Add(p);
                        break;
                }


                if (p.MaxTimeOnScreen < p.CurrentTimeOnScreen || Round.RoundIsOver)
                {
                    if (p.state == Item.State.Moving)
                        p.state = Item.State.DeSpawning;
                }

                else if (p is GreenGem)
                {
                    if (Glub.State == GlubState.Thrown && Glub.Hitbox.Intersects(p.Hitbox))
                    {
                        if (Glub.HasStrongBuff == false)
                            Glub.ThrownToSeeking();

                        if (p.CheckDamage())
                        {
                            Glub.GetBuffed(gameTime);
                            PowerUpsToDelete.Add(p);
                        }

                        Glub.HasBounced = true;
                    }
                }

                else if (p is CyanGem)
                {
                    if (Lanko.Hitbox.Intersects(p.Hitbox))
                    {
                        PowerUpsToDelete.Add(p);
                        Lanko.IncreaseSpeedMod();
                    }
                }

                checkCollision(p);
            }

            foreach (Target t in TargetsToDelete)
                Targets.Remove(t);

            foreach (PowerUp p in PowerUpsToDelete)
                PowerUps.Remove(p);

            foreach (PointSprite ps in PointSpritesToDelete)
                Points.Remove(ps);

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
                        if (t.SpawnAnimation())
                            t.state = Target.State.Moving;
                        break;

                    case Target.State.DeSpawning:
                        if (t.DespawnAnimation())
                            TargetsToDelete.Add(t);
                        break;

                    case Target.State.Dying:
                        if (t.DeathAnim())
                            TargetsToDelete.Add(t);
                        break;
                }

                if (t.MaxTimeOnScreen < t.CurrentTimeOnScreen || Round.RoundIsOver)
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
                    if (Glub.State == GlubState.Thrown && Glub.Hitbox.Intersects(t.Hitbox))
                    {
                        PointSprite ps;
                        switch (t.state)
                        {
                            case Target.State.Moving:
                            case Target.State.SpeedDown:
                            case Target.State.SpeedUp:
                            case Target.State.DeSpawning:
                                if(t is RedBat)
                                    ps = new PointSprite(Game, true);
                                else
                                    ps = new PointSprite(Game, false);

                                ps.Initialize();
                                ps.Location = new Vector2(t.LocationRect.X + 25, t.LocationRect.Y - 10);
                                ps.SetStartPos();

                                AddPointSprite(ps);

                                t.PlayHitSound();

                                Round.Points += t.PointValue;

                                if (Glub.HasStrongBuff == false)
                                    Glub.ThrownToSeeking();

                                Glub.HasBounced = true;
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
            foreach(PointSprite ps in Points)
            {
                ps.Update(gameTime);
                if (ps.color.A < 10) { PointSpritesToDelete.Add(ps); }
            }
        }

        private void updateItemManager(GameTime gameTime)
        {
            if (Round.RoundIsOver == false)
                generateItem(gameTime);

            else
            {
                Lanko.ResetSpeedMod();
                Glub.HasStrongBuff = false;
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

            if (i is RedBat)
            {
                i.Location = new Vector2(rand.Next((Border.Walls[3].LocationRect.Right + spawnEdgeBuffer),
                    (Border.Walls[1].LocationRect.Left - i.Hitbox.Width - spawnEdgeBuffer)),
                    rand.Next((Border.Walls[0].LocationRect.Bottom + spawnEdgeBuffer),
                    (Border.Walls[0].LocationRect.Bottom + 400)));
            }

            else if (i is GoldenBat)
            {
                i.Location = new Vector2(rand.Next((Border.Walls[3].LocationRect.Right + spawnEdgeBuffer),
                    (Border.Walls[1].LocationRect.Left - i.Hitbox.Width - spawnEdgeBuffer)),
                    rand.Next((Border.Walls[0].LocationRect.Bottom + spawnEdgeBuffer),
                    Border.Walls[0].LocationRect.Bottom + 150));
            }

            else if (i is CyanGem)
            {


                i.Location = new Vector2(rand.Next((Border.Walls[3].LocationRect.Right + spawnEdgeBuffer),
                    (Border.Walls[1].LocationRect.Left - i.Hitbox.Width - spawnEdgeBuffer)),
                     rand.Next((Border.Walls[0].LocationRect.Bottom + 300),
                     Border.Walls[2].LocationRect.Top - 50));
            }

            else if (i is GreenGem)
            {
                i.Location = new Vector2(rand.Next((Border.Walls[3].LocationRect.Right + spawnEdgeBuffer),
                    (Border.Walls[1].LocationRect.Left - i.Hitbox.Width - spawnEdgeBuffer)),
                    rand.Next((Border.Walls[0].LocationRect.Bottom + spawnEdgeBuffer),
                    Border.Walls[0].LocationRect.Bottom + 400));
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

            if(i.Hitbox.Intersects(Lanko.LocationRect) || i.Hitbox.Intersects(Glub.Hitbox))
                return true;

            return false;
        }

        private void spawnTarget(bool IsBasicTarget)
        {
            Target targ;

            if(IsBasicTarget) targ = new RedBat(Game);    
            else targ = new GoldenBat(Game);

            targ.Initialize();
            positionItem(targ);

            while (checkForOverlap(targ)) { positionItem(targ); }

            do
            {                
                targ.Direction.X = rand.Next(-100, 100);
                targ.Direction.Y = rand.Next(-20, 20);
            }
            while (Math.Abs(targ.Direction.Y) > Math.Abs(targ.Direction.X));

            if (targ.Direction.X > 0)
                targ.SpriteEffects = SpriteEffects.FlipHorizontally;

            AddTarget(targ);
        }

        private void spawnPowerUp(bool IsTriangle)
        {
            if (IsTriangle)
            {
                GreenGem tp = new GreenGem(Game);
                tp.Initialize();
                positionItem(tp);

                while (checkForOverlap(tp))
                {
                    positionItem(tp);
                }

                tp.Direction = new Vector2(rand.Next(-100, 100), rand.Next(-30, 30));
                AddPowerUp(tp);
            }

            else
            {
                CyanGem dp = new CyanGem(Game);
                dp.Initialize();
                positionItem(dp);

                while (checkForOverlap(dp))
                {
                    positionItem(dp);
                }

                AddPowerUp(dp);
            }
        }

        private void generateItem(GameTime gametime)
        {
            if (Targets.Count == 0 ||
                (gametime.TotalGameTime.TotalMilliseconds / 1000 - timeTargetSpawned > TIME_PER_TARGET_SPAWN))
            {
                if (Targets.Count < MAX_TARGETS)
                {
                    int odds = rand.Next(0, 50);

                    switch (odds)
                    {
                        case 1:
                        case 44:
                        case 19:
                            spawnTarget(true);
                            timeTargetSpawned = (float)gametime.TotalGameTime.TotalMilliseconds / 1000;
                            break;

                        case 14:
                            spawnTarget(false);
                            timeTargetSpawned = (float)gametime.TotalGameTime.TotalMilliseconds / 1000;
                            break;

                        default:
                            break;
                    }
                }
            }

            if (Round.CurrentRoundTime > 5)
            {
                if ((gametime.TotalGameTime.TotalMilliseconds / 1000 - timePowerUpSpawned > TIME_PER_POWERUP_SPAWN))
                {
                    if (PowerUps.Count < MAX_POWERUPS)
                    {
                        int odds = rand.Next(60, 90);

                        switch (odds)
                        {
                            case 62:
                            case 87:
                            case 84:
                                spawnPowerUp(false);
                                timePowerUpSpawned = (float)gametime.TotalGameTime.TotalMilliseconds / 1000;
                                break;

                            case 71:
                                spawnPowerUp(true);
                                timePowerUpSpawned = (float)gametime.TotalGameTime.TotalMilliseconds / 1000;
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
            foreach (Wall w in Border.Walls)
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

                if (i.Center.Y < rect.Bottom)
                    i.Location.Y -= (rect.Height + collisionSeparation);
                else if (i.Center.Y > rect.Top)
                    i.Location.Y += (rect.Height + collisionSeparation);
            }

            else if (rect.Height > rect.Width)
            {
                i.Direction.X *= -1;

                if (i.Center.X < rect.Left)
                    i.Location.X -= (rect.Width + collisionSeparation);
                else if (i.Center.X > rect.Right)
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
