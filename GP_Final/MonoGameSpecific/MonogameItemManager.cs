using System;
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

        private enum EnemyType
        {
            RedBat,
            GoldBat
        }

        private enum PowerUpType
        {
            CyanGem,
            GreenGem
        }

        private float timeTargetSpawned, timePowerUpSpawned;

        private List<Target> TargetsToDelete;
        private List<PowerUp> PowerUpsToDelete;
        private List<PointSprite> PointSpritesToDelete;

        private Random rand;

        #region Consts
        private const float TIME_PER_TARGET_SPAWN = .65f;
        private const float TIME_PER_POWERUP_SPAWN = 2;
        private const int MAX_TARGETS = 12;
        private const int MAX_POWERUPS = 2;
        private const float ENEMY_DEATH_SPEED = 10;

        private const int ITEM_MIN_X_DIRECTION = -100;
        private const int ITEM_MAX_X_DIRECTION = 100;

        private const int ENEMY_MIN_Y_DIRECTION = -20;
        private const int ENEMY_MAX_Y_DIRECTION = 20;

        private const int GREENGEM_MIN_Y_DIRECTION = -30;
        private const int GREENGEM_MAX_Y_DIRECTION = 30;
        #endregion

        public MonogameItemManager(Game game) : base(game)
        {
            TargetsToDelete = new List<Target>();
            PowerUpsToDelete = new List<PowerUp>();
            PointSpritesToDelete = new List<PointSprite>();
            rand = new Random();
        }

        #region Updates
        public override void Update(GameTime gameTime)
        {
            Color transparent = new Color(0, 0, 0, 0);

            if (!Lanko_And_Glub.utility.IsGamePaused)
            {
                foreach (PowerUp p in PowerUps)
                    if (p.color == transparent)
                        p.color = Color.White;

                foreach (Target t in Enemies)
                    if (t.color == transparent)
                        t.color = Color.White;

                updateItemManager(gameTime);
                updatePowerup(gameTime);
                updateEnemies(gameTime);
                updatePointSprites(gameTime);

                base.Update(gameTime);
            }

            //hide on-screen sprites when paused
            else
            {
                foreach (PowerUp p in PowerUps)
                    p.color = transparent;
                foreach (Target t in Enemies)
                    t.color = transparent;
            }
        }      

        #region PowerUps
        private void updatePowerup(GameTime gameTime)
        {
            foreach (PowerUp p in PowerUps)
            {
                ManagePowerUpState(p);

                if (PowerUpShouldDespawn(p))
                    p.state = Item.State.DeSpawning;

                else if (p is GreenGem && Glub.State == GlubState.Thrown && Glub.Hitbox.Intersects(p.Hitbox))
                    HandleGreenGemHit(p, gameTime);

                else if (p is CyanGem && Lanko.Hitbox.Intersects(p.Hitbox))
                {
                    PowerUpsToDelete.Add(p);
                    Lanko.IncreaseSpeedMod();
                }

                checkCollision(p);
            }

            foreach (PowerUp p in PowerUpsToDelete)
                PowerUps.Remove(p);

            PowerUpsToDelete.Clear();
        }

        private void ManagePowerUpState(PowerUp p)
        {
            switch (p.state)
            {
                case Item.State.Spawning:
                    if (p.SpawnAnimation())
                        p.state = Item.State.Moving;
                    break;

                case Item.State.DeSpawning:
                    if (p.DespawnAnimation())
                        PowerUpsToDelete.Add(p);
                    break;
            }
        }

        private bool PowerUpShouldDespawn(PowerUp p)
        {
            return p.MaxTimeOnScreen < p.CurrentTimeOnScreen || Round.RoundIsOver && p.state == Item.State.Moving;
        }

        //TODO: Glub should be managing his own state
        private void HandleGreenGemHit(PowerUp gem, GameTime gameTime)
        {
            if (Glub.HasStrongBuff == false)
                Glub.ThrownToSeeking();

            if (gem.CheckDamage())
            {
                Glub.GetBuffed(gameTime);
                PowerUpsToDelete.Add(gem);
            }

            Glub.HasBounced = true;
        }
        #endregion

        #region Enemies
        private void updateEnemies(GameTime gameTime)
        {
            foreach (Target t in Enemies)
            {
                if (EnemyShouldDespawn(t))
                    t.state = Item.State.DeSpawning;
                
                else
                {
                    ManageEnemyState(t);

                    if (DidGlubHitEnemy(t))
                        OnEnemyDeath(t);
                }

                checkCollision(t);
            }

            foreach (Target t in TargetsToDelete)
                Enemies.Remove(t);

            TargetsToDelete.Clear();
        }

        private void ManageEnemyState(Target t)
        {
            switch (t.state)
            {
                case Item.State.Spawning:
                    if (t.SpawnAnimation())
                        t.state = Item.State.Moving;
                    break;

                case Item.State.DeSpawning:
                    if (t.DespawnAnimation())
                        TargetsToDelete.Add(t);
                    break;

                case Item.State.Dying:
                    if (t.DeathAnim())
                        TargetsToDelete.Add(t);
                    break;
            }
        }

        private bool EnemyShouldDespawn(Target t)
        {
            if (t.MaxTimeOnScreen < t.CurrentTimeOnScreen || Round.RoundIsOver)
                switch (t.state)
                {
                    case Item.State.Moving:
                    case Item.State.SpeedDown:
                    case Item.State.SpeedUp:
                        return true;
                }
            return false;
        }

        private bool DidGlubHitEnemy(Target t)
        {
            if (Glub.State == GlubState.Thrown && Glub.Hitbox.Intersects(t.Hitbox))
                switch (t.state)
                {
                    case Item.State.Moving:
                    case Item.State.SpeedDown:
                    case Item.State.SpeedUp:
                    case Item.State.DeSpawning:
                        return true;
                }
            return false;
        }

        private void OnEnemyDeath(Target t)
        {
            SpawnPointSprite(t);
            t.PlayHitSound();

            Round.Points += t.PointValue;

            //Glub should be managing this, not the Item Manager
            if (Glub.HasStrongBuff == false)
                Glub.ThrownToSeeking();

            Glub.HasBounced = true;

            t.state = Item.State.Dying;
            t.Speed = ENEMY_DEATH_SPEED;
            t.color = new Color(230, 230, 230, 230);
        }

        private void SpawnPointSprite(Target t)
        {
            PointSprite ps;

            ps = (t is RedBat) ? new PointSprite(Game, true) : new PointSprite(Game, false);

            ps.Initialize();
            ps.Location = new Vector2(t.LocationRect.X + 25, t.LocationRect.Y - 10);
            ps.SetStartPos();

            AddPointSprite(ps);
        }
        #endregion

        private void updatePointSprites(GameTime gameTime)
        {
            foreach(PointSprite ps in Points)
            {
                ps.Update(gameTime);
                if (ps.color.A < 10)
                    PointSpritesToDelete.Add(ps);
            }

            foreach (PointSprite ps in PointSpritesToDelete)
                Points.Remove(ps);

            PointSpritesToDelete.Clear();
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
        }
        #endregion

        #region Item Spawning
        private void positionItem(Item i)
        {
            int spawnEdgeBuffer = 5;

            int verticalPositionMinimum = 0;
            int verticalPositionMaximum = 0;

            if (i is Target)
            {
                verticalPositionMinimum = Border.TopRect.Bottom + spawnEdgeBuffer;

                if (i is RedBat)
                    verticalPositionMaximum = Border.TopRect.Bottom + 400;
                else if (i is GoldenBat)
                    verticalPositionMaximum = Border.TopRect.Bottom + 150;
            }

            else if (i is PowerUp)
            {
                if (i is CyanGem)
                {
                    verticalPositionMinimum = Border.TopRect.Bottom + 300;
                    verticalPositionMaximum = Border.BottomRect.Top - 50;
                }
                else if (i is GreenGem)
                {
                    verticalPositionMinimum = Border.TopRect.Bottom + spawnEdgeBuffer;
                    verticalPositionMaximum = Border.TopRect.Bottom + 400;
                }
            }

            i.Location = new Vector2(
                rand.Next(Border.LeftRect.Right + spawnEdgeBuffer, (Border.RightRect.Left - i.Hitbox.Width - spawnEdgeBuffer)),
                rand.Next(verticalPositionMinimum, verticalPositionMaximum));

            i.SetTranformAndRect();
            i.UpdateHitbox();
        }       

        private bool checkForOverlap(Item i)
        {
            foreach (Target targ in Enemies)
                if(i.Hitbox.Intersects(targ.Hitbox))
                    return true;

            foreach (PowerUp pow in PowerUps)
                if(i.Hitbox.Intersects(pow.Hitbox))
                    return true;

            if(i.Hitbox.Intersects(Lanko.LocationRect) || i.Hitbox.Intersects(Glub.Hitbox))
                return true;

            return false;
        }

        private void spawnTarget(EnemyType enemy)
        {
            Target targ;

            switch (enemy)
            {
                case EnemyType.RedBat:
                    targ = new RedBat(Game);
                    break;
                case EnemyType.GoldBat:
                    targ = new GoldenBat(Game);
                    break;
                default:
                    return;
            }

            targ.Initialize();
            positionItem(targ);

            while (checkForOverlap(targ))
                positionItem(targ);

            do
            {                
                targ.Direction.X = rand.Next(ITEM_MIN_X_DIRECTION, ITEM_MAX_X_DIRECTION);
                targ.Direction.Y = rand.Next(ENEMY_MIN_Y_DIRECTION, ENEMY_MAX_Y_DIRECTION);
            }
            while (Math.Abs(targ.Direction.Y) > Math.Abs(targ.Direction.X));

            if (targ.Direction.X > 0)
                targ.SpriteEffects = SpriteEffects.FlipHorizontally;

            AddTarget(targ);
        }

        private void spawnPowerUp(PowerUpType powerup)
        {
            PowerUp pow;
            switch(powerup)
            {
                case PowerUpType.CyanGem:
                    pow = new CyanGem(Game);
                    break;
                case PowerUpType.GreenGem:
                    pow = new GreenGem(Game);
                    break;
                default:
                    return;
            }

            pow.Initialize();
            positionItem(pow);

            while (checkForOverlap(pow))
                positionItem(pow);

            if(pow is GreenGem)
                pow.Direction = new Vector2(
                    rand.Next(ITEM_MIN_X_DIRECTION, ITEM_MAX_X_DIRECTION), 
                    rand.Next(GREENGEM_MIN_Y_DIRECTION, GREENGEM_MAX_Y_DIRECTION));

            AddPowerUp(pow);
        }

        private void generateItem(GameTime gametime)
        {
            double currentGameTime = gametime.TotalGameTime.TotalMilliseconds / 1000;

            //if we don't have enemies OR we exceed the time between enemy spawns AND we're below the maximum enemy limit
            if (Enemies.Count == 0 || (currentGameTime - timeTargetSpawned > TIME_PER_TARGET_SPAWN) && Enemies.Count < MAX_TARGETS)
                TrySpawnEnemy(currentGameTime);
            
            //basically the same rules but we wait until 5 seconds into a round to try and spawn any powerups
            if (Round.CurrentRoundTime > 5 && (currentGameTime - timePowerUpSpawned > TIME_PER_POWERUP_SPAWN) && PowerUps.Count < MAX_POWERUPS)
                TrySpawnPowerUp(currentGameTime);
        }

        private void TrySpawnEnemy(double currentGameTime)
        {
            int odds = rand.Next(0, 50);

            switch (odds)
            {
                case 1:
                case 44:
                case 19:
                    spawnTarget(EnemyType.RedBat);
                    timeTargetSpawned = (float)currentGameTime;
                    break;

                case 14:
                    spawnTarget(EnemyType.GoldBat);
                    timeTargetSpawned = (float)currentGameTime;
                    break;

                default:
                    break;
            }
        }

        private void TrySpawnPowerUp(double currentGameTime)
        {
            int odds = rand.Next(60, 90);

            switch (odds)
            {
                case 62:
                case 87:
                case 84:
                    spawnPowerUp(PowerUpType.CyanGem);
                    timePowerUpSpawned = (float)currentGameTime;
                    break;

                case 71:
                    spawnPowerUp(PowerUpType.GreenGem);
                    timePowerUpSpawned = (float)currentGameTime;
                    break;

                default:
                    break;
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

            //vertical collision bounce
            if (rect.Height < rect.Width)
            {
                i.Direction.Y *= -1;

                if (i.Center.Y < rect.Bottom)
                    i.Location.Y -= (rect.Height + collisionSeparation);
                else if (i.Center.Y > rect.Top)
                    i.Location.Y += (rect.Height + collisionSeparation);
            }

            //horizontal collision bounce
            else if (rect.Height > rect.Width)
            {
                i.Direction.X *= -1;

                if (i.Center.X < rect.Left)
                    i.Location.X -= (rect.Width + collisionSeparation);
                else if (i.Center.X > rect.Right)
                    i.Location.X += (rect.Width + collisionSeparation);

                //flip sprite when horizontal direction changes
                if (i is Target)
                {
                    i.SpriteEffects = (i.SpriteEffects == SpriteEffects.None)
                        ? SpriteEffects.FlipHorizontally
                        : SpriteEffects.None;
                }
            }

            i.SetTranformAndRect();
        }
        #endregion
    }
}
