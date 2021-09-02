using System;
using System.IO;

namespace SMWEngine
{
    public static class Program
    {
        [STAThread]
        public static void Main() => new SMW().Run();
    }
}
