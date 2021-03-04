using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GP_Final
{
    public interface IItemManager
    {
        List<Target> Targets { get; set; }
        List<PowerUp> PowerUps {get; set;}
        List<PointSprite> Points { get; set; }

        void AddTarget(Target t);
        void AddPowerUp(PowerUp p);
        void AddPoint(PointSprite ps);

        void RemoveTarget(Target t);
        void RemovePowerUp(PowerUp p);
        void RemovePoint(PointSprite ps);
    }

    public class ItemManager : GameComponent
    {
        private List<Target> enemies;
        private List<PowerUp> powerUps;
        private List<PointSprite> pointSprites;

        public ItemManager(Game game) : base(game)
        {
            enemies = new List<Target>();
            powerUps = new List<PowerUp>();
            pointSprites = new List<PointSprite>();
        }

        public List<Target> Enemies
        {
            get { return enemies; }
            set { enemies = value; }
        }

        public List<PowerUp> PowerUps
        {
            get { return powerUps; }
            set { powerUps = value; }
        }

        public List<PointSprite> Points
        {
            get { return pointSprites; }
            set { pointSprites = value; }
        }

        public void AddTarget(Target t) {enemies.Add(t);}
        public void AddPowerUp(PowerUp p) {powerUps.Add(p);}
        public void AddPointSprite(PointSprite ps) {pointSprites.Add(ps);}

        public void RemoveTarget(Target t) {enemies.Remove(t);}
        public void RemovePowerUp(PowerUp p) {powerUps.Remove(p);}
        public void RemovePointSprite(PointSprite ps) {pointSprites.Remove(ps);}

        public override void Update(GameTime gameTime)
        {
            foreach (Target t in Enemies) {t.Update(gameTime);}
            foreach (PowerUp p in PowerUps) {p.Update(gameTime);}
            foreach (PointSprite ps in Points) {ps.Update(gameTime);}

            base.Update(gameTime);
        }

        public void Draw(SpriteBatch sb)
        {
            foreach (Target t in Enemies)
            {
                t.DrawMarkers(sb);
                t.Draw(sb);
            }

            foreach (PowerUp p in PowerUps)
            {
                p.DrawMarkers(sb);
                p.Draw(sb);
            }

            foreach (PointSprite ps in Points)
            {
                ps.DrawMarkers(sb);
                ps.Draw(sb);
            }
        }

    }
}
