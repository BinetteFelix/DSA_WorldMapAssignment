using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FrontierDispatch.Core;

if (args.Length > 0 && args[0] == "--baseline")
{
    RunBaseline();
    return;
}
if (args.Length > 0 && args[0] == "--baselinePlus")
{
    RunBaselinePlusInstantLookUp();
    return;
}
if (args.Length > 0 && args[0] == "--baselineRouteFinderDijkstra")
{
    RunRouteFinderBaseLineDijkstra();
    return;
}

WorldData world1 = WorldDatasetFactory.CreateXS();
MinPriorityQueue<Location> locQ = new MinPriorityQueue<Location>();


for (int i = 0; i < world1.Locations.Count; i++)
{
    Random rand = new Random();
    int randomPrio = rand.Next(1, 26);

    locQ.Enqueue(world1.Locations[i], randomPrio);
    Console.WriteLine($"ID: {world1.Locations[i].Id} Prio: {randomPrio}");
}

Console.WriteLine($"\nTotal count after insertion: {locQ.Count}");
Console.WriteLine($"Peek lowest priority: '{locQ.Peek().Id}'\n");

while (locQ.Count > 0)
{
    Location loc = locQ.Dequeue().Item;
    Console.WriteLine($"-> processed: {loc.Id}");
}

RunDijkstra();
RunReachabilityQuery();
RunAStar();
RunBaselinePlusInstantLookUp();
RunRouteFinderBaseLineDijkstra();
RunDijkstraMemoryBenchmark();

static void RunDijkstra()
{
    Console.WriteLine();
    Console.WriteLine("--- Dijkstra's route finder ---");
    Console.WriteLine();
    var worlds = new (string Name, WorldData Data)[]
    {
        ("XS", WorldDatasetFactory.CreateXS()),
        ("Small", WorldDatasetFactory.CreateSmall()),
        ("Medium", WorldDatasetFactory.CreateMedium()),
        ("Large", WorldDatasetFactory.CreateLarge())
    };

    foreach (var (name, world) in worlds)
    {
        WorldQueries queries = new WorldQueries(world);
        DijkstraRouteFinder routeFinder = new DijkstraRouteFinder(world);
        int startID = 0;
        int goalID = world.Locations.Count - 1;

        RouteResult result = routeFinder.FindRoute(startID, goalID);

        Console.WriteLine($"{name}: Found = {result.Found}, Cost = {result.TotalCost}, Expanded = {result.ExpandedCount}");
        Console.WriteLine($"Jumps = {result.LocationIds.Count}");
        if (result.LocationIds.Count <= 25)
        {
            Console.WriteLine($"    Path: {string.Join("->", result.LocationIds)}");
            Console.WriteLine();
        }
    }
}
static void RunReachabilityQuery()
{
    Console.WriteLine();
    Console.WriteLine("--- Reachability finder ---");
    Console.WriteLine();

    var worlds = new (string Name, WorldData Data, int budget)[]
    {
        ("XS", WorldDatasetFactory.CreateXS(), 10),
        ("XS", WorldDatasetFactory.CreateXS(), 16),
        ("Small", WorldDatasetFactory.CreateSmall(), 0),
        ("Small", WorldDatasetFactory.CreateSmall(), 10),
        ("Small", WorldDatasetFactory.CreateSmall(), 30),
        ("Medium", WorldDatasetFactory.CreateMedium(), 50),
        ("Large", WorldDatasetFactory.CreateLarge(), 100)
    };

    foreach (var (name, world, budget) in worlds)
    {
        ReachabilityQuery routeFinder = new ReachabilityQuery(world);

        IReadOnlyDictionary<int, int> results = routeFinder.FindReachable(0, budget);

        Console.WriteLine($"{name}: Reachable Count = {results.Count}");
        if (results.Count <= 25)
        {
            var formatted = string.Join(", ", results.OrderBy(kvp => kvp.Key).Select(kvp => $"{kvp.Key} : {kvp.Value}"));
            Console.WriteLine($"  Nodes (ID : Cost): {formatted}");
        }
    }
}
static void RunAStar()
{
    Console.WriteLine();
    Console.WriteLine("--- A* route finder ---");
    Console.WriteLine();
    var worlds = new (string Name, WorldData Data)[]
    {
        ("XS", WorldDatasetFactory.CreateXS()),
        ("Small", WorldDatasetFactory.CreateSmall()),
        ("Medium", WorldDatasetFactory.CreateMedium()),
        ("Large", WorldDatasetFactory.CreateLarge())
    };

    foreach (var (name, world) in worlds)
    {
        WorldQueries queries = new WorldQueries(world);
        AStarRouteFinder routeFinder = new AStarRouteFinder(world);
        int startID = 0;
        int goalID = world.Locations.Count - 1;

        RouteResult result = routeFinder.FindRoute(startID, goalID, new HashSet<int>());

        Console.WriteLine($"{name}: Found = {result.Found}, Cost = {result.TotalCost}, Expanded = {result.ExpandedCount}");
        Console.WriteLine($"Jumps = {result.LocationIds.Count}");
        if (result.LocationIds.Count <= 25)
        {
            Console.WriteLine($"    Path: {string.Join("->", result.LocationIds)}");
            Console.WriteLine();
        }
    }
}
static void RunBaseline()
{
    Console.WriteLine();
    // Build the dataset before timing the repeated runtime queries.
    // This baseline therefore measures query work, not world construction.
    WorldData world = WorldDatasetFactory.CreateMedium();
    var queries = new WorldQueries(world);

    // The checksum provides a stable sanity check that the workload still
    // produces equivalent results after implementation changes.
    int checksum = 0;

    const int iterations = 5000;

    var sw = Stopwatch.StartNew();

    for (int i = 0; i < iterations; ++i)
    {
        // Deterministically visit many different location IDs rather than
        // repeatedly querying the same location.
        int id = (i * 997) % world.Locations.Count;

        Location? location = queries.FindLocationByIdLinear(id);
        checksum += location?.Id ?? 0;
        checksum += queries.FindOutgoingConnections(id).Count;
    }

    sw.Stop();

    Console.WriteLine("-----Baseline world-query workload-----");
    Console.WriteLine($"Locations: {world.Locations.Count}");
    Console.WriteLine($"Connections: {world.Connections.Count}");
    Console.WriteLine($"Iterations: {iterations}");
    Console.WriteLine($"Elapsed: {sw.ElapsedMilliseconds} ms");
    Console.WriteLine($"Checksum: {checksum}");
    Console.WriteLine("Absolute time varies by machine. Keep this result for before/after comparison.");
}
static void RunBaselinePlusLinearScan()
{
    // A. Measure world creation.
    var worldCreateSW = Stopwatch.StartNew();
    WorldData world = WorldDatasetFactory.CreateMedium();
    worldCreateSW.Stop();

    // A. Measure WorldQueries preparation separately.
    const int preparationIterations = 100;

    // Warm-up before measurement.
    WorldQueries queries = new WorldQueries(world);

    GC.Collect();
    GC.WaitForPendingFinalizers();
    GC.Collect();

    var preparationSW = Stopwatch.StartNew();

    for (int i = 0; i < preparationIterations; i++)
    {
        queries = new WorldQueries(world);
    }
    preparationSW.Stop();

    double preparationElapsed = preparationSW.Elapsed.TotalMilliseconds;
    double preparationAverage = preparationElapsed / preparationIterations;

    // B. Measure repeated runtime queries.
    int checksum = 0;
    const int iterations = 5000;

    var sw = Stopwatch.StartNew();

    for (int i = 0; i < iterations; ++i)
    {
        int id = (i * 997) % world.Locations.Count;

        Location? location = queries.FindLocationByIdLinear(id);
        checksum += location?.Id ?? 0;
        //checksum += queries.FindOutgoingConnections(id).Count;
    }

    sw.Stop();

    double elapsedMs = sw.Elapsed.TotalMilliseconds;
    double averageMs = elapsedMs / iterations;

    // Keep the final query object alive through the measurements.
    GC.KeepAlive(queries);

    Console.WriteLine();
    Console.WriteLine("Frontier Dispatch - Performance workload");
    Console.WriteLine("----------------------------------------");

    Console.WriteLine("A. Loading and preparation");
    Console.WriteLine($"World creation: {worldCreateSW.Elapsed.TotalMilliseconds:F2} ms");
    Console.WriteLine($"WorldQueries preparation iterations: {preparationIterations}");
    Console.WriteLine($"Preparation elapsed: {preparationElapsed:F2} ms");
    Console.WriteLine($"Average preparation: {preparationAverage:F4} ms");
    Console.WriteLine();

    Console.WriteLine("B. Runtime code efficiency");
    Console.WriteLine($"Locations: {world.Locations.Count:N0}");
    Console.WriteLine($"Connections: {world.Connections.Count:N0}");
    Console.WriteLine($"Iterations: {iterations:N0}");
    Console.WriteLine($"Elapsed: {elapsedMs:F2} ms");
    Console.WriteLine($"Average per iteration: {averageMs:F4} ms");
    Console.WriteLine($"Checksum: {checksum:N0}");
}
static void RunBaselinePlusInstantLookUp()
{
    // A. Measure world creation.
    var worldCreateSW = Stopwatch.StartNew();
    WorldData world = WorldDatasetFactory.CreateMedium();
    worldCreateSW.Stop();

    // A. Measure WorldQueries preparation separately.
    const int preparationIterations = 100;

    // Warm-up before measurement.
    WorldQueries queries = new WorldQueries(world);

    GC.Collect();
    GC.WaitForPendingFinalizers();
    GC.Collect();

    var preparationSW = Stopwatch.StartNew();

    for (int i = 0; i < preparationIterations; i++)
    {
        queries = new WorldQueries(world);
    }
    preparationSW.Stop();

    double preparationElapsed = preparationSW.Elapsed.TotalMilliseconds;
    double preparationAverage = preparationElapsed / preparationIterations;

    // B. Measure repeated runtime queries.
    int checksum = 0;
    const int iterations = 5000;

    var sw = Stopwatch.StartNew();

    for (int i = 0; i < iterations; ++i)
    {
        int id = (i * 997) % world.Locations.Count;

        Location? location = queries.FindLocationByIdInstant(id);
        checksum += location?.Id ?? 0;
        //checksum += queries.FindOutgoingConnections(id).Count;
    }

    sw.Stop();

    double elapsedMs = sw.Elapsed.TotalMilliseconds;
    double averageMs = elapsedMs / iterations;

    // Keep the final query object alive through the measurements.
    GC.KeepAlive(queries);

    Console.WriteLine();
    Console.WriteLine("Frontier Dispatch - Performance workload");
    Console.WriteLine("----------------------------------------");

    Console.WriteLine("A. Loading and preparation");
    Console.WriteLine($"World creation: {worldCreateSW.Elapsed.TotalMilliseconds:F2} ms");
    Console.WriteLine($"WorldQueries preparation iterations: {preparationIterations}");
    Console.WriteLine($"Preparation elapsed: {preparationElapsed:F2} ms");
    Console.WriteLine($"Average preparation: {preparationAverage:F4} ms");
    Console.WriteLine();

    Console.WriteLine("B. Runtime code efficiency");
    Console.WriteLine($"Locations: {world.Locations.Count:N0}");
    Console.WriteLine($"Connections: {world.Connections.Count:N0}");
    Console.WriteLine($"Iterations: {iterations:N0}");
    Console.WriteLine($"Elapsed: {elapsedMs:F2} ms");
    Console.WriteLine($"Average per iteration: {averageMs:F4} ms");
    Console.WriteLine($"Checksum: {checksum:N0}");
}
static void RunRouteFinderBaseLineDijkstra()
{
    // A. Create the world only once.
    var worldCreateSW = Stopwatch.StartNew();
    WorldData world = WorldDatasetFactory.CreateLarge();
    worldCreateSW.Stop();

    // B. Prepare WorldQueries only once.
    var preparationSW = Stopwatch.StartNew();
    var queries = new WorldQueries(world);
    preparationSW.Stop();

    // C. Both algorithms share the same WorldQueries object.
    
    var reachability = new ReachabilityQuery(world);

    var dijkstra = new DijkstraRouteFinder(world);

    // D. Measure Dijkstra.
    var dijkstraSW = Stopwatch.StartNew();
    var routeResult = dijkstra.FindRoute(0, world.Locations.Count - 1);

    dijkstraSW.Stop();

    // E. Measure Reachability.
    const int maxTravelCost = 1000;

    var reachabilitySW = Stopwatch.StartNew();

    var reachableLocations = reachability.FindReachable(0, maxTravelCost);

    reachabilitySW.Stop();

    // Keep results alive through measurements.
    GC.KeepAlive(routeResult);
    GC.KeepAlive(reachableLocations);
    GC.KeepAlive(queries);

    Console.WriteLine();
    Console.WriteLine("Frontier Dispatch - Dijkstra Performance");
    Console.WriteLine("---------------------------------------------");
    Console.WriteLine("A. World creation and preparation");
    Console.WriteLine($"World creation: {worldCreateSW.Elapsed.TotalMilliseconds:F2} ms");
    Console.WriteLine($"WorldQueries preparation: {preparationSW.Elapsed.TotalMilliseconds:F2} ms");
    Console.WriteLine();

    Console.WriteLine("B. Dijkstra");
    Console.WriteLine($"Elapsed: {dijkstraSW.Elapsed.TotalMilliseconds:F2} ms");
    Console.WriteLine();

    Console.WriteLine("C. Reachability");
    Console.WriteLine($"Maximum travel cost: {maxTravelCost}");
    Console.WriteLine($"Reachable locations: {reachableLocations.Count}");
    Console.WriteLine($"Elapsed: {reachabilitySW.Elapsed.TotalMilliseconds:F2} ms");
    Console.WriteLine();

    Console.WriteLine("D. Shared world information");
    Console.WriteLine($"Locations: {world.Locations.Count:N0}");
    Console.WriteLine($"Connections: {world.Connections.Count:N0}");
    Console.WriteLine("Both algorithms use the same WorldQueries instance.");
}
static void RunDijkstraMemoryBenchmark()
{
    Console.WriteLine("---- Memory Benchmark Dijkstra ----");

    WorldData warmUpWorld = WorldDatasetFactory.CreateLarge();
    _ = new DijkstraRouteFinder(warmUpWorld);

    long bytesBeforeCreation = GC.GetAllocatedBytesForCurrentThread();
    WorldData world = WorldDatasetFactory.CreateLarge();
    long bytesAfterCreation = GC.GetAllocatedBytesForCurrentThread();

    var routeFinder = new DijkstraRouteFinder(world);
    long afterRouteFinder = GC.GetAllocatedBytesForCurrentThread();

    double totalBytesCreation = bytesAfterCreation - bytesBeforeCreation;
    double totalBytesAdjecencyList = afterRouteFinder - bytesAfterCreation;

    Console.WriteLine($"Memory allocated for World Creation: {totalBytesCreation / 1000000:F2} MB");
    Console.WriteLine($"Memory allocated for Adjecency list Creation: {totalBytesAdjecencyList / 1000000:F2} MB");
}