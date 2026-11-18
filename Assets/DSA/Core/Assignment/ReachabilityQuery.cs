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

        /// <summary>
        /// Creates a reachability query operating on the supplied world.
        /// </summary>
        public ReachabilityQuery(WorldData world)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));

            _adjecencyList = new Dictionary<int, List<Connection>>(_world.Locations.Count);

            foreach (var location in _world.Locations)
            {
                bool isNewLocation = (!_adjecencyList.ContainsKey(location.Id));
                if (isNewLocation)
                    _adjecencyList[location.Id] = new List<Connection>();
            }
            foreach (var connection in _world.Connections)
            {
                bool connectionHasFromLocation = (_adjecencyList.TryGetValue(connection.FromLocationId, out var list));

                if (connectionHasFromLocation)
                    list.Add(connection);
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
            bool lessThanZeroMaxCost = (maxTravelCost < 0);
            bool noStartLocation = (!_adjecencyList.ContainsKey(startLocationId));

            if (lessThanZeroMaxCost || noStartLocation)
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
                bool idExists = (!visited.Add(currentID));

                if (idExists)
                    continue;

                int currentCost = cheapestCosts[currentID];

                foreach(Connection edge in _adjecencyList[currentID])
                {
                    bool locationVisited = (visited.Contains(edge.ToLocationId));

                    if (locationVisited)
                        continue;

                    int newCost = currentCost + edge.TravelCost;
                    bool toExpensive = newCost > maxTravelCost;

                    if (toExpensive)
                        continue;

                    bool isNewCost = (!cheapestCosts.TryGetValue(edge.ToLocationId, out int existingCost));
                    bool isCheaperCost = (newCost < existingCost);

                    if (isNewCost || isCheaperCost)
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
