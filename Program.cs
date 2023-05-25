using System;
using System.Collections.Generic;
using System.Linq;

namespace AFD_Minimal
{
    class Program
    {
        static void Main(string[] args)
        {
            AFD afd = new AFD(@"../../Input5.txt");
            afd.MinimizeazaAFD();
            Console.ReadKey();
        }
    }
}
