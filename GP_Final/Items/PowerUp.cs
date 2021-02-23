using Microsoft.Xna.Framework;

namespace GP_Final
{
    public abstract class PowerUp : Item
    {
        public PowerUp(Game game) : base(game){}

        protected void UpdatePowerUp(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public virtual bool CheckDamage()
        {
            return true;
        }      

    }
}
