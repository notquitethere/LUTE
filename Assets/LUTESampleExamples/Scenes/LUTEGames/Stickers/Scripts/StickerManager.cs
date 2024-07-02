using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Contains lists of postcards and achievements related to the sticker system
/// </summary>
public class StickerManager : MonoBehaviour
{
    public enum StickerType
    {
        None,
        Animal,
        Nature
    }

    [Tooltip("The list of postcards saved in the game")]
    [SerializeField] protected List<Postcard> postcards = new List<Postcard>();
    [Tooltip("The list of achievements saved in the game")]
    [SerializeField] protected Achievement[] achievements;

    public BasicFlowEngine engine;
    public Postcard GetPostcard(int index)
    {
        if (index < 0 || index >= postcards.Count)
            return null;
        return postcards[index];
    }

    public Achievement GetAchievement(int index)
    {
        if (index < 0 || index >= achievements.Length)
            return null;
        return achievements[index];
    }

    public List<Postcard> GetPostcards()
    {
        return new List<Postcard>(postcards);
    }

    public List<Achievement> GetAchievements()
    {
        return new List<Achievement>(achievements);
    }

    public void LoadPostCard(int index)
    {
        SetPostcard(index);
    }

    private Postcard SetPostcard(int index)
    {
        // Need to ensure index is not out of bounds of engine postcard list count
        var _postcard = engine.Postcards[index];
        var postcard = Postcard.SetStickers(_postcard, false);
        
        if(postcard == null)
            return null;

        postcard.SetActive(true);
        return postcard;
    }

    public Sticker AddStickerComponent()
    {
        var sticker = this.AddComponent<Sticker>();
        return sticker;
    }

    public bool SubmitDesign(Postcard postcard)
    {
        if (postcard == null)
            return false;

        if (engine == null)
            return false;

        //var postcards = engine.Postcards;
        //// Find the postcard with the same matching name
        //var matchingPostcard = postcards.FirstOrDefault(p => p.PostcardName == postcard.PostcardName);
        //if (matchingPostcard != null)
        //{
        //    Debug.Log("match");
        //    return false;
        //}

        var newPostcard = engine.SetPostcard(postcard);

        var saveManager = LogaManager.Instance.SaveManager;
        saveManager.AddSavePoint("Postcards" + postcard.PostcardName, "A list of postcards to be stored");

        // Check the list of achievements and see if the stickers match any of them
        return false;
    }
}