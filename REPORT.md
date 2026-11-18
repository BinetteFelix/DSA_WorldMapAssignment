# REPORT.md - Individual Assignment

Keep this concise and specific to your implementation.

## 1. Initial analysis

- How are locations and connections represented in the baseline?
Locations are represented as 4 variable types, consisting of an id, a name, a world position and the type of location
Connections are represented by 3 variables of type int, the first being a from-location id, and the second being to-location id, and third travel cost.

- Which operations appear frequent?
The creation of new locations and connections happen multiple times to create the world, the vast number of dequeuing and enqueuing in the priority queue means that is also frequent, and 

- Which operation(s) do you predict may scale poorly?
Some operations that may scale poorly are, CreateSmall() method of sample world factory, this method creates hard coded locations and connections, meaning it’s not scalable at all. 
Edit: also, the CreateGrid() method in world dataset factory scales properly, but when it gets to a certain point, like for example 40000 locations, then this will cause some considerable FPS drops if making a Unity visualization. First of all creating this many locations and connections allocates around 2.2MB of memory in the start frame (Not very dangerous since it only causes a little longer load time (meaning in the start method of a Unity scene) and not a complete stutter in gameplay, but still valuable time-loss, so it wouldn’t be a crazy big problem in a Game, but when talking about algorithms and Big-O and such, this could be pretty bad), Second of all having 40000 Locations (and + about 40000*4 Connections) in a Unity scene will cause severe amounts of batches, and extreme amounts of draw-calls, making it impossible to use regular GameObject’s when trying to preserve acceptable framerates and need to move over to other ways of having objects or meshes in the scene, like for example using entity structures and such.

- What will you investigate first?
I will investigate the create world methods and see how they are constructed in the world dataset factory class. 

## 2. Data representation

Describe important changes/retained choices, operations affected, and time/memory trade-offs.

*Dijkstra*
I chose to use an adjacency list dictionary when storing the outgoing connections from the locations. I chose this representation since I needed instant look-up for Location ID’s, trading it for high memory overhead and CPU cache locality.

*Dijkstra*
I also used to have a HashSet for checking for “_validLocationIDs” meaning locations existing in the world data that was initialized in the constructor. But I removed this to save memory since this was causing redundant/double checks because the adjacency list was already checking if the locations existed in the world data and therefore making the whole entire HashSet redundant.

## 3. Algorithms and adaptation

Explain your route finder at a useful high level and how the travel-budget query adapts the same ideas.

## 4. Complexity

| Operation | Your representation/algorithm | Complexity | Short explanation | 

| Find location by ID | AdjecencyList.ContainsKey() | O(1) | Instant look up, for increased memory usage |

| Get outgoing connections | AdjecencyList.TryGetValue() | O(1) | Instant look up, for increased memory usage |

| Priority queue enqueue |  |  |  |

| Priority queue dequeue-min |  | O(Log n) |  |

| Route finding | Dijkstra | O((E + V)Log V) | E = number of edges (connections), V = number of vertices (locations) and Log V for the number of iterations in the priority queue |

## 5. Correctness and testing

Important tests/edge cases and what they demonstrate.

## 6. Performance investigation

Record reproducible before/after measurements for loading/preparation, repeated runtime work, and relevant allocation behavior. Explain what changed and why.

## 7. Development reflection

Describe one bug, wrong assumption, failed approach, or surprising result and how you investigated it.

I assumed that in order to know if the start and goal locations were valid, that I needed to have some sort of like HashSet that stores all of the locations in the world, and then check if the entered start location exists in the world through this HashSet. This is non-destructive and works perfectly fine, but the thing is when I then looked through the code I realized that by just checking in the adjacency list by doing “AdjecencyList.ContainsKey()” I could do the exact same thing, since the adjacency list already contains all valid locations in the world, and thus making the HashSet unnecessary and redundant. 

## 8. Tools and AI assistance

Briefly describe relevant assistance, if any.

I wanted to know what different ways there were to check for outgoing edges, so I used Google’s “AI-Mode” so that I didn’t need to look through multiple websites just to find some simple information that I could get almost instantly with the “AI-Mode”.

## 9. VG extension

Only if attempting VG: heuristic, traversal constraints, correctness, and Dijkstra/A* comparison.


