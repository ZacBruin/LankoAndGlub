namespace GP_Final
{
    public class GameConsoleGlub : Glub
    {
        GameConsole console;

        public GameConsoleGlub()
        {
            this.console = null;
        }

        public GameConsoleGlub(GameConsole gameconsole)
        {
            this.console = gameconsole;
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
