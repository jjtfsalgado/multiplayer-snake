using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace MultiPlayerSnake
{
    public class GameManager
    {
        private static GameManager instance;
        private static readonly object padlock = new object();
        public ConcurrentDictionary<string, Snake> Snakes { get; set; }
        public Timer Timer;

        public static GameManager Instance
        {
            get
            {
                lock (padlock)
                {
                   return instance ?? (instance = new GameManager());
                }
            }
        }


        public void Initialize()
        {
            Snakes = new ConcurrentDictionary<string, Snake>();
            
            //15 frames per second
            Timer = new Timer(Callback, null, 0, 1000/15);


        }

        private void Callback(object state)
        {
            var listOfSnakes = JsonConvert.SerializeObject(Snakes.Values);
            Startup.ServiceProvider.GetRequiredService<SnakeHandler>().InvokeClientMethodToAllAsync("pingSnakes", listOfSnakes).Wait();
        }
    }
}
