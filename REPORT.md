# REPORT.md - Individual Assignment

Keep this concise and specific to your implementation.

## 1. Initial analysis

- How are locations and connections represented in the baseline?
Locations are represented as 4 variable types, consisting of an id, a name, a world position and the type of location
Connections are represented by 3 variables of type int, the first being a from-location id, and the second being to-location id, and third travel cost.
- Which operations appear frequent?
The creation of new locations and connections seem frequent
- Which operation(s) do you predict may scale poorly?
Some operations that may scale poorly are, CreateSmall() method of sample world factory, this method creates hard coded locations and connections, meaning its not scalable at all, and the
- What will you investigate first?
I will investigate the create world methods and see how they are constructed in the world dataset factory class.

## 2. Data representation

Describe important changes/retained choices, operations affected, and time/memory trade-offs.

## 3. Algorithms and adaptation

Explain your route finder at a useful high level and how the travel-budget query adapts the same ideas.

## 4. Complexity

| Operation | Your representation/algorithm | Complexity | Short explanation |
| --- | --- | --- | --- |
| Find location by ID | | | |
| Get outgoing connections | | | |
| Priority queue enqueue | | | |
| Priority queue dequeue-min | | | |
| Route finding | | | |

## 5. Correctness and testing

Important tests/edge cases and what they demonstrate.

## 6. Performance investigation

Record reproducible before/after measurements for loading/preparation, repeated runtime work, and relevant allocation behavior. Explain what changed and why.

## 7. Development reflection

Describe one bug, wrong assumption, failed approach, or surprising result and how you investigated it.

## 8. Tools and AI assistance

Briefly describe relevant assistance, if any.

## 9. VG extension

Only if attempting VG: heuristic, traversal constraints, correctness, and Dijkstra/A* comparison.

