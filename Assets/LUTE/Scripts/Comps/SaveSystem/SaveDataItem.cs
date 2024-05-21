using UnityEngine;

/// Container for saved data items.
/// Data and the type are stored as strings which are typically strings in JSON representing save objects.
[System.Serializable]
public class SaveDataItem : MonoBehaviour
{
    [SerializeField] protected string data = "";
    [SerializeField] protected string type = "";

    public string Data { get { return data; } set { data = value; } }
    public string Type { get { return type; } set { type = value; } }

    /// Creates a new SaveDataItem with the given data and type.
    public static SaveDataItem Create(string _data, string _type)
    {
        var item = new SaveDataItem();
        item.Data = _data;
        item.Type = _type;

        return item;
    }
}