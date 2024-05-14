using MoreMountains.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AchievementItem : MonoBehaviour
{
    [SerializeField] protected TextMeshProUGUI achievementTitle;
    [SerializeField] protected TextMeshProUGUI achievementDesc;
    [SerializeField] protected Image achievementImage;
    [SerializeField] protected Image achievementBackground;
    [SerializeField] protected Color unlockedColor = Color.green;
    [SerializeField] protected Color lockedColor = Color.red;


    [HideInInspector]
    [SerializeField] protected MMAchievementList achievementList;
    [HideInInspector]
    public string achievementID;


    public virtual void SetAchievement(string title, string description, Sprite image, string ID, MMAchievementList _list)
    {
        achievementTitle.text = title;
        achievementDesc.text = description;
        achievementImage.sprite = image;
        this.achievementID = ID;
        achievementList = _list;
        achievementBackground.color = lockedColor;
    }

    protected virtual void Update()
    {
        if (achievementList == null)
        {
            return;
        }

        foreach (MMAchievement achievement in MMAchievementManager.AchievementsList)
        {
            if (achievement.AchievementID == achievementID)
            {
                if (achievement.ProgressTarget > 1)
                {
                    if (!achievementDesc.text.Contains(achievement.ProgressCurrent + "/" + achievement.ProgressTarget))
                    {
                        achievementDesc.text += "\t\t\t\t\t" + achievement.ProgressCurrent + "/" + achievement.ProgressTarget;
                    }
                }

                if (achievement.UnlockedStatus)
                {
                    achievementDesc.text = "";
                    achievementDesc.text = achievement.Description;
                    achievementDesc.text += "\t\t\t\t\t" + achievement.ProgressCurrent + "/" + achievement.ProgressTarget;
                    SetCompleteAchievement();
                }
            }
        }
    }

    private void SetCompleteAchievement()
    {
        //set the background color to green
        achievementBackground.color = unlockedColor;
        //set the icon to unlocked?
    }
}
