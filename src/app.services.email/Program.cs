using System;
using System.Threading.Tasks;

namespace app.services.email
{
    class Program
    {
        static void Main(string[] args)
        {
            var startup = new Startup();

            Console.CancelKeyPress += (o, e) =>
            {
                Console.WriteLine("\n\n Stopping App Email Notification server");
                startup.Stop();
            };

            Console.WriteLine("\n Starting App Email Notification server ...");
            Console.WriteLine(" Press Ctrl+C or Ctrl+Break to exit");
            startup.Start();  

        }
    }
}
