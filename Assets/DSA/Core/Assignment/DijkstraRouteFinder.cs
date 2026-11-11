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
        private readonly WorldQueries _worldQueries;

        /// <summary>
        /// Constructor. Creates a route finder operating on the supplied world.
        /// </summary>
        public DijkstraRouteFinder(WorldData world, WorldQueries queries)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));
            _worldQueries = new WorldQueries(_world);

            _adjecencyList = new Dictionary<int, List<Connection>>(_world.Locations.Count);
            _validLocationIDs = new HashSet<int>(_world.Locations.Count);

            foreach (var location in _world.Locations)
            {
                if (_validLocationIDs.Add(location.Id))
                    continue;
                _adjecencyList[location.Id] = new List<Connection>();
            }
            foreach (var connection in world.Connections)
                _adjecencyList.Add(connection.FromLocationId, _worldQueries.FindOutgoingConnections(connection.FromLocationId).ToList());
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
            // TODO: implement Dijkstra using your prepared world representation
            // and MinPriorityQueue<T>.
            throw new NotImplementedException();
        }
    }
}
