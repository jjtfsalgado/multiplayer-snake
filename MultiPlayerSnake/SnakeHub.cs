using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace MultiPlayerSnake
{
    public class SnakeHub: Hub
    {
        private GameManager _gameManager;

        public SnakeHub(GameManager gameManager)
        {
            _gameManager = gameManager;
        }

        private static readonly System.Timers.Timer _timer = new System.Timers.Timer();

        public Task OnMove(string snakeData)
        {
            if (snakeData == null) return null;

            var snake = JsonConvert.DeserializeObject<Snake>(snakeData);
            _gameManager.Move(snake, async (string snakes) => await PingSnakes(snakes));

            return Clients.Client(snake.id).SendAsync("ReceiveMessage", JsonConvert.SerializeObject(snake));
        }

        public Task ConnectedSnake(string serializedSnake)
        {
            var snake = JsonConvert.DeserializeObject<Snake>(serializedSnake);

            if (!_gameManager.CheckExists(snake))
            {
                _gameManager.Add(snake, async (string snakes) => await PingSnakes(snakes));
            }

            return Clients.Client(snake.id).SendAsync("ReceiveMessage", snake.id);
        }

        //public Task DisconnectedSnake(string serializedSnake)
        //{
        //    var snake = JsonConvert.DeserializeObject<Snake>(serializedSnake);

        //    _gameManager.Remove(snake);
            
        //    return Clients.Client(snake.id).SendAsync("ReceiveMessage", snake.id);
        //}

        [HubMethodName("SendMessage")]
        public Task SendMessage(string message)
        {
            return Clients.All.SendAsync("ReceiveMessage", message);
        }

        public Task PingSnakes(string snakes)
        {
            return Clients.All.SendAsync("Snakes", snakes);
        }

        public override async Task OnConnectedAsync()
        {
            var socketId = Context.ConnectionId;

            await Groups.AddToGroupAsync(socketId, "SignalR Users");
            await base.OnConnectedAsync();

            await SendMessage($"Snake with socket id :{socketId} is now connected!");
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {

            var socketId = Context.ConnectionId;

            _gameManager.Remove(socketId, async (string snakes) => await PingSnakes(snakes));

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "SignalR Users");
            await base.OnDisconnectedAsync(exception);

            await SendMessage($"Snake with socket id :{socketId} is now disconnected!");

        }
    }
}
