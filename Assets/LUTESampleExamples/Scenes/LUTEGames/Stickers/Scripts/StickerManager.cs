using System.Collections.Generic;
using UnityEngine;

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
    [SerializeField] protected Postcard[] postcards;
    [Tooltip("The list of achievements saved in the game")]
    [SerializeField] protected Achievement[] achievements;

    public Postcard GetPostcard(int index)
    {
        if (index < 0 || index >= postcards.Length)
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

    public static bool SubmitDesign(List<Sticker> stickerList)
    {
        //Check the list of achievements and see if the stickers match any of them
        //Return true or false but also save this postcard in the list of postcards
        return false;
    }
}