using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace MultiPlayerSnake
{
    public interface IGameManager
    {
        ConcurrentDictionary<string, Snake> GetAll();
        void Add(Snake snake, Canvas Canvas);
        void ChangeDirection(Snake snake);
        //void Delete(Snake snake);
    }

    public class GameManager : IGameManager
    {
        public ConcurrentDictionary<string, Snake> Snakes { get; set; }
        public List<Pixel> Food { get; set; }
        public Canvas Canvas {get; set;}
        public Timer Timer;
        private readonly IHubContext<SnakeHub> _hub;

        public GameManager(IHubContext<SnakeHub> hub)
        {
            _hub = hub;
        }

        public void Initialize(Canvas canvas)
        {
            Snakes = new ConcurrentDictionary<string, Snake>();
            Canvas = canvas;

            AddFood();

            Timer = new Timer(onMove, null, 0, 1000/30);
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

        public void Add(Snake snake, Canvas Canvas)
        {
            if (Snakes == null)
            {
                Initialize(Canvas);
            }

            if (Snakes.ContainsKey(snake.id))
            {
                return;
            }

            Snakes.TryAdd(snake.id, snake);
            var listOfSnakes = JsonConvert.SerializeObject(Snakes.Values);
            var food = JsonConvert.SerializeObject(Food);

            _hub.Clients.All.SendAsync("Snakes", listOfSnakes, food);
        }

        public void Remove(string snakeId)
        {
            Snakes.TryRemove(snakeId, out Snake ex);
            var listOfSnakes = JsonConvert.SerializeObject(Snakes.Values);
            var food = JsonConvert.SerializeObject(Food);

            _hub.Clients.All.SendAsync("Snakes", listOfSnakes, food);
        }

        public void ChangeDirection(Snake snake)
        {
            Snakes.TryGetValue(snake.id, out Snake exists);

            if(exists != null)
            {
                exists.direction = snake.direction;

                var listOfSnakes = JsonConvert.SerializeObject(Snakes.Values);
                var food = JsonConvert.SerializeObject(Food);

                _hub.Clients.All.SendAsync("Snakes", listOfSnakes, food);
            }
        }

        private void onMove(object state)
        {
            foreach (Snake sna in Snakes.Values.ToList())
            {
                Snakes.TryGetValue(sna.id, out Snake exists);

                if (exists != null)
                {
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

                    exists.trail.Add(new Pixel { x = exists.x, y = exists.y });

                    if (exists.trail.Count >= exists.tail)
                    {
                        exists.trail.RemoveAt(0);
                    }

                    HandleMovement(exists);
                }
            }
        }

        public void HandleMovement(Snake snake)
        {
            var head = snake.trail.ToList().LastOrDefault();
            Snake collidedSnake = null;

            if (head != null)
            {
                var hasEaten = Food.Find(i => (i.x - 3) <= head.x && (i.x + 3 >= head.x) && (i.y + 3 >= head.y) && (i.y - 3 <= head.y)) != null;

                if (hasEaten)
                {
                    Snakes.TryGetValue(snake.id, out Snake exists);

                    if (exists != null)
                    {
                        exists.tail += 10;
                        AddFood();
                    }
                }

                foreach (Snake sna in Snakes.Values)
                {
                    if (sna.id != snake.id && sna.trail.Find(i => (i.x == head.x && i.y == head.y)) != null)
                    {
                        collidedSnake = sna;
                    };
                }

                if (collidedSnake != null)
                {
                    if (collidedSnake.tail >= snake.tail)
                    {
                        Remove(snake.id);
                        return;
                    }
                    else
                    {
                        Snakes.TryGetValue(snake.id, out Snake exists);

                        if (exists != null)
                        {
                            exists.tail += collidedSnake.tail;
                            Remove(collidedSnake.id);
                            return;
                        }
                    }
                }
            }

            var listOfSnakes = JsonConvert.SerializeObject(Snakes.Values);
            var food = JsonConvert.SerializeObject(Food);

            _hub.Clients.All.SendAsync("Snakes", listOfSnakes, food);
        }

        public ConcurrentDictionary<string, Snake> GetAll()
        {
            return Snakes;
        }
    }
}
