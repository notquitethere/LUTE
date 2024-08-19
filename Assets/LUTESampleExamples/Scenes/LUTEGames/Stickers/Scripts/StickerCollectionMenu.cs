using UnityEditor;
using UnityEngine;

namespace LoGaCulture.LUTE
{
    public class StickerCollectionMenu : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            var items = GetAllInstances<StickerItem>();
            foreach (var item in items)
            {
                Debug.Log(item.ItemName);

                // Have a locked and unlocked state
                // Check against inventory to see if item exists then set state
            }
        }

        public static T[] GetAllInstances<T>() where T : ScriptableObject
        {
            string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name); //FindAssets uses tags check documentation for more info
            T[] instances = new T[guids.Length];
            for (int i = 0; i < guids.Length; i++) //probably could get optimized
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                instances[i] = AssetDatabase.LoadAssetAtPath<T>(path);
            }

            return instances;
        }
    }
}
