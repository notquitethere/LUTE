using System.Collections;
using System.Collections.Generic;
using LoGaCulture.LUTE;
using MoreMountains.Tools;
using UnityEngine;

//Fill the achievement list with the achievements from the database
public class AchievementListFiller : MonoBehaviour
{
    [SerializeField] protected MMAchievementList achievementList;
    [SerializeField] protected AchievementItem achievementItem;
    [SerializeField] protected Transform scrollRect;

    public static AchievementListFiller ActiveList;
    public static List<MMAchievement> achievements = new List<MMAchievement>();

    protected static List<AchievementListFiller> activeLists = new List<AchievementListFiller>();
    protected Canvas canvas;

    protected virtual void Awake()
    {
        canvas = GetComponent<Canvas>();
        if (!activeLists.Contains(this))
            activeLists.Add(this);

        canvas.enabled = false;
    }
    protected virtual void OnDestroy()
    {
        activeLists.Remove(this);
    }

    //we update the achievement list in update as we can add achievements at runtime
    protected virtual void Update()
    {
        // Achievements are a combination of regular achievements and the postcard achievements
        foreach(MMAchievement achievement in achievementList.Achievements)
        {
            if (!achievements.Exists(a => a.AchievementID == achievement.AchievementID))
            {
                achievements.Add(achievement);
            }
        }
        PostcardAchievementList postcardAchievementList = null;
        if(achievementList is PostcardAchievementList)
        {
            postcardAchievementList = achievementList as PostcardAchievementList;
        }
        if (postcardAchievementList != null)
        {
            foreach (PostcardAchievement achievement in postcardAchievementList.PostcardAchievements)
            {
                if (!achievements.Exists(a => a.AchievementID == achievement.AchievementID))
                {
                    achievements.Add(achievement);
                }
            }
        }

        foreach (MMAchievement achievement in achievements)
        {
            // Check if the achievement item already exists
            if (!AchievementItemExists(achievement.AchievementID))
            {
                AchievementItem newAchievementItem = Instantiate(achievementItem, scrollRect);
                Sprite achievementSprite = achievement.UnlockedStatus ? achievement.UnlockedImage : achievement.LockedImage;
                newAchievementItem.SetAchievement(achievement.Title, achievement.Description, achievementSprite, achievement.AchievementID, achievements);
            }
        }

        bool AchievementItemExists(string achievementID)
        {
            // Check if an achievement item with the given ID already exists in the scrollRect
            foreach (Transform child in scrollRect)
            {
                AchievementItem achievementItem = child.GetComponent<AchievementItem>();
                if (achievementItem != null && achievementItem.achievementID == achievementID)
                {
                    return true;
                }
            }
            return false;
        }
    }

    public static AchievementListFiller GetList()
    {
        if (ActiveList == null)
        {
            AchievementListFiller list = null;
            if (activeLists.Count > 0)
            {
                list = activeLists[0];
            }
            if (list != null)
                ActiveList = list;
            else
            {
                GameObject listPrefab = Resources.Load<GameObject>("Prefabs/AchievementCanvas");
                if (listPrefab != null)
                {
                    GameObject listObject = Instantiate(listPrefab) as GameObject;
                    listObject.name = "AchievementCanvas";
                    list = listObject.GetComponent<AchievementListFiller>();
                    ActiveList = list;
                }
            }
        }
        return ActiveList;
    }

    public virtual void ShowList()
    {
        canvas.enabled = !canvas.enabled;
    }
}
