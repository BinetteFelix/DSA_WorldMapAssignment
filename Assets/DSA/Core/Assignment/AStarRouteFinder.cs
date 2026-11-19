#nullable enable

using System;
using System.Collections.Generic;

namespace FrontierDispatch.Core
{
    public sealed class AStarRouteFinder 
    {
        private readonly WorldData _world;
        private readonly Dictionary<int, List<Connection>> _adjecencyList;

        public AStarRouteFinder(WorldData world)
        {
            _world = world ?? throw new NullReferenceException($"World: {world} doesn't exist");
            _adjecencyList = new Dictionary<int, List<Connection>>(_world.Locations.Count);

            foreach (Location? location in _world.Locations.Values)
            {
                if (!_adjecencyList.ContainsKey(location.Id))
                    _adjecencyList[location.Id] = new List<Connection>();
            }
            foreach (Connection? connection in _world.Connections)
            {
                if (_adjecencyList.TryGetValue(connection.FromLocationId, out List<Connection> list))
                {
                    list.Add(connection);
                }
            }
        }

        public RouteResult FindRoute(int startLocationID, int goalLocationID, ISet<int> nonTraversable)
        {
            #region Reality Check
            if  (!_adjecencyList.ContainsKey(startLocationID) || !_adjecencyList.ContainsKey(goalLocationID) ||
                nonTraversable != null && nonTraversable.Contains(startLocationID) ||
                nonTraversable != null && nonTraversable.Contains(goalLocationID))
            {
                Console.WriteLine("Failed because start or goal didn't exist");
                return new RouteResult
                (
                    found: false,
                    Array.Empty<int>(),
                    totalCost: 0,
                    expandedCount: 0
                );
                
            }
            if  (startLocationID == goalLocationID)
            {
                Console.WriteLine("Failed because start was goal");
                return new RouteResult
                (
                    found: true,
                    new[] { startLocationID },
                    totalCost: 0,
                    expandedCount: 0
                );
            }

            #endregion

            Location startNode = _world.Locations[startLocationID];
            Location goalNode = _world.Locations[goalLocationID];

            Dictionary<int, int> costs = new Dictionary<int, int>();
            Dictionary<int, int> previousNode = new Dictionary<int, int>();
            MinPriorityQueue<int> prioQueue = new MinPriorityQueue<int>();

            costs[startLocationID] = 0;
            prioQueue.Enqueue(startLocationID, 0);

            int expandedCount = 0;

            while (prioQueue.Count > 0)
            {
                MinPriorityQueue<int>.Entry entry = prioQueue.Dequeue();
                int currentID = entry.Item;
                _world.Locations.TryGetValue(currentID, out Location currentNode);

                if (!costs.TryGetValue(currentID, out int currentG))
                    continue;

                int currentTotalCost = checked(currentG + WorldQueries.ManhattanDistance(currentNode, goalNode));

                if (entry.Priority != currentTotalCost && currentNode != _world.Locations[startLocationID])
                    continue;

                expandedCount++;

                if (currentID == goalLocationID)
                {
                    var path = ReconstructPath(previousNode, startLocationID, goalLocationID);

                    return new RouteResult
                    (
                        found: true,
                        locationIds: path,
                        totalCost: costs[goalLocationID],
                        expandedCount: expandedCount
                    );
                }
                if (_adjecencyList.TryGetValue(currentID, out var outgoingEdges))
                {
                    foreach (Connection edge in outgoingEdges)
                    {
                        if (nonTraversable != null && nonTraversable.Contains(edge.ToLocationId))
                            continue;

                        int newCostG = checked(currentG + edge.TravelCost);

                        if (!costs.TryGetValue(edge.ToLocationId, out int existingCost) || newCostG < existingCost)
                        {
                            costs[edge.ToLocationId] = newCostG;
                            previousNode[edge.ToLocationId] = currentID;
                            int newTotalCost = checked(newCostG + WorldQueries.ManhattanDistance(_world.Locations[edge.ToLocationId], goalNode));
                            prioQueue.Enqueue(edge.ToLocationId, newTotalCost);
                        }
                    }
                }
            }
            Console.WriteLine("Failed because goal didn't exist");
            return new RouteResult
            (
                found: false,
                Array.Empty<int>(),
                totalCost: 0,
                expandedCount: expandedCount
            );
        }
        private static IReadOnlyList<int> ReconstructPath(Dictionary<int, int> previousNode, int startID, int goalID)
        {
            var path = new List<int>();
            int current = goalID;

            while (current != startID)
            {
                path.Add(current);
                current = previousNode[current];
            }

            path.Add(startID);
            path.Reverse();
            return path;
        }
    }
}
