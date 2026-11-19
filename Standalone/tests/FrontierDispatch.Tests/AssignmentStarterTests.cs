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

        [Test/*, Ignore("Phase 5: enable when implementing bounded reachability.")*/]
        public void Reachability_ZeroBudget_IncludesStartAtZeroCost()
        {
            var query = new ReachabilityQuery(WorldDatasetFactory.CreateSmall());
            var result = query.FindReachable(0, 0);
            Assert.That(result.ContainsKey(0), Is.True);
            Assert.That(result[0], Is.EqualTo(0));
        }
    }
}
