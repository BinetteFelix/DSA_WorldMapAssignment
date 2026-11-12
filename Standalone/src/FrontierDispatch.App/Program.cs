using System;
using System.Diagnostics;
using FrontierDispatch.Core;

if (args.Length > 0 && args[0] == "--baseline")
{
    RunBaseline();
    return;
}

// Small sanity-check application.
// This is separate from the larger performance workload below.
WorldData world = SampleWorldFactory.CreateSmall();
var queries = new WorldQueries(world);

Console.WriteLine("Frontier Dispatch - assignment starter");
Console.WriteLine($"Small demo locations: {world.Locations.Count}");
Console.WriteLine($"Find 734: {queries.FindLocationById(734)?.Name ?? "missing"}");
Console.WriteLine();
Console.WriteLine("Run with --baseline to record the supplied baseline workload.");

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
    Location loc = locQ.Dequeue();
    Console.WriteLine($"-> processed: {loc.Id}");
}

RunRouteFinder();

static void RunRouteFinder()
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
static void RunBaseline()
{
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

        Location? location = queries.FindLocationById(id);
        checksum += location?.Id ?? 0;
        checksum += queries.FindOutgoingConnections(id).Count;
    }

    sw.Stop();

    Console.WriteLine("Baseline world-query workload");
    Console.WriteLine($"Locations: {world.Locations.Count}");
    Console.WriteLine($"Connections: {world.Connections.Count}");
    Console.WriteLine($"Iterations: {iterations}");
    Console.WriteLine($"Elapsed: {sw.ElapsedMilliseconds} ms");
    Console.WriteLine($"Checksum: {checksum}");
    Console.WriteLine("Absolute time varies by machine. Keep this result for before/after comparison.");
}
