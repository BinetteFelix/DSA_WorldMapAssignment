using System;
using System.Collections.Generic;

namespace FrontierDispatch.Core
{
    // Assignment Phase 5. Adapt shortest-path reasoning to a travel-cost budget.

    /// <summary>
    /// Finds locations that can be reached from a starting location without
    /// exceeding a maximum total travel cost.
    ///
    /// This is an adaptation of shortest-path reasoning: instead of searching
    /// for one goal, the query determines the cheapest known cost to every
    /// location that fits within the supplied budget.
    ///
    /// This class is intentionally incomplete in the assignment starter.
    ///
    /// What do we mean by "query"? In this context, a query is simply a one-time search for information.
    /// </summary>
    public sealed class ReachabilityQuery
    {
        private readonly WorldData _world;
        private readonly Dictionary<int, List<Connection>> _adjecencyList;
        private readonly HashSet<int> _validLocationIDs;

        /// <summary>
        /// Creates a reachability query operating on the supplied world.
        /// </summary>
        public ReachabilityQuery(WorldData world)
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
                    list.Add(connection);
                }
            }
        }

        /// <summary>
        /// Returns reachable location IDs mapped to their cheapest known
        /// travel cost from the starting location.
        ///
        /// Only locations whose cheapest cost is within maxTravelCost should
        /// be included. The starting location has cost 0.
        /// </summary>
        public IReadOnlyDictionary<int, int> FindReachable(int startLocationId, int maxTravelCost)
        {
            if (maxTravelCost < 0 || !_validLocationIDs.Contains(startLocationId))
                return new Dictionary<int, int>();

            var cheapestCosts = new Dictionary<int, int>
            {
                [startLocationId] = 0
            };

            HashSet<int> visited = new HashSet<int>();
            MinPriorityQueue<int> prioQueue = new MinPriorityQueue<int>();
            prioQueue.Enqueue(startLocationId, 0);

            while(prioQueue.Count > 0)
            {
                int currentID = prioQueue.Dequeue().Item;

                if (!visited.Add(currentID))
                    continue;

                int currentCost = cheapestCosts[currentID];

                foreach(Connection edge in _adjecencyList[currentID])
                {
                    if (visited.Contains(edge.ToLocationId))
                        continue;

                    int newCost = currentCost + edge.TravelCost;

                    if (newCost > maxTravelCost)
                        continue;

                    if (!cheapestCosts.TryGetValue(edge.ToLocationId, out int existingCost) || newCost < existingCost)
                    {
                        cheapestCosts[edge.ToLocationId] = newCost;
                        prioQueue.Enqueue(edge.ToLocationId, newCost);
                    }
                }
            }
            return cheapestCosts;
        }
    }
}
