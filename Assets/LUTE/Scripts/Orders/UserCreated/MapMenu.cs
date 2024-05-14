using UnityEngine;

[OrderInfo("Map",
              "MapMenu",
              "Creates a custom icon to open and clsoe the map (avoiding the need to put this choice in a sub menu)")]
[AddComponentMenu("")]
public class MapMenu : Order
{
  [Tooltip("Custom icon to display for this menu")]
  [SerializeField] protected Sprite customButtonIcon;
  [Tooltip("A custom popup class to use to display this menu - if one is in the scene it will be used instead")]
  [SerializeField] protected PopupIcon setIconButton;
  [Tooltip("If true, the popup icon will be displayed, otherwise it will be hidden")]
  [SerializeField] protected bool showIcon = true;
  public override void OnEnter()
  {
    var engine = GetEngine();
    if (engine == null)
    {
      Continue();
      return;
    }
    var map = engine.GetMap();
    if (map == null)
    {
      Continue();
      return;
    }

    if (setIconButton != null)
    {
      PopupIcon.ActivePopupIcon = setIconButton;
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
    UnityEngine.Events.UnityAction action = () =>
{
  map.ToggleMap();
};
    popupIcon.SetAction(action);
    popupIcon.MoveToNextOption();
    Continue();
  }

  public override string GetSummary()
  {
    return "Creating a custom icon to open/close the map (if one exists)";
  }
}