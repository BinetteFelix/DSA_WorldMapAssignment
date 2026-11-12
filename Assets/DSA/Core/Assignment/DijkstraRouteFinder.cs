using System;
using System.Collections.Generic;
using System.Linq;

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
        private readonly Dictionary<int, List<Connection>> _adjecencyList;
        private readonly HashSet<int> _validLocationIDs;

        /// <summary>
        /// Constructor. Creates a route finder operating on the supplied world.
        /// </summary>
        public DijkstraRouteFinder(WorldData world)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));

            _adjecencyList = new Dictionary<int, List<Connection>>(_world.Locations.Count);
            _validLocationIDs = new HashSet<int>(_world.Locations.Count);

            foreach (var location in _world.Locations)
            {
                _validLocationIDs.Add(location.Id);
                if (!_adjecencyList.ContainsKey(location.Id))
                    _adjecencyList[location.Id] = new List<Connection>();
            }
            foreach (var connection in world.Connections)
            {
                if (_adjecencyList.TryGetValue(connection.FromLocationId, out var list))
                {
                    //list = new List<Connection>();
                    //_adjecencyList[connection.FromLocationId] = list;
                    list.Add(connection);
                }
                //_adjecencyList.Add(connection.FromLocationId, _worldQueries.FindOutgoingConnections(connection.FromLocationId).ToList());
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
            if (!_validLocationIDs.Contains(startLocationId) || !_validLocationIDs.Contains(goalLocationId))
                return new RouteResult(found: false, 
                    Array.Empty<int>(), 
                    totalCost: 0, 
                    expandedCount: 0);

            if (startLocationId == goalLocationId)
                return new RouteResult(found: true, 
                    new[] {startLocationId}, 
                    totalCost: 0, 
                    expandedCount: 0
                );

            Dictionary<int, int> distances = new Dictionary<int, int>();
            Dictionary<int, int> previousNode = new Dictionary<int, int>();
            HashSet<int> visited = new HashSet<int>();
            MinPriorityQueue<int> prioQueue = new MinPriorityQueue<int>();

            distances[startLocationId] = 0;
            prioQueue.Enqueue(startLocationId, 0);

            int expandedCount = 0;

            while(prioQueue.Count > 0)
            {
                int currentID = prioQueue.Dequeue();

                if (!visited.Add(currentID))
                    continue;

                expandedCount++;

                if (currentID == goalLocationId)
                {
                    var path = ReconstructPath(previousNode, startLocationId, goalLocationId);

                    return new RouteResult(
                        found: true,
                        locationIds: path,
                        totalCost: distances[goalLocationId],
                        expandedCount: expandedCount
                    );
                }

                int currentDistance = distances[currentID];

                if (_adjecencyList.TryGetValue(currentID, out var edges))
                {
                    foreach (Connection? edge in _adjecencyList[currentID])
                    {
                        if (visited.Contains(edge.ToLocationId))
                            continue;
                        int newDistance = currentDistance + edge.TravelCost;

                        if (!distances.TryGetValue(edge.ToLocationId, out int existingDistance) || newDistance < existingDistance)
                        {
                            distances[edge.ToLocationId] = newDistance;
                            previousNode[edge.ToLocationId] = currentID;
                            prioQueue.Enqueue(edge.ToLocationId, newDistance);
                        }
                    }
                }
            }
            return new RouteResult(found: false,
                    Array.Empty<int>(),
                    totalCost: 0,
                    expandedCount: expandedCount);
        }
        /// <summary>
        /// Returns the list of locations visited to get the shortest path.
        /// </summary>
        /// <returns></returns>
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
