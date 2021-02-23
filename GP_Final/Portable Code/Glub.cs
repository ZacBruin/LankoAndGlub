using System;

namespace GP_Final
{
    public enum GlubState
    {
        Still,
        Following,
        Held,
        Thrown,
        Falling,
        SeekingLanko,
        Stranded,
        AnimCoolDown
    };

    public class Glub
    {
        protected GlubState proct_State;

        public GlubState Pub_State
        {
            get { return proct_State; }

            set
            {
                if(proct_State != value)
                {
                    Log("Glub State: " + value);
                    proct_State = value;
                }
            }
        }

        protected bool withLanko, hasStrongBuff;

        public bool WithLanko
        {
            get { return withLanko; }

            set
            {
                if (withLanko != value)
                {
                    Log("Glub WithLanko: " + value);
                    withLanko = value;
                }
            }
        }

        public bool HasStrongBuff
        {
            get { return hasStrongBuff; }

            set
            {
                if (hasStrongBuff != value)
                {
                    Log("Glub CanDamage: " + value);
                    hasStrongBuff = value;
                }
            }
        }

        public virtual void Log(string p)
        {
            Console.Write(p);
        }
    }
}
