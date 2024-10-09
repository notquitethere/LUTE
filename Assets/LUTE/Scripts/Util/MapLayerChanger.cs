using Mapbox.Unity.Location;
using UnityEngine;

namespace LoGaCulture.LUTE
{
    /// <summary>
    /// Changes the current abstract map layer based on what has been provided.
    /// </summary>
    public class MapLayerChanger : MonoBehaviour
    {
        private bool isInitialized;

        protected virtual void Start()
        {
            LocationProviderFactory.Instance.mapManager.OnInitialized += () => isInitialized = true;
        }

        public virtual void ChangeLayer(string layerSource)
        {
            var map = LocationProviderFactory.Instance.mapManager;

            map.ImageLayer.SetLayerSource(layerSource);
        }
    }
}
