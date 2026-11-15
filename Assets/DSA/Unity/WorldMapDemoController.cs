#nullable enable

using FrontierDispatch.Core;
using FrontierDispatch.Unity.Assets.DSA.Unity;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Rendering;

namespace FrontierDispatch.Unity
{
    public sealed class WorldMapDemoController : MonoBehaviour
    {
        [SerializeField] private GameObject _nodePrefab;
        [SerializeField] private Material edgeMAT;
        private readonly Dictionary<int, GameObject> _views = new Dictionary<int, GameObject>();
        private WorldData? _world;
        private WorldQueries? _queries;

        private RouteResult routeResult;
        private string _idText = "734";

        private string _startIDText = "500";
        private string _goalIDText = "1968";

        private string _status = "Baseline lookup uses a linear scan.";

        private float updateInterval = 0.2f;
        private int currentNodeIndex;
        private bool hasRunRouteFinder;

        private void Start()
        {
            _world = WorldDatasetFactory.CreateLarge();
            _queries = new WorldQueries(_world);

            for (int i = 0; i < _world.Locations.Count; ++i)
            {
                Location location = _world.Locations[i];
                var node = Instantiate(_nodePrefab);
                node.name = $"{location.Id}_{location.Name}";
                node.transform.position = new Vector3(location.Position.X, 0f, location.Position.Y);
                node.transform.localScale = Vector3.one * 0.7f;
                node.GetComponent<Renderer>().material.color = new Color(0.45f, 0.65f, 0.85f);
                _views.Add(location.Id, node);
            }

            for (int i = 0; i < _world.Connections.Count; ++i)
            {
                Connection c = _world.Connections[i];
                if (!_views.TryGetValue(c.FromLocationId, out GameObject? a) || !_views.TryGetValue(c.ToLocationId, out GameObject? b)) continue;
                if (c.FromLocationId > c.ToLocationId) continue; // draw bidirectional road once
                var road = new GameObject($"Road_{c.FromLocationId}_{c.ToLocationId}");
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
            }
        }
        private void Update()
        {
            updateInterval -= Time.deltaTime;

            if (updateInterval < 0 && hasRunRouteFinder && currentNodeIndex < routeResult.LocationIds.Count)
            {
                updateInterval = 0.02f;
                SelectNode();
                currentNodeIndex++;
            }
            else if (currentNodeIndex >= _views.Count)
            {
                hasRunRouteFinder = false;
                currentNodeIndex = 0;
                Debug.Log("Reached the goal node!");
            }
        }

        private void SelectNode()
        {
            if (currentNodeIndex >= _views.Count)
                return;
            else
                _views[routeResult.LocationIds[currentNodeIndex]].GetComponent<Renderer>().material.color = new Color(0.2f, 0.85f, 0.35f);
        }
        private void ResetColors()
        {
            foreach (GameObject view in _views.Values)
            {
                view.GetComponent<Renderer>().material.color = new Color(0.45f, 0.65f, 0.85f);
            }
            hasRunRouteFinder = false;
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

            _views[id].GetComponent<Renderer>().material.color = new Color(0.2f, 0.85f, 0.35f);
            int outgoing = _queries.FindOutgoingConnections(id).Count;
            _status = $"Found {location.Name}. Outgoing connections: {outgoing}. Baseline queries scan the stored collections.";
        }
        private void FindShortestPath(int startLocationID, int goalLocationID)
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
            hasRunRouteFinder = true;
        }
        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(20f, 20f, 500f, 245f), GUI.skin.box);
            GUILayout.Label("FRONTIER DISPATCH - assignment world-map baseline");
            GUILayout.Label("This scene visualizes the same pure C# WorldData/WorldQueries compiled by the standalone .NET solution.");
            GUILayout.Label("Location ID:");
            _idText = GUILayout.TextField(_idText);
            if (GUILayout.Button("Find location + outgoing connections")) Find();
            _startIDText = GUILayout.TextField(_startIDText);
            _goalIDText = GUILayout.TextField(_goalIDText);

            if (GUILayout.Button("Find shortest path")) FindShortestPath(int.Parse(_startIDText), int.Parse(_goalIDText));
            GUILayout.Space(10f);
            GUILayout.Label(_status);
            GUILayout.Label("Students are assessed on the headless C# system. Unity visualization is optional.");
            GUILayout.EndArea();
            
        }
    }
}
