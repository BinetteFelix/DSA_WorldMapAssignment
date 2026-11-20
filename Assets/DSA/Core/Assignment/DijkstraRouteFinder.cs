#nullable enable

using System;
using System.Collections.Generic;

namespace FrontierDispatch.Core
{
    // Assignment Phase 4.

    /// <summary>
    /// Finds minimum-total-cost routes through a Frontier Dispatch world
    /// using Dijkstra's shortest-path algorithm.
    ///
    /// This class is intentionally incomplete in the assignment starter.
    /// It should eventually use the student's prepared world representation
    /// and MinPriorityQueue implementation.
    /// </summary>
    public sealed class DijkstraRouteFinder
    {
        private readonly WorldData _world;
        private readonly List<Connection>[] _adjecencyList;

        /// <summary>
        /// Constructor. Creates a route finder operating on the supplied world.
        /// </summary>
        public DijkstraRouteFinder(WorldData world)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));

            int maxId = -1;
            foreach (var location in _world.Locations.Values)
                if (location.Id > maxId)
                    maxId = location.Id;
            
            _adjecencyList = new List<Connection>[maxId + 1];

            foreach (var location in _world.Locations.Values)
            {
                if (_adjecencyList[location.Id] == null)
                    _adjecencyList[location.Id] = new List<Connection>(4);
            }
            foreach (var connection in _world.Connections)
            {
                if (_adjecencyList[connection.FromLocationId] != null)
                    _adjecencyList[connection.FromLocationId].Add(connection);
            }
        }

        /// <summary>
        /// Finds a minimum-total-travel-cost route from the start location
        /// to the goal location.
        ///
        /// A successful RouteResult should contain the route as location IDs,
        /// including both the start and goal locations.
        /// </summary>
        public RouteResult FindRoute(int startLocationId, int goalLocationId)
        {
            if (startLocationId < 0 || startLocationId >= _adjecencyList.Length ||
                goalLocationId < 0 || goalLocationId >= _adjecencyList.Length ||
                _adjecencyList[startLocationId] == null || _adjecencyList[goalLocationId] == null)
                return new RouteResult
                (
                    found: false, 
                    Array.Empty<int>(), 
                    totalCost: 0, 
                    expandedCount: 0
                );

            if (startLocationId == goalLocationId)
                return new RouteResult
                (   
                    found: true, 
                    new[] {startLocationId}, 
                    totalCost: 0, 
                    expandedCount: 0
                );

            Dictionary<int, int> costs = new Dictionary<int, int>();
            Dictionary<int, int> previousNodes = new Dictionary<int, int>();
            HashSet<int> visited = new HashSet<int>();
            MinPriorityQueue<int> prioQueue = new MinPriorityQueue<int>();

            costs[startLocationId] = 0;
            prioQueue.Enqueue(startLocationId, 0);

            int expandedCount = 0;

            while(prioQueue.Count > 0)
            {
                int currentID = prioQueue.Dequeue().Item;

                if (!visited.Add(currentID))
                    continue;

                expandedCount++;

                if (currentID == goalLocationId)
                {
                    var path = ReconstructPath(previousNodes, startLocationId, goalLocationId);

                    return new RouteResult
                    (
                        found: true,
                        locationIds: path,
                        totalCost: costs[goalLocationId],
                        expandedCount: expandedCount
                    );
                }

                int currentCost = costs[currentID];

                if (_adjecencyList[currentID] != null)
                {
                    foreach (Connection? edge in _adjecencyList[currentID])
                    {
                        if (visited.Contains(edge.ToLocationId))
                            continue;

                        int newCost = currentCost + edge.TravelCost;

                        if (!costs.TryGetValue(edge.ToLocationId, out int existingCost) || newCost < existingCost)
                        {
                            costs[edge.ToLocationId] = newCost;
                            previousNodes[edge.ToLocationId] = currentID;
                            prioQueue.Enqueue(edge.ToLocationId, newCost);
                        }
                    }
                }
            }
            return new RouteResult
            (
                found: false,
                Array.Empty<int>(),
                totalCost: 0,
                expandedCount: expandedCount
            );
        }
        /// <summary>
        /// Returns the list of locations visited to get the shortest path.
        /// </summary>
        /// <returns></returns>
        private static IReadOnlyList<int> ReconstructPath(Dictionary<int, int> previousNodes, int startID, int goalID)
        {
            var path = new List<int>();
            int current = goalID;

            while (current != startID)
            {
                path.Add(current);
                current = previousNodes[current];
            }

            path.Add(startID);
            path.Reverse();
            return path;
        }
    }
}
