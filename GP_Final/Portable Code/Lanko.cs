using System;

namespace GP_Final
{
    public enum LankoState 
    {
        Standing,
        Walking
    };

    public class Lanko 
    {
        protected LankoState proct_State;

        public LankoState Pub_State
        {
            get { return proct_State; }

            set
            {
                if (proct_State != value)
                {
                    Log("Lanko state: " + value);
                    proct_State = value;
                }
            }
        }

        protected bool hasGlub, hasJumped, isAiming;

        public bool HasGlub
        {
            get { return hasGlub; }

            set 
            { 
                if(hasGlub != value)
                {
                    Log("Lanko HasGlub: " + value);
                    hasGlub = value;
                }
            }
        }

        public bool HasJumped
        {
            get { return hasJumped; }

            set
            {
                if (hasJumped != value)
                {
                    Log("Lanko HasJumped: " + value);
                    hasJumped = value;
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
                    Log("Lanko IsAiming: " + value);
                    isAiming = value;
                }
            }
        }

        public virtual void Log(string p)
        {
            Console.Write(p);
        }
    }
}
