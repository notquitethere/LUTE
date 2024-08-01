using Mapbox.Unity.Utilities;
using Mapbox.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace LoGaCulture.LUTE
{
    /// <summary>
    /// A class that handles the failure of the location service (i.e. the user cannot accses a location)
    /// Provides a series of methods that can be called to handle inaccessibility of the location
    /// When player reaches a location that cannot be accessed, the location searches for this object and matching location
    /// It then acts according to the priority of the methods defined in this class (this list can be modified by designers in the inspector)
    /// </summary>
    public class LocationFailureHandler : MonoBehaviour
    {
        [SerializeField] protected List<FailureMethod> failureMethods = new List<FailureMethod>();

        public List<FailureMethod> FailureMethods { get => failureMethods; set => failureMethods = value; }

        public enum FailureHandlingOutcome
        {
            Continue, // Continue to the next method in the list
            Stop, // Stop the execution of the methods, consider the failure as handled
            Abort // Abort the execution of the methods, consider the failure unhandled
        }

        private static Dictionary<string, MethodInfo> availableMethods = new Dictionary<string, MethodInfo>();

        private void Awake()
        {
            RegisterFailureHandlingMethods();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnBeforeSceneLoadRuntimeMethod()
        {
            RegisterFailureHandlingMethods();
        }

        private static void RegisterFailureHandlingMethods()
        {
            if (availableMethods.Count > 0)
            {
                return;
            }

            var methods = typeof(LocationFailureHandler).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            foreach (var method in methods)
            {
                var attribute = Attribute.GetCustomAttribute(method, typeof(FailureHandlingMethodAttribute)) as FailureHandlingMethodAttribute;
                if (attribute != null)
                {
                    availableMethods[method.Name] = method;
                }
            }
        }

        public static string[] GetAvailableMethods()
        {
            RegisterFailureHandlingMethods();
            return new List<string>(availableMethods.Keys).ToArray();
        }

        public bool HandleFailure(Vector2d location)
        {
            if (location == null)
            {
                // There are no locations to handle
                return false;
            }
            foreach (FailureMethod method in failureMethods)
            {
                var location2d = Conversions.StringToLatLon(method.QueriedLocation.Value);
                if (Vector2d.Equals(location2d, location))
                {
                    foreach (string methodName in method.PriorityMethods)
                    {
                        if (availableMethods.TryGetValue(methodName, out var methodInfo))
                        {
                            FailureHandlingOutcome outcome = InvokeMethod(methodInfo, method);
                            switch (outcome)
                            {
                                case FailureHandlingOutcome.Stop:
                                    return true;
                                case FailureHandlingOutcome.Abort:
                                    return false;
                                case FailureHandlingOutcome.Continue:
                                    continue;
                            }
                        }
                    }
                    return false; // If we've gone through all methods without a Stop or Abort
                }
            }
            return false; // If no matching FailureMethod was found
        }

        private FailureHandlingOutcome InvokeMethod(MethodInfo methodInfo, FailureMethod failureMethod)
        {
            var parameters = methodInfo.GetParameters();
            var args = new object[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].ParameterType == typeof(FailureMethod))
                {
                    args[i] = failureMethod;
                }
                else if (parameters[i].ParameterType == typeof(LocationVariable))
                {
                    args[i] = failureMethod.QueriedLocation;
                }
                else if (parameters[i].ParameterType == typeof(List<LocationVariable>))
                {
                    args[i] = failureMethod.BackupLocations;
                }
                // Add more parameter types as needed
            }

            object result = methodInfo.Invoke(this, args);

            // Interpret the result
            if (result is FailureHandlingOutcome outcome)
            {
                return outcome;
            }
            else if (result is bool boolResult)
            {
                return boolResult ? FailureHandlingOutcome.Stop : FailureHandlingOutcome.Continue;
            }
            else if (methodInfo.ReturnType == typeof(void))
            {
                return FailureHandlingOutcome.Continue;
            }
            else
            {
                // For any other return type, consider it a success if non-null
                return result != null ? FailureHandlingOutcome.Stop : FailureHandlingOutcome.Continue;
            }
        }

        [FailureHandlingMethod]
        private FailureHandlingOutcome UseBackupLocation(FailureMethod failureMethod)
        {
            // Check if there are any backup locations
            if (failureMethod.BackupLocations == null || failureMethod.BackupLocations.Count == 0)
            {
                return FailureHandlingOutcome.Continue;
            }

            // Iterate through all backup locations
            foreach (var backupLocation in failureMethod.BackupLocations)
            {
                // Check if the backup location is the same as the queried location
                if (backupLocation.Equals(failureMethod.QueriedLocation))
                {
                    Debug.Log($"Skipping backup location {backupLocation} as it's the same as the queried location.");
                    continue;
                }

                // Check if the backup location is accessible
                if (IsLocationAccessible(backupLocation))
                {
                    var engine = failureMethod.GetEngine();
                    if (engine != null)
                    {
                        var map = engine.GetMap();
                        if (map != null)
                        {
                            map.HideLocationMarker(failureMethod.QueriedLocation);
                            bool updateText = failureMethod.UpdateLocationText;
                            map.ShowLocationMarker(backupLocation, updateText, failureMethod.QueriedLocation.Key);
                        }
                    }

                    failureMethod.QueriedLocation.Apply(SetOperator.Assign, backupLocation);
                    failureMethod.IsHandled = true;
                    return FailureHandlingOutcome.Stop;
                }
                else
                {
                    Debug.Log($"Backup location {backupLocation} is inaccessible. Trying next backup location.");
                }
            }

            // If we've gone through all backup locations and none were suitable
            return FailureHandlingOutcome.Continue;
        }

        private bool IsLocationAccessible(LocationVariable location)
        {
            // Implement your logic to check if a location is accessible using some server side or client side logic
            // This could involve checking GPS coordinates, network connectivity, or any other relevant factors
            // Return true if the location is accessible, false otherwise

            // For now, we'll just return true for demonstration purposes
            return true;
        }

        [FailureHandlingMethod]
        private FailureHandlingOutcome IncreaseRadius(FailureMethod failureMethod)
        {
            if (failureMethod.HasIncreased && !failureMethod.AllowContinuousIncrease)
            {
                // We have already increased but we don't allow multiple increases
                return FailureHandlingOutcome.Continue;
            }
            // Increase the location radius check size
            failureMethod.QueriedLocation.RadiusIncrease += failureMethod.RadiusIncreaseSize;
            failureMethod.HasIncreased = true;

            // Possibly show player a message about the increased radius and to move around
            // Could use dialogue system for this

            failureMethod.IsHandled = true;
            return FailureHandlingOutcome.Stop;
        }

        [FailureHandlingMethod]
        private FailureHandlingOutcome ExecuteAnyway(FailureMethod failureMethod)
        {
            // If location cannot be accessed then we create a menu of failed nodes for the player to execute
            failureMethod.QueriedLocation.locationDisabled = true;
            var engine = failureMethod.GetEngine();
            if (engine != null)
            {
                var map = engine.GetMap();
                if (map != null)
                {
                    map.HideLocationMarker(failureMethod.QueriedLocation);
                }

                var nodes = engine.GetComponents<Node>();
                var affectedNodes = new List<Node>();

                foreach (var node in nodes)
                {
                    bool nodeAffected = false;

                    if (node.NodeLocation != null && Equals(node.NodeLocation.Value, failureMethod.QueriedLocation.Value))
                    {
                        nodeAffected = true;
                    }

                    if (!nodeAffected)
                    {
                        foreach (var order in node.OrderList)
                        {
                            if (order.GetOrderLocation() != null && Equals(order.GetOrderLocation().Value, failureMethod.QueriedLocation.Value))
                            {
                                nodeAffected = true;
                            }
                        }
                    }

                    if (nodeAffected)
                    {
                        affectedNodes.Add(node);
                    }
                }

                foreach (var affectedNode in affectedNodes)
                {
                    affectedNode.NodeLocation = null;
                    affectedNode.Stop();
                    affectedNode.ShouldCancel = true;
                    LocationServiceSignals.DoLocationFailed(failureMethod, affectedNode);
                }

                // If we have affected nodes then we stop the failure method and the player can execute the node via menu
                if (affectedNodes.Any())
                {
                    failureMethod.IsHandled = true;
                    return FailureHandlingOutcome.Stop;
                }
            }

            return FailureHandlingOutcome.Continue;
        }

        [FailureHandlingMethod]
        private FailureHandlingOutcome UseNearestLocation(FailureMethod failureMethod)
        {
            var engine = failureMethod.GetEngine();
            if (engine != null)
            {
                var map = engine.GetMap();
                if (map != null)
                {
                    LocationVariable nearestLocation = null;
                    var allLocations = engine.GetComponents<LocationVariable>();
                    foreach (var location in allLocations)
                    {
                        if (location != null && !location.Equals(failureMethod.QueriedLocation))
                        {
                            if (nearestLocation == null)
                            {
                                nearestLocation = location;
                            }
                            else
                            {
                                var currentDistance = Vector2d.Distance(Conversions.StringToLatLon(nearestLocation.Value), Conversions.StringToLatLon(failureMethod.QueriedLocation.Value));
                                var newDistance = Vector2d.Distance(Conversions.StringToLatLon(location.Value), Conversions.StringToLatLon(failureMethod.QueriedLocation.Value));
                                if (newDistance < currentDistance)
                                {
                                    nearestLocation = location;
                                }
                            }
                        }
                    }
                    if (nearestLocation != null)
                    {
                        map.HideLocationMarker(failureMethod.QueriedLocation);
                        bool updateText = failureMethod.UpdateLocationText;
                        map.ShowLocationMarker(nearestLocation, updateText, failureMethod.QueriedLocation.Key);
                        failureMethod.QueriedLocation.Apply(SetOperator.Assign, nearestLocation);
                        failureMethod.IsHandled = true;
                        return FailureHandlingOutcome.Stop;
                    }
                }
            }
            return FailureHandlingOutcome.Continue;
        }

        [FailureHandlingMethod]
        private FailureHandlingOutcome JumpToNode(FailureMethod failureMethod)
        {
            var engine = failureMethod.GetEngine();
            // If we find a node and the engine is available then we can jump to the node
            if (engine != null && failureMethod.BackupNode != null)
            {
                int index = 0;
                if (failureMethod.StartIndex >= 0 && failureMethod.StartIndex <= failureMethod.BackupNode.OrderList.Count)
                {
                    index = failureMethod.StartIndex;
                }
                engine.ExecuteNode(failureMethod.BackupNode, index);
                // Do we hide the location also?
                failureMethod.IsHandled = true;
                return FailureHandlingOutcome.Stop;
            }

            // Otherwise, we continue to the next method
            return FailureHandlingOutcome.Continue;
        }

        [FailureHandlingMethod]
        private void DisbaleLocationBehaviour(FailureMethod failureMethod)
        {
            // Find any reference to location - if on node then set node to cannot execute
            // If on order then do the same to the parent node
            var engine = failureMethod.GetEngine();
            if (engine != null)
            {
                var map = engine.GetMap();
                if (map != null)
                {
                    map.HideLocationMarker(failureMethod.QueriedLocation);
                }

                var nodes = engine.GetComponents<Node>();
                foreach (var node in nodes)
                {
                    // If the node uses the same location as the failure method then it cannot execute
                    if (node.NodeLocation != null && Equals(node.NodeLocation.Value, failureMethod.QueriedLocation.Value))
                    {
                        node.NodeComplete = true;
                        node.CanExecuteAgain = false;
                        failureMethod.IsHandled = true;
                    }
                    foreach (var order in node.OrderList)
                    {
                        // If the order uses the same location as the failure method then the parent node cannot execute
                        if (order.GetOrderLocation() != null && Equals(order.GetOrderLocation(), failureMethod.QueriedLocation.Value))
                        {
                            node.NodeComplete = true;
                            node.CanExecuteAgain = false;
                            failureMethod.IsHandled = true;
                        }
                    }
                }
            }
        }
    }

    [InitializeOnLoad]
    public class LocationFailureHandlerInitializer
    {
        static LocationFailureHandlerInitializer()
        {
            LocationFailureHandler.GetAvailableMethods(); // This will trigger method registration
        }
    }

    [Serializable]
    public class FailureMethod
    {
        //[Header("Failure Method and Location")]
        [Tooltip("The location that this method is associated with")]
        [SerializeField] protected LocationVariable queriedLocation;
        [Tooltip("A list of methods that can be executed to handle the failure")]
        [SerializeField] protected List<string> priorityMethods = new List<string>();
        //[Header("Backup Locations")]
        [Tooltip("A list of locations that can be used as alternatives")]
        [SerializeField] protected List<LocationVariable> backupLocations = new List<LocationVariable>();
        [Tooltip("Whether the failure has been handled")]
        [SerializeField] protected bool isHandled = false;
        [Tooltip("Whether the location text should be updated when the location is changed")]
        [SerializeField] protected bool updateLocationText = false;
        //[Header("Node Jump Settings")]
        [Tooltip("The node to jump to if the location is inaccessible")]
        [SerializeField] protected Node backupNode;
        [Tooltip("The index of the order list to start from on the backup node")]
        [SerializeField] protected int startIndex = 0;
        //[Header("Radius Increase Settings")]
        [Tooltip("Whether the radius of the location can be increased more than once")]
        [SerializeField] protected bool allowContinuousIncrease = false;
        [Tooltip("The size of the radius increase in meters")]
        [SerializeField] protected float radiusIncreaseSize = 50.0f;

        // This is a special case where we need to ensure that the radius has been increased
        private bool hasIncreased = false;

        public LocationVariable QueriedLocation { get => queriedLocation; }
        public List<LocationVariable> BackupLocations { get => backupLocations; }
        public List<string> PriorityMethods { get => priorityMethods; }
        public bool IsHandled { get => isHandled; set => isHandled = value; }
        public bool AllowContinuousIncrease { get => allowContinuousIncrease; }
        public bool UpdateLocationText { get => updateLocationText; }
        public bool HasIncreased { get => hasIncreased; set => hasIncreased = value; }
        public Node BackupNode { get => backupNode; }
        public int StartIndex { get => startIndex; }
        public float RadiusIncreaseSize { get => radiusIncreaseSize; }

        public BasicFlowEngine GetEngine()
        {
            var engine = BasicFlowEngine.CachedEngines.Find(e => e != null);
            if (engine == null)
            {
                engine = UnityEngine.Object.FindObjectOfType<BasicFlowEngine>();
            }
            return engine;
        }
    }
}