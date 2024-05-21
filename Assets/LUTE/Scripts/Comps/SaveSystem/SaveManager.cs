using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_5_3_OR_NEWER

/// <summary>
/// SaveManager is a class that manages the saving and loading of the game.
/// WebGL requires extra js so we use playerprefs
/// webplayer does not implement systemio so requires playerprefs
/// </summary>

public class SaveManager : MonoBehaviour
{
    protected static SaveHistory saveHistory = new SaveHistory();

    public static string SAVE_DIRECTORY  { get { return Application.persistentDataPath + "/LUTESaves/"; } }

    private static string GetFullFilePath(string saveDataKey)
    {
        return SAVE_DIRECTORY + saveDataKey + ".json";
    }

    protected virtual bool ReadSaveHistory(string saveDataKey)
    {
        var historyData = string.Empty;
#if UNITY_WEBPLAYER || UNITY_WEBGL
        historyData = PlayerPrefs.GetString(saveDataKey);
#else
        var filePath = GetFullFilePath(saveDataKey);
        if (System.IO.File.Exists(filePath))
        {
            historyData = System.IO.File.ReadAllText(filePath);
        }
#endif // UNITY_WEBPLAYER || UNITY_WEBGL
        if (!string.IsNullOrEmpty(historyData))
        {
            var tempSaveHistory = JsonUtility.FromJson<SaveHistory>(historyData);
            if (tempSaveHistory != null)
            {
                saveHistory = tempSaveHistory;
                return true;
            }
        }
        return false;
    }

    protected virtual bool WriteSaveHistory(string saveDataKey)
    {
        var historyData = JsonUtility.ToJson(saveHistory, true);
        if(!string.IsNullOrEmpty(historyData))
        {
#if UNITY_WEBPLAYER || UNITY_WEBGL
            PlayerPrefs.SetString(saveDataKey, historyData);
            PlayerPrefs.Save();
#else
            var filePath = GetFullFilePath(saveDataKey);
            System.IO.FileInfo fileInfo = new System.IO.FileInfo(filePath);
            fileInfo.Directory.Create();

            System.IO.File.WriteAllText(filePath, historyData);
#endif // UNITY_WEBPLAYER || UNITY_WEBGL
            return true;
        }
        return false;
    }

    /// <summary>
    /// Executes the node based on save point key in the following order:
    /// 1. Save point loaded using event handler with matching save point key.
    /// 2. First save point order found with matching key; will execute the next order in the list.
    /// 3. Any label in a node with matching key; will execute the next order in the list.
    /// </summary>
    /// <param name="savePointKey"></param>
    protected virtual void ExecuteNodes(string savePointKey)
    {
        // Fire any matching SavePointLoaded event handler with matching save key
        SavePointLoaded.NotifyEventHandlers(savePointKey);

        // Execute any node containing a save point order with matching save key, with resume on load set to true
        var savePoints = Object.FindObjectsOfType<SavePoint>();
        for (int i = 0; i < savePoints.Length; i++)
        {
            var savePoint = savePoints[i];
            if (savePoint.ResumeOnLoad &&
                string.Compare(savePoint.SavePointKey, savePointKey, true) == 0)
            {
                int index = savePoint.OrderIndex;
                var node = savePoint.ParentNode;
                var engine = savePoint.GetEngine();
                engine.ExecuteNode(node, index + 1);

                // Should only be one save point with the same key
                break;
            }
        }
    }

    /// Start executing the first save found with isStartingPoint set to true
    protected virtual void ExecuteStartNode()
    {
        var savePoints = Object.FindObjectsOfType<SavePoint>();
        for (int i = 0; i < savePoints.Length; i++)
        {
            var savePoint = savePoints[i];
            if (savePoint.IsStartingPoint)
            {
                var node = savePoint.ParentNode;
                var engine = savePoint.GetEngine();
                engine.ExecuteNode(node, savePoint.OrderIndex);
                break;
            }
        }
    }

    protected virtual void LoadSaveGame(string saveDataKey)
    {
        if(ReadSaveHistory(saveDataKey))
        {
            saveHistory.ClearRewoundSavePoints();
            saveHistory.LoadLastSavePoint();
        }
    }

    // Scene loading in Unity is asynchronous so we need to take care to avoid race conditions. 
    // The following callbacks tell us when a scene has been loaded and when 
    // a saved game has been loaded. We delay taking action until the next 
    // frame (via a delegate) so that we know for sure which case we're dealing with.

    public virtual void AddSavePoint(string savePointKey, string savePointDescription)
    { }
}
#endif // UNITY_5_3_OR_NEWER