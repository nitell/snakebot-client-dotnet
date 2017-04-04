using System;
using Cygni.Snake.Client;

namespace Cygni.Snake.SampleBot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var client = SnakeClient.Connect(new Uri("ws://snake.cygni.se:80/training"), new MyPrinter());
            client.Start(new Glennbot()
            {
                AutoStart = false
            });
            Console.ReadLine();
        }
    }

    class MyPrinter : IGameObserver
    {
        public void OnSnakeDied(string reason, string snakeId)
        {

        }

        public void OnGameStart()
        {

        }

        public void OnGameEnd(Map map)
        {
            
        }

        public void OnUpdate(Map map)
        {

        }

        public void OnGameLink(string url)
        {

            Console.WriteLine($"The game can be viewed at '{url}'.");

        }
    }
}