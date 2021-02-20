﻿namespace GP_Final
{
    public class GameConsoleLanko : Lanko
    {
        GameConsole console;

        public GameConsoleLanko()
        {
            console = null;
        }

        public GameConsoleLanko(GameConsole gameconsole)
        {
            console = gameconsole;
        }

        public override void Log(string s)
        {
            if (console != null)
                console.GameConsoleWrite(s);

            else
                base.Log(s);
        }

    }
}
