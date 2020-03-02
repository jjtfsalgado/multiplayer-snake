using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
using WebSocketManager;
using WebSocketManager.Common;

namespace MultiPlayerSnake
{
    public class SnakeHandler: WebSocketHandler
    {
        public SnakeHandler(WebSocketConnectionManager webSocketConnectionManager) : base(webSocketConnectionManager)
        {
        }

        public async Task OnMove(string socketId, string snakeData)
        {
            var snake = JsonConvert.DeserializeObject<Snake>(snakeData);

            GameManager.Instance.Snakes.TryGetValue(snake.id, out Snake exists);

            if(exists != null)
            {
                exists.x = snake.x;
                exists.y = snake.y;
                exists.trail = snake.trail;
            }
            

        }

        public async Task ConnectedSnake(string socketId, string serializedSnake)
        {
            var snake = JsonConvert.DeserializeObject<Snake>(serializedSnake);
            var exists = GameManager.Instance.Snakes.ContainsKey(socketId);

            if (!exists)
            {
                GameManager.Instance.Snakes.TryAdd(snake.id, snake);
            }
        }

        public async Task DisconnectedSnake(string socketId, string test)
        {
            GameManager.Instance.Snakes.TryRemove(socketId, out Snake pew);
        }

        public override async Task OnConnected(WebSocket socket)
        {
            await base.OnConnected(socket);

            var socketId = WebSocketConnectionManager.GetId(socket);

            var message = new Message
            { 
                MessageType = MessageType.Text,
                Data = $"Snake with socket id :{socketId} is now connected!"      
            };


            await SendMessageToAllAsync(message);
        }

        public override async Task OnDisconnected(WebSocket socket)
        {
            await base.OnDisconnected(socket);

            var socketId = WebSocketConnectionManager.GetId(socket);

            var message = new Message
            {
                MessageType = MessageType.Text,
                Data = $"Snake with socket id :{socketId} is now disconnected!"
            };

            await SendMessageToAllAsync(message);
        }
    }
}
