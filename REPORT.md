# REPORT.md - Individual Assignment

Keep this concise and specific to your implementation.

## 1. Initial analysis

- How are locations and connections represented in the baseline?
Locations are represented as 4 variable types, consisting of an id, a name, a world position and the type of location
Connections are represented by 3 variables of type int, the first being a from-location id, and the second being to-location id, and third travel cost.
The locations are represented in 2 lists in WorldData that are initialized in the RunBaseline() method in Program.cs and also in WorldDatasetFactory.CreateGrid(int width, int height)

- Which operations appear frequent?
The creation of new locations and connections happen multiple times to create the world, the vast number of dequeuing and enqueuing in the priority queue means that is also frequent, and 

- Which operation(s) do you predict may scale poorly?
Some operations that may scale poorly are, CreateSmall() method of sample world factory, this method creates hard coded locations and connections, meaning it’s not scalable at all. 
Edit: also, the CreateGrid() method in world dataset factory scales properly, but when it gets to a certain point, like for example 40000 locations, then this will cause some considerable FPS drops if making a Unity visualization. First of all creating this many locations and connections allocates around 2.2MB of memory in the start frame (Not very dangerous since it only causes a little longer load time (meaning in the start method of a Unity scene) and not a complete stutter in gameplay, but still valuable time-loss, so it wouldn’t be a crazy big problem in a Game, but when talking about algorithms and Big-O and such, this could be pretty bad), Second of all having 40000 Locations (and + about 40000*4 Connections) in a Unity scene will cause severe amounts of batches, and extreme amounts of draw-calls, making it impossible to use regular GameObject’s when trying to preserve acceptable framerates and need to move over to other ways of having objects or meshes in the scene, like for example using entity structures and such.
Operations that scale very poorly in the baseline are FindLocationById(int id), this method scans the list in the created WorldData.Locations List linearly, which means that if the amount of location doubles the amount of work also doubles; O(V).
The FindOutgoingConnections(int fromLocationId) also scales very poorly since the amount of connections are waaay more than locations and this will have a worse scaling but still; O(E). This way of checking for outgoing connections is very bad since it HAS to check through every connection in the connections list in WorldData. 

- What will you investigate first?
I will investigate the create world methods and see how they are constructed in the world dataset factory class. And I will also check out how to optimize the FindLocationById() method, and also the FindOutgoingConnections(). 

## 2. Data representation

Describe important changes/retained choices, operations affected, and time/memory trade-offs.

*Dijkstra*
I chose to use an adjacency list dictionary when storing the outgoing connections from the locations. I chose this representation since I needed instant look-up for Location ID’s, trading it for high memory overhead and CPU cache locality. Edit: This decision was changed later on because I realized that the creation of the grids/worlds in WorldDatasetFactory was creating a grid of locations with id’s ranging from 0 – x (int length), this made it possible to use an indexed list instead of a dictionary, i.e an array of lists instead, trading the possibility to create a grid with random id’s, and having to keep the world creation as is. This method of storing the adjacency list saves the algorithm ~0.90MB of memory allocation when creating the adjacency list.
This new optimization only works if the locations in the grid have id’s that can be indexed, like {0, 1, 2, 3, 4, 5, 6..} otherwise if they are creted like {0, 245, 13, 50} the array would first of all scale up to the highest index number, so ‘245’ in this case, and thus making the size of the array way bigger than it needs too, or if we remove the resize preparation in the constructors of Reachability and Dijkstra, the algorithms will return a index out of range exception. 

*Dijkstra*
I also used to have a HashSet for checking for “_validLocationIDs” meaning locations existing in the world data that was initialized in the constructor. But I removed this to save memory since this was causing redundant/double checks because the adjacency list was already checking if the locations existed in the world data and therefore making the whole entire HashSet redundant. I check if the location has connections before running through the location’s connections, thus doing the same thing that the “_validLocationIDs” Set was doing. 

*Reachability Query*
Because I switched the way I store the adjacency list I needed a way to check if the start location is out of bounds of the array, since that isn’t really a problem with dictionaries. The problem itself is that I have to store the id’s of locations as indices in the array, otherwise the whole thing breaks, so I also made a preparation for this by – in the constructor (of both Dijkstra and Reachability Query) – putting a check for the highest id, and it determines the size of the array.
The test then creates a world and tries to reach nodes within max travel cost of 10 and sends the start location ‘-1’, which will return an empty dictionary. 

I also made other tests for edge cases but they will not be shown here because I’m really tired of typing at this point.


## 3. Algorithms and adaptation

Explain your route finder at a useful high level and how the travel-budget query adapts the same ideas.

My Dijkstra’s algorithm uses a MinPriorityQueue<int>, an adjacency list using a Dictionary<int, List<Connection>> (Edit: later changed to an array of lists: List<Connection>[]) 

The algorithm – when running – keeps a dictionary of found costs<int, int> with <id, cost>, another dictionary of previousNodes<int, int> with the key of which id can get to the current id represented as the value <edge.tolocationid, currentid>. 

It also keeps a MinPriorityQueue<int> with Entries containing an int id, and an int priority, and the priority being the cost of reaching the location with the entry’s id. Speaking of cost, the costs are only updated if the cost of getting to the next location costs less than the already existing cost, by checking for this we will always get closer to the goal, and never move away from it farther than we have to. 

The reconstruction of the path – once a shortest route is found – works by getting the previous nodes dictionary (previousNodes is a Dictionary<int, int> each KeyValuePair as <to location id, current id> meaning it stores, so it basically tells the algorithm which nodes are connected to a particular node, and this making it possible for multiple nodes being connected to another one) the start id, and goal id, then it iterates through all previous nodes and adds every location visited in previous nodes dictionary to a list of all location id’s, then reverses the list, since the iteration starts from the goal id. 

The Reachability query builds atop of Dijkstra’s route finder by introducing a max travel cost, because of this new variable we could possibly end up not being able to reach the goal location, but in the reachability query we don’t even care about the end location, we only care about getting to locations under a certain cost. This could be implemented into the Dijkstra’s algorithm, using it for evaluating which step to take next, potentially checking if the path that we want to take fits under the max cost and if not check for other parameters, although this could introduce increased load time, and way more calculation, it could be useful for an enemy that tries to attack towards you but it is a certain distance away from you, the attack has a range of int maxTravelCost, and it increases after each attack, and when it has attacked 3 times the maxTravelCost is long enough to hit the player, it could be an interesting way of doing this sort of thing, but there are probably other ways of doing it. Or maybe the same type of situation but only an enemy that is patrolling och tracking the player based on the maxTravelCost, if the maxTravelCost is exceeded when trying to track the player the enemy starts patrolling instead.

The reachability query basically works the same way of Dijkstra’s algorithm, but adds more to it and has a different return type. The reachability query returns a dictionary consisting of only locations that are reachable, meaning cost less or equally as much as the entered maxTravelCost. The algorithm does not care for the goal locations, and therefore returns all possible locations and their travel cost that fits in the budget. The algorithm also uses a HashSet to see which nodes have already been visited, because why would we need to check the same node twice? It also has a min priority queue, just like Dijkstra and A*. The algorithm only enters a new location if the new location found is less or equal to the max cost, and if the location is not already visited.

## 4. Complexity
| Operation | Your representation/algorithm | Complexity | Short explanation | 

| --- | --- | --- | --- | 




| Find location by ID | AdjecencyList.ContainsKey() (A*) | ~O(1)  | Trade for increased memory usage for instant look up |

| Get outgoing connections | AdjecencyList.TryGetValue() (A*)| O(1) | Trade for increased memory usage for instant look up |

| Find location by ID | AdjecencyList[CurrentID] (Array of lists) (Dijkstra) | ~O(1)  | Trade for having to have a sorted array by location id’s, has to have id’s sorted {0, 1, 2, 3, 4, 5, 6}, no id is ever less or nor more than 1 higher than the previous id |

| Get outgoing connections | for each edge in AdjecencyList[CurrentID] (Array of lists) (Dijkstra)| O(1) | Trade for having to have a sorted array by location id’s, has to have id’s sorted {0, 1, 2, 3, 4, 5, 6}, no id is ever less or nor more than 1 higher than the previous id |

| Priority queue enqueue | Adding to a list and using and recursevly calling “Swap” item to swap and organize the heap | O(log n) | 
Creates an entry with an item<T> and its priority and adds it to the queue then calls “Move-Up” and recursively moves the entry up the heap based on its priority |

| Priority queue dequeue-min | Return type Entry(Item<T>, int priority) and gets the root item and index of the last item and recursively moves down the heap to return the root item | O(Log n) | Dequeue calls the move down method, which in turn checks if the index next to the input index is smaller or larger in priority, or if the given index has smaller priority |

| Route finding | Dijkstra | O((E + V)Log V) | E = number of edges (connections), V = number of vertices (locations) and Log V for the number of iterations in the priority queue |

| Reachability finding | ReachabilityQuery (~Dijkstra) | O((E + V)Log V) | E is the amount of reachable edges in the world, and V the amount of reachable nodes/vertices and log V is the amount of operations it takes to restore the heap invariant in the min priority queue | 

| World Creation | Two-pass Construction | O(W * H) (O(V)) | The complexity is entirely based on the width and height that we input when calling CreateGrid() | 


## 5. Correctness and testing

Important tests/edge cases and what they demonstrate.

PriorityQueue_EnqueueAndDequeue_ReturnsLowestPriorityFirst()
This test demonstrates that the MinPriorityQueue returns the right entry when calling dequeue.

Dijkstra_StartEqualsGoal_ReturnsZeroCostRoute()
This test demonstrates that the start location is the same as the goal location, returns found as true, locationIDs list with 1 element of id 0, total cost of 0 and expanded count of 0.

Reachability_ZeroBudget_IncludesStartAtZeroCost()
Demonstrates that the reachability query can do basically the same thing as the last test for the Dijkstra route finder, but still different. It checks the edge case of the max travel cost being 0 and therefore returns a dictionary with 1 element being id 0 and cost 0.

## 6. Performance investigation

Record reproducible before/after measurements for loading/preparation, repeated runtime work, and relevant allocation behavior. Explain what changed and why.

When running a linear Find location by id scan over 2500 locations and 5000 iterations, the elapsed time is: 45.13 ms with an average of 0.0090 ms per iteration, the world creation time was ~1.48 ms. 

Then when optimizing look-up time by switching to a dictionary lookup over the same amount of locations and same amount of iterations, the elapsed time is: 0.24 ms with an average time per iteration of 0.0000 ms, and world creation time of ~1.48 ms. 

I ran all these tests using System.Diagnostics.StopWatch, calling StartNew() when doing a new test and Stop() when the test is finished. 

I made a minor change in priority queue that didn’t necessarily impact performance, but it changed how the algorithm works. The change was that the return type of Dequeue was a generic type (T) and I changed it to be a type ‘Entry’ and this greatly impacted the use cases of the Priority queue. When returning an entry instead of the generic type I can now get the Priority of the Entries in the Queue.

I made a change in the Dijkstra’s algorithm that impacted the amount of memory used to create the world data. The change was that I removed a HashSet called ‘_validLocationIDs’, this Set was completely unnecessary since all locations in the adjacency list are valid, and if they weren’t valid the algorithm would have already caught that and skipped said location/node.

When running the Unity visualization with the WorldDatasetFactory.CreateLarge(), the amount of batches were around 150000, and the same for drawcalls, this impacted performance like crazy, the change I made was to not create visual representations for the connections, as well as change the materials used for the nodes/locations, and marked them all as static, this saved some amount of batches but not a lot. So using the CreateLarge() version of world creation is not recommended when doing the visualization…

I figured out that I could optimize the adjacency list creation by utilizing an array of List<Connection> instead of a dictionary, this decreased the memory allocation of the adjacency list creation by A LOT. I ran a test to see how much memory the dictionary creation allocated when creating a large world (CreateLarge()) and it returned that it allocated roughly ~4.75 MB of memory, while the Array of List<Connection> gave a memory allocation output of around 3.85 MB, this change means that instant look-up may be corrupted if the creation of id’s change its definition, but if it stays as is than this is simply a massive performance increase without downsides, but ofcourse the downside is exactly that, the world cannot contain id’s out of range of the world. Since we have removed the dictionary hashing overhead, we have cut 0.90MB of allocated memory.
The time of which it takes for the Dijkstra algorithm to run is essentially unchanged, or even improved in best cases.

When testing for memory allocations I use System.GC, simply creating a long and setting it to GC.GetAllocatedBytesForCurrentThread() before the world creation, then creating a new long and setting that to the same thing, then I calculated the total allocated memory for the world creation by subtracting the memory after creation by the memory before, thus getting the memory allocated for that test, and then starting the route finder (Dijkstra) and after is setting another long to the same thing again, and subtracting the memory after world creation by the memory after the routefinder, thus getting the total allocated memory for that test as well. 

## 7. Development reflection

Describe one bug, wrong assumption, failed approach, or surprising result and how you investigated it.

I assumed that in order to know if the start and goal locations were valid, that I needed to have some sort of like HashSet that stores all of the locations in the world, and then check if the entered start location exists in the world through this HashSet. This is non-destructive and works perfectly fine, but the thing is when I then looked through the code I realized that by just checking in the adjacency list by doing “AdjecencyList.ContainsKey()” I could do the exact same thing, since the adjacency list already contains all valid locations in the world, and thus making the HashSet unnecessary and redundant. 

## 8. Tools and AI assistance

Briefly describe relevant assistance, if any.

I wanted to know what different ways there were to check for outgoing edges, so I used Google’s “AI-Mode” so that I didn’t need to look through multiple websites just to find some simple information that I could get almost instantly with the “AI-Mode”.

I used AI generation for the baseline tests in Program.cs, but not the “RunDijkstra()” or “RunReachabilityQuery()” or “RunAStar()” those were handmade.

I Used AI assistance in finding bugs when I was creating the routefinders, and most of the time the bugs were just that I had made a stupid syntax error, or like missing to put an ! in an if-statement…

## 9. VG extension

Only if attempting VG: heuristic, traversal constraints, correctness, and Dijkstra/A* comparison.

I’m using Manhattan Distance as Heuristic for my A* algorithm’s cost calculation, the calculation looks a bit like this: 
Math.Abs(fromNode.position.x – toNode.position.x) + Math.Abs(fromNode.position.y – toNode.position.y)

The Heuristic calculation is admissible in the case of grid worlds because if we would have random positions of the world, kind of like an open world map, we would not be able to use Manhattan, but in this case the distance between every node is the same and the world is built like a grid, so the Manhattan distance would be the best case here, as for the open-world case, the Euclidean distance would fit better.

Im using an ISet for listing blocked Location ID’s, and evaluating if a node can be traversed before actually calculating any costs, not to mention before traversing at all, this is in order to calculate some sort of maze-like world graph, or in order to decide if a place of the world should not be able to be traversed (sort of implicit but still). 

The difference between A* and Dijkstra can be measured in multiple different ways, but the difference between the way A* evaluates which node to traverse to is calculated based on the accumulated cost so far + the next edge’s travel cost + the Manhattan distance to the goal node from the current node, where as Dijkstra’s algorithm only gets the edge’s travel cost + the accumulated travel cost so far.

One key difference between my Dijkstra and my A* is the fact that my Dijkstra evaluates whether the algorithm has already visited a node and cannot move back to said node, my A* doesn’t do that but instead checks if the next node being checked has a worse total g-cost than the current node, and is able to move backward if there are blocked nodes, the blocked nodes implementation was not made in my Dijkstra’s route finder.

When running the Dijkstra and A* with CreateMedium() the expandedCount is different, meaning they found the target, but had to check different amounts of nodes. Dijkstra expanded 2316 times, while A* expanded 1798 times, so a significant amount of expansions saved. Both algorithms return the same amount of “jumps” meaning traversed nodes, and the same travel cost. The run time difference of A* and Dijkstra in my case is that A* is around ~2-4ms, depending on background processes, the system and some other stuff.. The difference comes from the amount of calculations of the Manhattan distance becomes more expensive than the time saved by going through less nodes, which is what A* does.

