namespace Mapbox.Examples
{
	using UnityEngine;
	using Mapbox.Utils;
	using Mapbox.Unity.Map;
	using Mapbox.Unity.Utilities;
	using System.Collections.Generic;
	using System.Linq;
	using System.Collections;
	using Mapbox.Unity.MeshGeneration.Factories;

	public class SpawnOnMap : MonoBehaviour
	{
		[SerializeField] public Transform tracker;
		[SerializeField]
		AbstractMap _map;

		[Geocode]
		protected List<string> _locationStrings = new List<string>();
		public Vector2d[] _locations = new Vector2d[1];

		[SerializeField]
		public float _spawnScale = 5f;

		[SerializeField]
		public CameraBillboard _markerPrefab;
		[SerializeField]
		private DirectionsFactory _directionPrefab;

		List<CameraBillboard> _spawnedObjects;

		public bool _isWithinRadius = false;

		private LocationVariable locationVariable;
		private If ifOrder;
		private Node _parentNode;

		private static List<string> _locationNames = new List<string>();
		private static List<Sprite> _locationSprites = new List<Sprite>();
		private static List<Color> _locationColours = new List<Color>();
		private static List<bool> _locationShowNames = new List<bool>();
		private BasicFlowEngine engine;

		void Start()
		{
            //get engine by component
            //get all nodes
            //check all orders on nodes
            //if order is equal to variable condition and is based on location
            //get location and add it to the location string
            //then spawn the marker

            engine = GetComponentInParent<BasicFlowEngine>();
			if (!engine)
			{
				Debug.LogError("No engine found");
				return;
			}

			_locationNames.Clear();

			List<Node> nodes = engine.gameObject
				.GetComponents<Node>()
				.ToList();

			foreach (Node node in nodes)
			{
				if (node == null)
					continue;
				if (node.OrderList == null)
					continue;
				if (node.OrderList.Count == 0)
					continue;
				if (node.NodeLocation != null)
				{
					locationVariable = node.NodeLocation;
					Vector2d latLong = Conversions.StringToLatLon(node.NodeLocation.Value);
					var newLocationString = string.Format("{0}, {1}", latLong.x, latLong.y);
                    if (!_locationStrings.Contains(newLocationString))
                    {
						_locationStrings.Add(newLocationString);
						_locationNames.Add(locationVariable.Key);
						_locationSprites.Add(locationVariable.locationSprite);
						_locationColours.Add(locationVariable.locationColor);
						_locationShowNames.Add(locationVariable.showLocationName);
					}
				}

				var orderList = node.OrderList;
				if (orderList.Count > 0)
				{
					foreach (Order order in orderList)
					{
						var locations = new List<LocationVariable>();
						order.GetLocationVariables(ref locations);
						foreach (var location in locations)
						{
							Vector2d latLong = Conversions.StringToLatLon(location.Value);
							var newLocationString = string.Format("{0}, {1}", latLong.x, latLong.y);
							if(!_locationStrings.Contains(newLocationString))
							{
								_locationStrings.Add(newLocationString);
								_locationNames.Add(location.Key);
								_locationSprites.Add(location.locationSprite);
								_locationColours.Add(location.locationColor);
								_locationShowNames.Add(location.showLocationName);
							}
						}
						if (order.GetType() == typeof(If))
						{
							ifOrder = order as If;
							locationVariable = ifOrder.ReferencesLocation();
							if (locationVariable)
							{
								_parentNode = node;
								Vector2d latLong = Conversions.StringToLatLon(locationVariable.Value);
								var newLocationString = string.Format("{0}, {1}", latLong.x, latLong.y);
								if(!_locationStrings.Contains(newLocationString))
								{
									_locationStrings.Add(newLocationString);
									_locationNames.Add(locationVariable.Key);
									_locationSprites.Add(locationVariable.locationSprite);
									_locationColours.Add(locationVariable.locationColor);
									_locationShowNames.Add(locationVariable.showLocationName);
								}
							}
						}
					}
				}
			}

			_locations = new Vector2d[_locationStrings.Count];
			_spawnedObjects = new List<CameraBillboard>();
			for (int i = 0; i < _locationStrings.Count; i++)
			{
				var locationString = _locationStrings[i];
				_locations[i] = Conversions.StringToLatLon(locationString);
				var instance = Instantiate(_markerPrefab);
				instance.GetComponent<CameraBillboard>().SetCanvasCam(GetComponent<QuadTreeCameraMovement>()?._referenceCameraGame);
				instance.gameObject.transform.localPosition = _map.GeoToWorldPosition(_locations[i], true);
				instance.gameObject.transform.localScale = new Vector3(_spawnScale, _spawnScale, _spawnScale);
				_spawnedObjects.Add(instance);
                //ensure you set the radius sphere correctly here (get radius then multiply by 4 then add to scale of marker)
            }

            // DrawDirections();
        }

		public GameObject HideLocationMarker(LocationVariable location)
		{
			var loc2d = Conversions.StringToLatLon(location.Value);
			var globalPos = _map.GeoToWorldPosition(loc2d, true);

            var locationMarker = _spawnedObjects.Find(marker => marker.transform.localPosition == globalPos);
            if (locationMarker != null)
            {
                locationMarker.gameObject.SetActive(false);
                return locationMarker.gameObject;
            }
            return null;
        }

        public GameObject ShowLocationMarker(LocationVariable location)
        {
            var loc2d = Conversions.StringToLatLon(location.Value);
            var globalPos = _map.GeoToWorldPosition(loc2d, true);

            var locationMarker = _spawnedObjects.Find(marker => marker.transform.localPosition == globalPos);
            if (locationMarker != null)
            {
                locationMarker.gameObject.SetActive(true);
                return locationMarker.gameObject;
            }
            return null;
        }

		public bool IsWithinRadius(string location, string centre, float radius)
		{
			var location2D = Conversions.StringToLatLon(location);
            var centre2D = Conversions.StringToLatLon(centre);

			var locationMetered = Conversions.LatLonToMeters(location2D);
            var centreMetered = Conversions.LatLonToMeters(centre2D);

            var distance = Vector2d.Distance(locationMetered, centreMetered);
			return distance < radius;

            //If true then you either:
            //1. use backup location (using the engine)
            // does the engine have a backup location that uses the input location?
            // if so then go through the backup locations of the location and check if they are within the radius
			// find the first one that is not and if none are outside of radius then continue
            //2. disable order
            //3. disable node
        }

        private void DrawDirections()
		{
			// Go through all nodes
			List<Node> nodes = engine.gameObject.GetComponents<Node>().ToList();
			foreach (Node node in nodes)
			{
				// Go through all orders
				foreach (Order order in node.OrderList)
				{
					// Check if order is type of If and uses node variable
					if (order.GetType() == typeof(If))
					{
						If ifOrder = order as If;
						foreach (ConditionExpression condition in ifOrder.conditions)
						{
							if (condition.AnyVariable.variable is NodeVariable)
							{
								Node targetNode = (condition.AnyVariable.variable as NodeVariable).Value;
								// Check if both nodes use location
								if (node.NodeLocation != null && targetNode.NodeLocation != null)
								{
									// Spawn a direction factory object using prefab
									if (_directionPrefab == null)
									{
										Debug.LogError("Direction prefab is null");
										return;
									}
									DirectionsFactory directionFactory = Instantiate(_directionPrefab);
									Transform[] waypoints = new Transform[2];

									foreach (var location in _spawnedObjects)
									{
										var nodeLatLon = Conversions.StringToLatLon(node.NodeLocation.Value);
										var targetNodeLatLon = Conversions.StringToLatLon(targetNode.NodeLocation.Value);
										if (location.gameObject.transform.localPosition == _map.GeoToWorldPosition(nodeLatLon, true))
										{
											waypoints[0] = location.transform;
										}
										if (location.gameObject.transform.localPosition == _map.GeoToWorldPosition(targetNodeLatLon, true))
										{
											waypoints[1] = location.transform;
										}

										if (waypoints[0] != null && waypoints[1] != null)
										{
											directionFactory.SetWaypoints(waypoints, _map);
										}
									}
								}
							}
						}
					}
					else
					{
						//if node calls to another node and both nodes have a location
						if (order.GetType() == typeof(NextNode))
						{
							NextNode nextNode = order as NextNode;
							Node targetNode = nextNode.targetNode;

							if (node.NodeLocation != null && targetNode.NodeLocation != null)
							{
								// Spawn a direction factory object using prefab
								if (_directionPrefab == null)
								{
									Debug.LogError("Direction prefab is null");
									return;
								}
								DirectionsFactory directionFactory = Instantiate(_directionPrefab);
								Transform[] waypoints = new Transform[2];

								foreach (var location in _spawnedObjects)
								{
									var nodeLatLon = Conversions.StringToLatLon(node.NodeLocation.Value);
									var targetNodeLatLon = Conversions.StringToLatLon(targetNode.NodeLocation.Value);
									if (location.transform.localPosition == _map.GeoToWorldPosition(nodeLatLon, true))
									{
										waypoints[0] = location.transform;
									}
									if (location.transform.localPosition == _map.GeoToWorldPosition(targetNodeLatLon, true))
									{
										waypoints[1] = location.transform;
									}

									if (waypoints[0] != null && waypoints[1] != null)
									{
										directionFactory.SetWaypoints(waypoints, _map);
									}
								}
							}
						}
					}
				}
				// Check if the node has a target unlock node and location
				if (node.TargetUnlockNode != null)
				{
					Node targetUnlockNode = node.TargetUnlockNode;
					// Check if both nodes use location (not null)
					if (node.NodeLocation != null && targetUnlockNode.NodeLocation != null)
					{
						// Spawn a direction factory object using prefab
						if (_directionPrefab == null)
						{
							Debug.LogError("Direction prefab is null");
							return;
						}
						DirectionsFactory directionFactory = Instantiate(_directionPrefab);
						Transform[] waypoints = new Transform[2];

						waypoints[0] = new GameObject().transform;
						waypoints[0].gameObject.name = "Waypoint 0";
						var latLon = Conversions.StringToLatLon(node.NodeLocation.Value);
						Vector3 pos1 = new Vector3((float)latLon.x, 0, (float)latLon.y);
						waypoints[0].transform.localPosition = pos1;
						waypoints[1] = new GameObject().transform;
						var latLon2 = Conversions.StringToLatLon(targetUnlockNode.NodeLocation.Value);
						Vector3 pos2 = new Vector3((float)latLon2.x, 0, (float)latLon2.y);
						waypoints[1].transform.localPosition = pos2;
						waypoints[1].gameObject.name = "Waypoint 1";

						directionFactory.SetWaypoints(waypoints, _map);
					}
				}
			}
		}

		private void Update()
		{
			int count = _spawnedObjects.Count;
			for (int i = 0; i < count; i++)
			{
				var spawnedObject = _spawnedObjects[i];
				var location = _locations[i];
				spawnedObject.transform.localPosition = _map.GeoToWorldPosition(location, true);
				spawnedObject.transform.localScale = new Vector3(_spawnScale, _spawnScale, _spawnScale);
				var billboard = spawnedObject.GetComponent<CameraBillboard>();
				billboard.SetCanvasCam(GetComponent<QuadTreeCameraMovement>()?._referenceCameraGame);
				if (_locationNames.Count > i)
				{
					if (_locationNames[i].Contains("_"))
					{
						_locationNames[i] = _locationNames[i].Replace("_", " ");
					}
					billboard.SetText(_locationNames[i]);
				}

				Sprite locSprite = _locationSprites[i];
				if (locSprite)
				{
					billboard.SetIcon(locSprite);
				}

				Color locColour = _locationColours[i];
				if (locColour != null)
				{
					billboard.SetColor(locColour);
				}

				bool showName = _locationShowNames[i];
				billboard.SetName(showName);
			}

			// if (Input.GetMouseButtonUp(1))
			// {
			// 	var mousePosScreen = Input.mousePosition;
			// 	//assign distance of camera to ground plane to z, otherwise ScreenToWorldPoint() will always return the position of the camera
			// 	//http://answers.unity3d.com/answers/599100/view.html
			// 	var cam = GetComponent<QuadTreeCameraMovement>()?._referenceCameraGame;
			// 	mousePosScreen.z = cam.transform.localPosition.y;

			// 	var pos = cam.ScreenToWorldPoint(mousePosScreen);

			// 	var latlongDelta = _map.WorldToGeoPosition(pos);

			// 	var newLocationString = string.Format("{0}, {1}", latlongDelta.x, latlongDelta.y);
			// 	_locationStrings.Add(newLocationString);
			// 	_locations = new Vector2d[_locationStrings.Count];
			// 	for (int i = 0; i < _locationStrings.Count; i++)
			// 	{
			// 		var locationString = _locationStrings[i];
			// 		_locations[i] = Conversions.StringToLatLon(locationString);
			// 	}

			// 	var instance = Instantiate(_markerPrefab);
			// 	// _spawnedObjects.Add(instance);
			// }

			var _mapCam = GetComponent<QuadTreeCameraMovement>()?._referenceCamera;

			if (engine.DemoMapMode)
			{
				//if we are in demo mode then we must show the tracker
				if (_mapCam.enabled)
				{
					tracker.gameObject.SetActive(true);
				}
				else
					tracker.gameObject.SetActive(false);
			}
			else
				tracker.gameObject.SetActive(false);
		}

		public Vector2d TrackerPos()
		{
			var trackerPos = tracker.localPosition;
			var cam = GetComponent<QuadTreeCameraMovement>()?._referenceCamera;
			var pos = cam.ScreenToWorldPoint(trackerPos);

			var latlongDelta = _map.WorldToGeoPosition(pos);

			return latlongDelta;
		}

		public Vector3 TrackerPosWorld()
		{
			return tracker.localPosition;
		}

		public bool ToggleMap()
		{
			var _mapCam = GetComponent<QuadTreeCameraMovement>()?._referenceCamera;
			//set the tracker cam to this cam
			tracker.GetComponent<CameraBillboard>().SetCanvasCam(_mapCam);
			if (_mapCam)
			{
				_mapCam.enabled = !_mapCam.enabled;

				return _mapCam.enabled;
			}
			return false;
		}
	}
}