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
            var t = Task.Run(() => path= FindFoodPath());
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
            MapCoordinate[] longestPath = null;

            foreach (var f in map.FoodPositions.OrderBy(p => map.MySnake.HeadPosition.GetManhattanDistanceTo(p)))
            {
                var myPath = pathFinder.FindShortestPath(map.MySnake.HeadPosition, f);
                if (myPath != null)
                {
                    var otherPaths = map.Snakes.Where(s => s.Id != map.MySnake.Id)
                        .Select(s => pathFinder.FindShortestPath(s.HeadPosition, f))
                        .Where(p => p != null && p.Any()).ToArray();
                    var shortestOtherPath = otherPaths.Any() ? otherPaths.Min(p => p.Length) : int.MaxValue;
                    if (myPath.Length < shortestOtherPath)
                        return pathFinder.FindShortestPath(map.MySnake.HeadPosition, f, AvoidHeadsAndWalls);

                    var longestSafePath = pathFinder.FindShortestPath(map.MySnake.HeadPosition, f, AvoidHeadsAndWalls);

                    if (longestPath == null || longestSafePath.Length < myPath.Length)
                        longestPath = longestSafePath;
                }
            }

            ////Try to go the furthest away corner, just to stay alive
            //if (longestPath == null)
            //{
            //    foreach (var corner in new[]
            //        {
            //            new MapCoordinate(0, 0), new MapCoordinate(0, map.Height), new MapCoordinate(map.Width, 0),
            //            new MapCoordinate(map.Height, map.Width)
            //        }.OrderByDescending(c => map.MySnake.HeadPosition.GetManhattanDistanceTo(c))
            //    )
            //    {
            //        var path = pathFinder.FindShortestPath(map.MySnake.HeadPosition, corner);
            //        if (path != null)
            //            return path;
            //    }
            //}
            return longestPath;
        }

        private int AvoidHeadsAndWalls(Map map, MapCoordinate target)
        {
            if (map.Snakes.Any(s => s.Id != map.MySnake.Id && s.HeadPosition.GetManhattanDistanceTo(target) < 3))
                return 2;

            if (target.X == 0 || target.X == map.Width - 1 ||
                target.Y == 0 || target.Y == map.Height - 1)
                return 2;

            return 1;
        }
    }
}