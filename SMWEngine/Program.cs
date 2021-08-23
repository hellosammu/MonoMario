using System;

namespace SMWEngine
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new SMW())
                game.Run();
        }
    }
}
