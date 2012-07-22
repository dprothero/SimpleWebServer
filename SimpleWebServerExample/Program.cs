using System;
using MB.Web;
using System.Diagnostics;

namespace SimpleWebServerExample
{
    class Program
    {
        static void Main(string[] args)
        {
            Trace.Listeners.Add(new ConsoleTraceListener());
           
            var server = new SimpleWebServer("http://localhost:10000/", "files/");

            server.Start();

            Console.WriteLine("Press key to exit ..."); 
            Console.ReadKey();
        }
    }
}
