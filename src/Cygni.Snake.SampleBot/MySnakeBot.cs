using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Cygni.Snake.Client;

namespace Cygni.Snake.SampleBot
{
    // This project can output the Class library as a NuGet Package.
    // To enable this option, right-click on the project and select the Properties menu item. In the Build tab select "Produce outputs on build".
    public class Glennbot : SnakeBot
    {
        private Map map;
        private PathFinder pathFinder;

        public Glennbot() : base("Glennbot")
        {
        }


        public override Direction GetNextMove(Map map)
        {
            Direction retVal = Direction.Down;
            var sw = new Stopwatch();
            sw.Start();
            try
            {
                retVal = CalculateMove(map);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            sw.Stop();
            Console.WriteLine($"Going {retVal}, calculation took {sw.ElapsedMilliseconds}ms");
            return retVal;

        }

        private Direction CalculateMove(Map map)
        {

            this.map = map;
            pathFinder = new PathFinder(map);

            MapCoordinate[] path = null;
            var t = Task.Run(() => path = FindFoodPath());
            var result = t.Wait(200);
            if (path != null)
            {
                Console.WriteLine("Found foodpath...");
                return Follow(path);
            }

            //ohh.. we're in trouble...
            if (CanGo(Direction.Right))
            {
                return Direction.Right;
            }
            if (CanGo(Direction.Left))
            {
                return Direction.Left;
            }
            if (CanGo(Direction.Up))
            {
                return Direction.Up;
            }

            return Direction.Down;
        }

        private bool CanGo(Direction dir)
        {
            var nextPos = map.MySnake.HeadPosition.GetDestination(dir);
            return nextPos.IsInsideMap(map.Width, map.Height) && !map.IsObstace(nextPos) && !map.IsSnake(nextPos);
        }

        private Direction Follow(MapCoordinate[] path)
        {
            if (path.First().X > map.MySnake.HeadPosition.X)
                return Direction.Right;
            if (path.First().X < map.MySnake.HeadPosition.X)
                return Direction.Left;
            if (path.First().Y > map.MySnake.HeadPosition.Y)
                return Direction.Down;
            return Direction.Up;
        }

        private MapCoordinate[] FindFoodPath()
        {
            //Try to find food that I'm closest to
            //If not, choose the food furthest away food, just to stay alive            
            foreach (var f in map.FoodPositions.OrderBy(p => map.MySnake.HeadPosition.GetManhattanDistanceTo(p)))
            {
                var myPath = pathFinder.FindShortestPath(map.MySnake.HeadPosition, f, AvoidHeadsAndWalls);
                return myPath;
            }

            //Try to go the some point far away
            foreach (var corner in new[]
                {

                        new MapCoordinate(0, 0), new MapCoordinate(0, map.Height-1), new MapCoordinate(map.Width-1, 0),
                        new MapCoordinate(map.Height-1, map.Width-1)
                    }.OrderByDescending(c => map.MySnake.HeadPosition.GetManhattanDistanceTo(c))
            )
            {
                var path = pathFinder.FindShortestPath(map.MySnake.HeadPosition, corner);
                if (path != null)
                    return path;
            }

            return null;
        }

        private int AvoidHeadsAndWalls(Map map, MapCoordinate target)
        {
            if (map.Snakes.Any(s => s.Id != map.MySnake.Id && s.HeadPosition.GetManhattanDistanceTo(target) < 3))
                return 5;

            if (target.X == 0 || target.X == map.Width - 1 ||
                target.Y == 0 || target.Y == map.Height - 1)
                return 5;

            return 1;
        }
    }
}