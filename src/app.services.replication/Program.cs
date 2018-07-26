using System;

namespace app.services.replication
{
    class Program
    {
        static void Main(string[] args)
        {
            var startup = new Startup();

            Console.CancelKeyPress += (o, e) =>
            {
                Console.WriteLine("\n\n Stopping App Data Sync  server");
                startup.Stop();
            };

            Console.WriteLine("\n Starting App Data Sync server ...");
            Console.WriteLine(" Press Ctrl+C or Ctrl+Break to exit");
            startup.Start();  
        }
    }
}
