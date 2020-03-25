using Newtonsoft.Json;
using System;

namespace ExampleWebservice
{
    class Program
    {
        static void Main(string[] args)
        {
            var webpage = new WebpageManager();
            var ws = new WebServer("http://localhost:5000/",
                request =>
            {
                Console.WriteLine("Received request:");
                Console.WriteLine($"RawUrl     => {request.RawUrl}");
                Console.WriteLine($"HttpMethod => {request.HttpMethod}");
                
                return webpage.GetPage(request.RawUrl, () =>
                {
                    // example data
                    return JsonConvert.SerializeObject(new DataItem { ID = 1, Title = "Test", Description = "What ever you want." });
                });
            });

            ws.Run();
            Console.WriteLine("Press a key to quit.");
            Console.ReadKey();
            ws.Stop();
        }
    }
}
