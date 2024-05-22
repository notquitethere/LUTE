using UnityEngine;
using UnityEngine.SceneManagement;
using static SavePoint;

[OrderInfo(
    "Saving",
    "Load Game",
    "Loads game from either default or custom save file and will load to latest save point or to specific save point with custom key.")]
public class LoadGamePoint : Order
{
    [Tooltip("The key to load the game from - if none is provided then we use default")]
    [SerializeField] protected string saveKey = LogaConstants.DefaultSaveDataKey;
    [Tooltip("If true, load the game from this specific point when using the save data file provided")]
    [SerializeField] protected bool loadCustomPoint = false;
    [Tooltip("If loading from specific point, provide the key that you wish to load")]
    [SerializeField] protected string customKey = string.Empty;

    public override void OnEnter()
    {
        var saveManager = LogaManager.Instance.SaveManager;

        if (loadCustomPoint)
        {
            //Get the latest save data and find the custom point
            //If the custom point is found, load the game from that point
        }
        else
        {
            if (string.IsNullOrEmpty(saveManager.StartScene))
            {
                saveManager.StartScene = SceneManager.GetActiveScene().name;
            }
            if (saveManager.HasSaveData(saveKey))
            {
                saveManager.Load(saveKey);
            }
        }
    }

    public override string GetSummary()
    {
        string summary = "Load game from ";
        if (saveKey != LogaConstants.DefaultSaveDataKey)
        {
            summary  += saveKey;
        }
        else
        {
            summary += "default save data";
        }
        if (loadCustomPoint)
        {
            summary += "custom point: " + customKey;
        }
        return summary;

    }

    public override Color GetButtonColour()
    {
        return new Color32(235, 191, 217, 255);
    }
}
