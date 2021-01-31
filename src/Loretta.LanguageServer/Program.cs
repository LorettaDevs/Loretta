using System;
using System.Threading;
using System.Threading.Tasks;

namespace Loretta.LanguageServer
{
    class Program
    {
        static void Main ( string[] args )
        {
            Console.WriteLine ( "Hello World!" );
        }

        private static async Task RunWithCancellationAsync(Func<CancellationToken, Task> func )
        {

        }
    }
}
