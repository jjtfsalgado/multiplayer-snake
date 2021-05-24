using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace MultiPlayerSnake
{
    public interface IGame
    {
        void Add(Snake snake, Canvas Canvas);
        void ChangeDirection(Snake snake);
        //void Delete(Snake snake);
    }

    public class Game : IGame
    {
        private ConcurrentDictionary<string, Snake> Snakes { get; set; }
        private List<Pixel> Food { get; set; }
        private Canvas Canvas {get; set;}
        private Timer Timer;
        private readonly IHubContext<SnakeHub> _hub;

        public Game(IHubContext<SnakeHub> hub)
        {
            _hub = hub;
        }
        
        public void Add(Snake snake, Canvas canvas)
        {
            Canvas = canvas;

            if (Snakes == null)
            {
                Snakes = new ConcurrentDictionary<string, Snake>();
                Timer = new Timer(onMove, null, 0, 1000/30);
                AddFood();
            }

            if (Snakes.ContainsKey(snake.id))
            {
                return;
            }

            Snakes.TryAdd(snake.id, snake);
            SendSnakes();
        }

        public void Remove(string snakeId)
        {
            Snakes.TryRemove(snakeId, out Snake ex);
            SendSnakes();
        }

        public void ChangeDirection(Snake snake)
        {
            Snakes.TryGetValue(snake.id, out Snake exists);
            if(exists == null) return;

            exists.direction = snake.direction;
            SendSnakes();
        }

        private void onMove(object state)
        {
            foreach (Snake snake in Snakes.Values.ToList())
            {
                Snakes.TryGetValue(snake.id, out Snake exists);
                
                if(exists == null) continue;
                
                if(exists.direction == "xx")
                {
                    exists.x += exists.speed;
                }else if (exists.direction == "-xx")
                {
                    exists.x -= exists.speed;
                }else if (exists.direction == "yy")
                {
                    exists.y -= exists.speed;
                }else if (exists.direction == "-yy")
                {
                    exists.y += exists.speed;
                }

                if (exists.x >= Canvas.width)
                {
                    exists.x = 0;
                }
                else if (exists.y >= Canvas.height)
                {
                    exists.y = 0;
                }
                else if (exists.x <= 0)
                {
                    exists.x = Canvas.width;
                }
                else if (exists.y <= 0)
                {
                    exists.y = Canvas.height;
                }
                
                var head = new Pixel {x = exists.x, y = exists.y};
                exists.trail.Add(head);

                if (exists.trail.Count >= exists.tail)
                {
                    exists.trail.RemoveAt(0);
                }
                
                var hasEaten = Food.Find(i => (i.x - 5) <= head.x && (i.x + 5 >= head.x) && (i.y + 5 >= head.y) && (i.y - 5 <= head.y)) != null;

                if (hasEaten)
                {
                    exists.tail += 10;
                    AddFood();
                }

                //check colisions
                foreach (Snake s in Snakes.Values.ToList())
                {
                    if (s.id != snake.id && s.trail.Find(i => (i.x == head.x && i.y == head.y)) != null)
                    {
                        Remove(s.id);
                    };
                }
            }
            
            SendSnakes();
        }

        private void AddFood()
        {
            Random random = new Random();

            if (Food == null)
            {
                Food = new List<Pixel> { };
            }
            else
            {
                Food.Clear();
            }

            Food.Add(new Pixel { x = random.Next(Canvas.width), y = random.Next(Canvas.height) });
        }
        
        private void SendSnakes()
        {
            var listOfSnakes = JsonConvert.SerializeObject(Snakes.Values);
            var food = JsonConvert.SerializeObject(Food);

            _hub.Clients.All.SendAsync("Snakes", listOfSnakes, food);
        }
    }
}
