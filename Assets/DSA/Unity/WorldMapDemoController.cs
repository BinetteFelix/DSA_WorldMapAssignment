#nullable enable

using FrontierDispatch.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Codice.CM.WorkspaceServer.DataStore.WkTree.WriteWorkspaceTree;

namespace FrontierDispatch.Unity
{
    public sealed class WorldMapDemoController : MonoBehaviour
    {
        public static WorldMapDemoController Instance { get; private set; }
        [SerializeField] private GameObject _nodePrefab;
        [SerializeField] private Material edgeMAT;
        public readonly Dictionary<int, GameObject> Views = new Dictionary<int, GameObject>();
        private WorldData? _world;
        private WorldQueries? _queries;
        private Action<int> onNodeSelected;
        private RouteResult routeResult;
        private string _idText = "734";

        public string StartIDText = "500";
        public string GoalIDText = "1968";

        private string _status = "Baseline lookup uses a linear scan.";

        private float updateInterval = 0.2f;
        private int currentNodeIndex;
        private bool hasCheckedLocations;
        private bool hasStartedRouteFinder;
        private int steps;

        private void Awake()
        {
            Instance = this;
        }
        private void Start()
        {
            _world = WorldDatasetFactory.CreateMedium();
            _queries = new WorldQueries(_world);

            for (int i = 0; i < _world.Locations.Count; ++i)
            {
                Location location = _world.Locations[i];
                var node = Instantiate(_nodePrefab);
                node.name = $"{location.Id}_{location.Name}";
                node.transform.position = new Vector3(location.Position.X, 0f, location.Position.Y);
                node.transform.localScale = Vector3.one * 0.9f;
                node.GetComponent<Renderer>().material.color = new Color(0.45f, 0.65f, 0.85f);
                Views.Add(location.Id, node);
            }

            /*for (int i = 0; i < _world.Connections.Count; ++i)
            {
                Connection c = _world.Connections[i];
                if (!Views.TryGetValue(c.FromLocationId, out GameObject? a) || !Views.TryGetValue(c.ToLocationId, out GameObject? b)) continue;
                if (c.FromLocationId > c.ToLocationId) continue; // draw bidirectional road once
                var road = new GameObject($"Edge_{c.FromLocationId}_{c.ToLocationId}");
                var lr = road.AddComponent<LineRenderer>();
                lr.positionCount = 2;
                lr.SetPosition(0, a.transform.position + Vector3.up * 0.05f);
                lr.SetPosition(1, b.transform.position + Vector3.up * 0.05f);
                lr.startWidth = 0.08f;
                lr.endWidth = 0.08f;
                lr.material = edgeMAT;
                lr.startColor = Color.gray;
                lr.endColor = Color.gray;
                lr.shadowCastingMode = ShadowCastingMode.Off;
                lr.receiveShadows = false;
                road.isStatic = true;
            }*/
            InitAction(SelectNode);
            onNodeSelected += CountSteps;

            
        }
        private void Update()
        {
            updateInterval -= Time.deltaTime;

            if (currentNodeIndex < Views.Count && hasStartedRouteFinder && !hasCheckedLocations)
            {
                StartCoroutine(CheckNode());
            }
            else if (currentNodeIndex >= Views.Count && !hasStartedRouteFinder && !hasCheckedLocations)
            {
                hasCheckedLocations = true;
                currentNodeIndex = 0;
                Debug.Log("Checked Everything!");
            }

            if (updateInterval < 0 && hasCheckedLocations && currentNodeIndex < routeResult.LocationIds.Count)
            {
                onNodeSelected?.Invoke(currentNodeIndex);
                updateInterval = 0.02f;
                currentNodeIndex++;
            }
            else if (routeResult != null)
            {
                if (currentNodeIndex >= routeResult.LocationIds.Count && hasCheckedLocations)
                {
                    hasCheckedLocations = false;
                    currentNodeIndex = 0;
                    onNodeSelected -= CountSteps;
                    Debug.Log("Reached the goal node!");
                }
            }
        }
        
        private void InitAction(Action<int> action)
        {
            onNodeSelected = action;
        }
        private void CountSteps(int something)
        {
            steps++;
        }
        private IEnumerator CheckNode()
        {
            hasStartedRouteFinder = false;

            if (currentNodeIndex >= Views.Count)
                yield break;

            foreach (var item in Views.Values)
            {
                item.GetComponent<Renderer>().material.color = new Color(0.2f, 0.85f, 0.35f);

                currentNodeIndex++;
                yield return new WaitForSeconds(0.0001f);
            }
        }
        private void SelectNode(int nodeIndex)
        {
            if (nodeIndex >= routeResult.LocationIds.Count)
            {
                return;
            }
            else if (nodeIndex == 0)
            {
                Renderer startLocationRenderer = Views[routeResult.LocationIds[nodeIndex]].GetComponent<Renderer>();
                startLocationRenderer.material.color = new Color(1f, 0.1f, 0.35f);
            }
            else
            {
                if (nodeIndex == (routeResult.LocationIds.Count - 1))
                {
                    Renderer goalLocationRenderer = Views[routeResult.LocationIds[routeResult.LocationIds.Count - 1]].GetComponent<Renderer>();
                    goalLocationRenderer.material.color = new Color(0.2f, 0.25f, 1f);
                }
                else
                    Views[routeResult.LocationIds[nodeIndex]].GetComponent<Renderer>().material.color = new Color(1f, 1f, 0.25f);
            }
                
        }
        private void ResetColors()
        {
            foreach (GameObject view in Views.Values)
            {
                view.GetComponent<Renderer>().material.color = new Color(0.45f, 0.65f, 0.85f);
            }
            hasCheckedLocations = false;
        }

        private void Find()
        {
            if (_queries == null || !int.TryParse(_idText, out int id)) return;
            ResetColors();
            Location? location = _queries.FindLocationById(id);
            if (location == null)
            {
                _status = $"Location {id} not found.";
                return;
            }

            Views[id].GetComponent<Renderer>().material.color = new Color(0.2f, 0.85f, 0.35f);
            int outgoing = _queries.FindOutgoingConnections(id).Count;
            _status = $"Found {location.Name}. Outgoing connections: {outgoing}. Baseline queries scan the stored collections.";
        }
        private void ShortestPathDijkstra(int startLocationID, int goalLocationID)
        {
            currentNodeIndex = 0;
            DijkstraRouteFinder routeFinder = new DijkstraRouteFinder(_world);
            routeResult = routeFinder.FindRoute(startLocationID, goalLocationID);

            ResetColors();

            for (int i = 0; i < routeResult.LocationIds.Count; i++)
            {
                /*if (updateInterval < 0)
                {
                    updateInterval = 0.2f;
                    _views[routeResult.LocationIds[i]].GetComponent<Renderer>().material.color = new Color(0.2f, 0.85f, 0.35f);
                }*/
                
            }
            hasStartedRouteFinder = true;
        }
        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(20f, 20f, 500f, 245f), GUI.skin.box);
            GUILayout.Label("FRONTIER DISPATCH - assignment world-map baseline");
            GUILayout.Label("This scene visualizes the same pure C# WorldData/WorldQueries compiled by the standalone .NET solution.");
            GUILayout.Label("Location ID:");
            _idText = GUILayout.TextField(_idText);
            if (GUILayout.Button("Find location + outgoing connections")) Find();
            StartIDText = GUILayout.TextField(StartIDText);
            GoalIDText = GUILayout.TextField(GoalIDText);

            if (GUILayout.Button("Find shortest path")) ShortestPathDijkstra(int.Parse(StartIDText), int.Parse(GoalIDText));
            GUILayout.Label("Steps taken: " + steps);
            GUILayout.Space(10f);
            GUILayout.Label(_status);
            GUILayout.Label("Students are assessed on the headless C# system. Unity visualization is optional.");
            GUILayout.EndArea();
        }
    }
}
