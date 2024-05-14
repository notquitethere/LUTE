using UnityEngine;

[OrderInfo("Scenes",
              "LoadScene",
              "Loads a scene based on a given scene index or exact name")]
[AddComponentMenu("")]
public class LoadScene : Order
{
  [Tooltip("the exact name of the target level")]
  [SerializeField] protected string levelName;
  [Tooltip("the index of the target level")]
  [SerializeField] protected int levelIndex;
  public override void OnEnter()
  {
    if (!string.IsNullOrEmpty(levelName))
      LevelSelector.LoadScene(levelName);
    else if (levelIndex >= 0)
    {
      //implement this later in level selector    
    }
    else
      Debug.LogError("No level name or index provided");
    //Continue();
  }

  public override string GetSummary()
  {
    //you can use this to return a summary of the order which is displayed in the inspector of the order
    string levelName = string.IsNullOrEmpty(this.levelName) ? "No level name provided" : this.levelName;
    if (levelIndex >= 0)
    {
      levelName += " (Index: " + levelIndex + ")";
    }
    return "Loading Scene " + levelName;
  }
}