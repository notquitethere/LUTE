using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.Events;

[OrderInfo("Menu",
              "GenericButton",
              "Creates a button which allows for generic event inputs")]
[AddComponentMenu("")]
public class GenericButton : Order
{
    [Tooltip("Custom icon to display for this menu")]
    [SerializeField] protected Sprite customButtonIcon;
    [Tooltip("A custom popup class to use to display this menu - if one is in the scene it will be used instead")]
    [SerializeField] protected PopupIcon setIconButton;
    [Tooltip("If true, the popup icon will be displayed, otherwise it will be hidden")]
    [SerializeField] protected bool showIcon = true;
    [Tooltip("The feedbacks to play when the button is clicked")]
    [SerializeField] protected MMFeedbacks buttonFeedback;
    [Tooltip("The event to call when the button is clicked")]
    [SerializeField] protected UnityEngine.Events.UnityEvent buttonEvent;
    public override void OnEnter()
    {
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

        // Add additional functionality to button event
        UnityAction extendedAction = () =>
        {
            // Call the original button event
            buttonEvent.Invoke();
            // Play the feedbacks
            buttonFeedback?.PlayFeedbacks();
        };

        popupIcon.SetAction(extendedAction.Invoke);
        popupIcon.MoveToNextOption();
        Continue();
    }

    public override string GetSummary()
    {
        return "Creates a button which allows for generic event inputs";
    }
}