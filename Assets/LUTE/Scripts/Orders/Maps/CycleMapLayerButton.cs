using Mapbox.Unity.Map;
using UnityEngine;

namespace LoGaCulture.LUTE
{
    [OrderInfo("Map",
             "Cycle Layers",
             "Creates a button which will cycle and loop through the map layers defined.")]
    [AddComponentMenu("")]
    public class CycleMapLayerButton : Order
    {
        [Tooltip("The layers to cycle through")]
        [SerializeField] protected ImageryLayer[] layers;
        [Tooltip("Custom icon to display for this menu")]
        [SerializeField] protected Sprite customButtonIcon;
        [Tooltip("If true, the popup icon will be displayed, otherwise it will be hidden")]
        [SerializeField] protected bool showIcon = true;

        public override void OnEnter()
        {
            if (layers == null || layers.Length == 0)
            {
                return;
            }

            var popupIcon = PopupIcon.GetPopupIcon();
            if (popupIcon != null)
            {
                if (customButtonIcon != null)
                {
                    popupIcon.SetIcon(customButtonIcon);
                }
            }
            if (showIcon)
            {
                popupIcon.SetActive(true);
            }

            UnityEngine.Events.UnityAction buttonAction = () =>
            {
                LogaManager.Instance.MapLayerChanger.CycleLayers(layers);
            };

            popupIcon.SetAction(buttonAction.Invoke);
            popupIcon.MoveToNextOption();
            Continue();
        }
    }
}
