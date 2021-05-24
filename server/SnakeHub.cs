using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace MultiPlayerSnake
{
    public class SnakeHub: Hub
    {
        private Game _game;

        public SnakeHub(Game game)
        {
            _game = game;
        }

        public Task ChangeDirection(string snakeData)
        {
            if (snakeData == null) return null;

            var snake = JsonConvert.DeserializeObject<Snake>(snakeData);
            
            _game.ChangeDirection(snake);

            return Clients.Client(snake.id).SendAsync("ReceiveMessage", JsonConvert.SerializeObject(snake));
        }

        public Task ConnectedSnake(string serializedSnake, string serializedCanvas)
        {
            var snake = JsonConvert.DeserializeObject<Snake>(serializedSnake);
            var canvas = JsonConvert.DeserializeObject<Canvas>(serializedCanvas);

            _game.Add(snake, canvas);

            return Clients.Client(snake.id).SendAsync("ReceiveMessage", snake.id);
        }

        [HubMethodName("SendMessage")]
        public Task SendMessage(string message)
        {
            return Clients.All.SendAsync("ReceiveMessage", message);
        }

        //public Task PingSnakesAndFood(string snakes, string food)
        //{
        //    return Clients.All.SendAsync("Snakes", snakes, food);
        //}

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

            _game.Remove(socketId);

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "SignalR Users");
            await base.OnDisconnectedAsync(exception);

            await SendMessage($"Snake with socket id :{socketId} is now disconnected!");

        }
    }
}
