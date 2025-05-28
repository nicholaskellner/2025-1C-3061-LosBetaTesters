using System;
using Microsoft.Xna.Framework;

namespace TGC.MonoGame.TP
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new TGCGame())
                game.Run();
        }
    }
}
