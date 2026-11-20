using System;
using NUnit.Framework;
using FrontierDispatch.Core;
using System.Collections.Generic;

namespace FrontierDispatch.Tests
{
    /// <summary>
    /// Small contract examples for later assignment phases.
    ///
    /// These are not an exhaustive assignment test suite.
    /// Students are expected to enable the relevant tests as they reach each
    /// phase and add meaningful tests of their own, including edge cases.
    /// </summary>
    public sealed class AssignmentStarterTests
    {
        private static WorldData CreateAlternativeRoutesWorld()
        {
            #region Custom Test World Creation (with alternative routes)
            var locations = new Dictionary<int, Location>
            {
                { 0, new Location(0, "Start", new WorldPosition(0, 1), "Outpost") },
                { 1, new Location(1, "North Pass", new WorldPosition(1, 0), "Outpost") },
                { 2, new Location(2, "South Pass", new WorldPosition(1, 2), "Outpost") },
                { 3, new Location(3, "Goal", new WorldPosition(2, 1), "Outpost") }
            };

            var connections = new List<Connection>
            {
                // Expensive direct route
                new Connection(0, 3, 20),

                // Cheapest alternative route (0 -> 1 -> 3 = 7)
                new Connection(0, 1, 3),
                new Connection(1, 3, 4),

                // Second-best alternative route (0 -> 2 -> 3 = 10)
                new Connection(0, 2, 5),
                new Connection(2, 3, 5)
            };
            #endregion

            return new WorldData(locations, connections);
        }
        // These tests document later assignment contracts without making the
        // untouched baseline red. Remove Ignore when you reach the named phase.

        [Test/*, Ignore("Phase 3: enable when implementing the binary min-heap priority queue.")*/]
        public void PriorityQueue_EnqueueAndDequeue_ReturnsLowestPriorityFirst()
        {
            var queue = new MinPriorityQueue<string>();
            queue.Enqueue("expensive", 9);
            queue.Enqueue("cheap", 2);
            queue.Enqueue("medium", 5);
            Assert.That(queue.Dequeue().Item, Is.EqualTo("cheap"));
        }

        #region A* TESTS
        [Test]
        public void AStar_StartEqualsGoal_ReturnsZeroCostRoute()
        {
            var finder = new AStarRouteFinder(WorldDatasetFactory.CreateSmall());
            RouteResult result = finder.FindRoute(0, 0, new HashSet<int>());
            Assert.That(result.Found, Is.True);
            Assert.That(result.LocationIds, Is.EqualTo(new[] { 0 }));
            Assert.That(result.TotalCost, Is.EqualTo(0));
            Assert.That(result.ExpandedCount, Is.EqualTo(0));
        }

        [Test]
        public void AStar_GoalEqualsNonTraversable_ReturnsNonFoundRoute()
        {
            WorldData world = WorldDatasetFactory.CreateSmall();
            var routeFinder = new AStarRouteFinder(world);
            int startID = 0;
            int goalID = world.Locations.Count - 1;

            var nonTraversableNodes = new HashSet<int>();
            nonTraversableNodes.Add(goalID);

            RouteResult result = routeFinder.FindRoute(startID, goalID, nonTraversableNodes);
            Assert.That(result.Found, Is.False);
            Assert.That(result.LocationIds, Is.EqualTo(Array.Empty<int>()));
            Assert.That(result.TotalCost, Is.EqualTo(0));
            Assert.That(result.ExpandedCount, Is.EqualTo(0));
        }

        [Test]
        public void AStar_ChoosesCheaperMultiHop_ReturnsFoundRouteWithPredeterminedCost()
        {
            var world = CreateAlternativeRoutesWorld();
            var finder = new AStarRouteFinder(world);

            RouteResult result = finder.FindRoute(0, 3, new HashSet<int>());

            Assert.That(result.Found, Is.True);
            Assert.That(result.TotalCost, Is.EqualTo(7));
            Assert.That(result.LocationIds, Is.EqualTo(new[] { 0, 1, 3 }));
        }

        [Test]
        public void AStar_WhenOptimalNodeIsNonTraversable_DetoursToNextBestRoute()
        {
            // Blocking node 1 forces A* to abandon the cost-7 route and take 0 -> 2 -> 3 (cost 10)
            var world = CreateAlternativeRoutesWorld();
            var finder = new AStarRouteFinder(world);
            var blocked = new HashSet<int> { 1 };

            RouteResult result = finder.FindRoute(0, 3, blocked);

            Assert.That(result.Found, Is.True);
            Assert.That(result.TotalCost, Is.EqualTo(10));
            Assert.That(result.LocationIds, Is.EqualTo(new[] { 0, 2, 3 }));
        }

        [Test]
        public void AStar_WhenAllAlternativeNodesBlocked_FallsBackToExpensiveDirectEdge()
        {
            // Blocking both 1 and 2 forces A* to use the direct cost-20 edge (0 -> 3)
            var world = CreateAlternativeRoutesWorld();
            var finder = new AStarRouteFinder(world);
            var blocked = new HashSet<int> { 1, 2 };

            RouteResult result = finder.FindRoute(0, 3, blocked);

            Assert.That(result.Found, Is.True);
            Assert.That(result.TotalCost, Is.EqualTo(20));
            Assert.That(result.LocationIds, Is.EqualTo(new[] { 0, 3 }));
        }
        #endregion

        #region REACHABILITY TESTS

        [Test/*, Ignore("Phase 5: enable when implementing bounded reachability.")*/]
        public void Reachability_ZeroBudget_IncludesStartAtZeroCost()
        {
            var query = new ReachabilityQuery(WorldDatasetFactory.CreateSmall());
            var result = query.FindReachable(0, 0);
            Assert.That(result.ContainsKey(0), Is.True);
            Assert.That(result[0], Is.EqualTo(0));
        }

        [Test]
        public void Reachability_WhenBudgetIsNegative_ReturnsEmptyDictionary()
        {
            var world = CreateAlternativeRoutesWorld();
            var query = new ReachabilityQuery(world);

            IReadOnlyDictionary<int, int> reachable = query.FindReachable(startLocationId: 0, maxTravelCost: -1);

            Assert.That(reachable, Is.Empty);
        }

        [Test]
        public void Reachability_WhenBudgetIsZero_ReturnsOnlyStartNodeAndZeroCostNeighbors()
        {
            #region Custom Test World Creation
            var locations = new Dictionary<int, Location>
            {
                { 0, new Location(0, "Hub", new WorldPosition(0, 0), "Outpost") },
                { 1, new Location(1, "Free Annex", new WorldPosition(0, 1), "Outpost") },
                { 2, new Location(2, "Paid Outpost", new WorldPosition(1, 0), "Outpost") }
            };

            var connections = new List<Connection>
            {
                new Connection(0, 1, 0), // Zero-cost edge
                new Connection(0, 2, 1)  // Cost 1 edge (exceeds budget 0)
            };
            #endregion

            var world = new WorldData(locations, connections);
            var query = new ReachabilityQuery(world);

            IReadOnlyDictionary<int, int> reachable = query.FindReachable(startLocationId: 0, maxTravelCost: 0);

            Assert.That(reachable.Count, Is.EqualTo(2));
            Assert.That(reachable[0], Is.EqualTo(0));
            Assert.That(reachable[1], Is.EqualTo(0));
            Assert.That(reachable.ContainsKey(2), Is.False);
        }

        [Test]
        public void Reachability_IncludesLocationsAtExactBudgetBoundary_AndExcludesTooExpensive()
        {
            // Costs from 0: node 1 = 3, node 2 = 5, node 3 = 7
            var world = CreateAlternativeRoutesWorld();
            var query = new ReachabilityQuery(world);

            // Exact budget of 7 should include node 3 (cost 7)
            IReadOnlyDictionary<int, int> atExactBudget = query.FindReachable(startLocationId: 0, maxTravelCost: 7);
            Assert.That(atExactBudget.ContainsKey(3), Is.True);
            Assert.That(atExactBudget[3], Is.EqualTo(7));

            // Budget of 6 (one below optimal cost 7) must exclude node 3, even though direct edge exists
            IReadOnlyDictionary<int, int> oneBelowBudget = query.FindReachable(startLocationId: 0, maxTravelCost: 6);
            Assert.That(oneBelowBudget.Keys, Is.EquivalentTo(new[] { 0, 1, 2 }));
            Assert.That(oneBelowBudget.ContainsKey(3), Is.False);
        }

        [Test]
        public void Reachability_WhenStartIdIsOutOfBounds_ReturnsEmptyDictionary()
        {
            // Note: In ReachabilityQuery.FindReachable, checking _adjecencyList[startLocationId]
            // before bounds-checking startLocationId will throw IndexOutOfRangeException for -1 or >= Length.
            var world = CreateAlternativeRoutesWorld();
            var query = new ReachabilityQuery(world);

            IReadOnlyDictionary<int, int> reachable = query.FindReachable(-1, 10);

            Assert.That(reachable.Count, Is.EqualTo(0));
        }
        #endregion

        #region DJIKSTRA TESTS
        [Test]
        public void Dijkstra_GoalIsDisconnected_ReturnsNonFoundRoute()
        {
            #region Custom Test World Creation
            var locations = new Dictionary<int, Location>
            {
                {0, new Location(0, "Island A - North", new WorldPosition(0, 0), "Outpost") }, 
                {1, new Location(1, "Island A - South", new WorldPosition(0, 1), "Outpost") }, 
                {2, new Location(2, "Island B - North", new WorldPosition(10, 10), "Outpost") }, 
                {3, new Location(3, "Island B - South", new WorldPosition(10, 11), "Outpost") }
            };
            var connections = new List<Connection>()
            {
                new Connection(0, 1, 5), 
                new Connection(1, 0, 5),
                new Connection(2, 3, 5),
                new Connection(3, 2, 5)
            };
            #endregion

            WorldData world = new WorldData(locations, connections);

            var routeFinder = new DijkstraRouteFinder(world);

            RouteResult result = routeFinder.FindRoute(0, 3);

            Assert.That(result.Found, Is.False);
            Assert.That(result.LocationIds, Is.Empty);
            Assert.That(result.TotalCost, Is.EqualTo(0));
            Assert.That(result.ExpandedCount, Is.EqualTo(2));
        }

        [Test]
        public void Dijkstra_ConnectionIsOneWay_ReturnsNonFoundRoute()
        {
            var locations = new Dictionary<int, Location>
            {
                { 0, new Location(0, "Cliff Top", new WorldPosition(0, 0), "Outpost") },
                { 1, new Location(1, "Cliff Bottom", new WorldPosition(0, 1), "Outpost") }
            };
            var connections = new List<Connection>
            {
                new Connection(0, 1, 3),
            };

            var world = new WorldData(locations, connections);
            var routeFinder = new DijkstraRouteFinder(world);

            RouteResult result = routeFinder.FindRoute(1, 0);

            Assert.That(result.Found, Is.False);
            Assert.That(result.LocationIds, Is.Empty);
            Assert.That(result.TotalCost, Is.EqualTo(0));
            Assert.That(result.ExpandedCount, Is.EqualTo(1));
        }

        [Test/*, Ignore("Phase 4: enable when implementing Dijkstra route finding.")*/]
        public void Dijkstra_StartEqualsGoal_ReturnsZeroCostRoute()
        {
            var finder = new DijkstraRouteFinder(WorldDatasetFactory.CreateSmall());
            RouteResult result = finder.FindRoute(0, 0);
            Assert.That(result.Found, Is.True);
            Assert.That(result.LocationIds, Is.EqualTo(new[] { 0 }));
            Assert.That(result.TotalCost, Is.EqualTo(0));
            Assert.That(result.ExpandedCount, Is.EqualTo(0));
        }

        [Test]
        public void Dijkstra_ChoosesCheaperMultiHopRoute_ReturnsFoundRouteWithPredeterminedCost()
        {
            var world = CreateAlternativeRoutesWorld();
            var finder = new DijkstraRouteFinder(world);

            RouteResult result = finder.FindRoute(0, 3);

            Assert.That(result.Found, Is.True);
            Assert.That(result.TotalCost, Is.EqualTo(7));
            Assert.That(result.LocationIds, Is.EqualTo(new[] { 0, 1, 3 }));
        }
        #endregion
    }
}
