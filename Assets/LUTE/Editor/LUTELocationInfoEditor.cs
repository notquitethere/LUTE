using System.Linq;
using UnityEditor;
using UnityEngine;

namespace LoGaCulture.LUTE
{
    [CustomEditor(typeof(LUTELocationInfo))]
    public class LUTELocationInfoEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var locationInfo = target as LUTELocationInfo;

            base.OnInspectorGUI();
            if (GUILayout.Button(new GUIContent("Create Node from Location", "Creates a Node based on this Location.")))
            {
                GraphWindow graphWindow = GraphWindow.ShowWindow();

                if (graphWindow != null)
                {
                    var engine = FindObjectOfType<BasicFlowEngine>();
                    if (engine != null)
                    {
                        var locVar = engine.GetComponents<LocationVariable>().FirstOrDefault(x => x.Value.infoID == locationInfo.infoID);
                        graphWindow.AddNodeFromLocation(locVar);
                    }
                }
            }
        }
    }
}
