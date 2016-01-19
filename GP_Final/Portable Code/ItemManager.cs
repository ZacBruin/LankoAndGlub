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

    public class ItemManager : Microsoft.Xna.Framework.GameComponent
    {
        List<Target> targets;
        List<PowerUp> powerUps;
        List<PointSprite> pointSprites;

        public ItemManager(Game game) : base(game)
        {
            this.targets = new List<Target>();
            this.powerUps = new List<PowerUp>();
            this.pointSprites = new List<PointSprite>();
        }

        public List<Target> Targets
        {
            get { return targets; }
            set { targets = value; }
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

        public void AddTarget(Target t) {this.targets.Add(t);}
        public void AddPowerUp(PowerUp p) {this.powerUps.Add(p);}
        public void AddPointSprite(PointSprite ps) {this.pointSprites.Add(ps);}

        public void RemoveTarget(Target t) {this.targets.Remove(t);}
        public void RemovePowerUp(PowerUp p) {this.powerUps.Remove(p);}
        public void RemovePointSprite(PointSprite ps) {this.pointSprites.Remove(ps);}

        public override void Update(GameTime gameTime)
        {
            foreach (Target t in Targets) {t.Update(gameTime);}
            foreach (PowerUp p in PowerUps) {p.Update(gameTime);}
            foreach (PointSprite ps in Points) {ps.Update(gameTime);}

            base.Update(gameTime);
        }

        public void Draw(SpriteBatch sb)
        {
            foreach (Target t in Targets)
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
