using UnityEngine;

[OrderInfo("Achievements",
              "Achievement List Button  ",
              "Creates a button which will toggle the achievement on/off (rather than using a nested button in popups)")]
[AddComponentMenu("")]
public class AchievementMenu : Order
{
    [Tooltip("Custom icon to display for this menu")]
    [SerializeField] protected Sprite customButtonIcon;
    [Tooltip("A custom popup class to use to display this menu - if one is in the scene it will be used instead")]
    [SerializeField] protected PopupIcon setIconButton;
    [Tooltip("The set achievement list to toggle - this will be found in the scene if not provided")]
    [SerializeField] protected AchievementListFiller setList;
    [Tooltip("If true, the popup icon will be displayed, otherwise it will be hidden")]
    [SerializeField] protected bool showIcon = true;

    public override void OnEnter()
    {
        if(setList != null)
        {
            AchievementListFiller.ActiveList = setList;
        }

        var list = AchievementListFiller.GetList();

        if (list == null)
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
            list.ShowList();
        };

        popupIcon.SetAction(action);
        popupIcon.MoveToNextOption();

        Continue();
    }
    public override string GetSummary()
    {
        return "Creates a button which will toggle the achievment list on/off (rather than using a nested button in popups)";
    }
}