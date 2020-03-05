using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace MultiPlayerSnake
{
    public interface IGameManager
    {
        ConcurrentDictionary<string, Snake> GetAll();
        void Add(Snake snake, Func<string, Task> callback);
        void Move(Snake snake, Func<string, Task> callback);
        bool CheckExists(Snake snake);
        //void Delete(Snake snake);
    }

    public class GameManager: IGameManager
    {
        public ConcurrentDictionary<string, Snake> Snakes { get; set; }
        public Timer Timer;

        public void Initialize()
        {
            Snakes = new ConcurrentDictionary<string, Snake>();
        }

        public void Add(Snake snake, Func<string, Task> callback)
        {
            if (Snakes == null)
            {
                Initialize();
            }

            Snakes.TryAdd(snake.id, snake);
            var listOfSnakes = JsonConvert.SerializeObject(Snakes.Values);
            callback(listOfSnakes);
        }

        public void Remove(string snakeId, Func<string, Task> callback)
        {
            Snakes.TryRemove(snakeId, out Snake ex);
            var listOfSnakes = JsonConvert.SerializeObject(Snakes.Values);
            callback(listOfSnakes);
        }

        public void Move(Snake snake, Func<string, Task> callback)
        {
            Snakes.TryGetValue(snake.id, out Snake exists);

            if(exists != null)
            {
                exists.x = snake.x;
                exists.y = snake.y;
                exists.tail = snake.tail;
                exists.trail = snake.trail;
            }

            var listOfSnakes = JsonConvert.SerializeObject(Snakes.Values);
            callback(listOfSnakes);
        }
        
        public bool CheckExists(Snake snake)
        {
            if (Snakes == null)
            {
                Initialize();
            }

            return Snakes.ContainsKey(snake.id) == null;
        }

        public ConcurrentDictionary<string, Snake> GetAll()
        {
            return Snakes;
        }

        private void Callback(object state)
        {
            var listOfSnakes = JsonConvert.SerializeObject(Snakes.Values);


            //await SnakeHub.SendMessage("buuu");

            //Startup.ServiceProvider.GetRequiredService<SnakeHub>().InvokeClientMethodToAllAsync("pingSnakes", listOfSnakes).Wait();
        }
    }
}
