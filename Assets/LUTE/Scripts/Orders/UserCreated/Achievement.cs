using UnityEngine;

[OrderInfo("Adventure",
              "Achievement",
              "Update an achievement or quest status")]
[AddComponentMenu("")]
public class Achievement : Order
{
  [Tooltip("The ID of the achievement or quest")]
  [SerializeField] protected string achievementID;
  [Tooltip("If true, will add progress to the achievement, if false, will unlock it")]
  [SerializeField] protected bool progress;
  [Tooltip("The amount of progress to add")]
  [SerializeField] protected int amount = 1;
  public override void OnEnter()
  {
    if (string.IsNullOrEmpty(achievementID))
    {
      Debug.LogError("Achievement ID is missing!");
      return;
    }

    var achievementRules = GetEngine().GetComponentInChildren<AchievementRules>();

    if (achievementRules == null)
    {
      return;
    }
    achievementRules.GenericEvent(achievementID, progress, amount);
    Continue();
  }

  public override string GetSummary()
  {
    return "Achievement: " + achievementID + " " + (progress ? "Progress" : "Unlock");
  }
}