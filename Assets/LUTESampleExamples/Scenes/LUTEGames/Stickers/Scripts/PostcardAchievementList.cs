using MoreMountains.Tools;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LoGaCulture.LUTE
{
    [CreateAssetMenu(fileName = "PostcardAchievementList", menuName = "LUTE/Postcard Achievement List")]
    [ExecuteAlways]
    public class PostcardAchievementList : MMAchievementList
    {
        public List<PostcardAchievement> PostcardAchievements = new List<PostcardAchievement>();

        public override void ResetAchievements()
        {
            base.ResetAchievements();
            foreach (PostcardAchievement achievement in PostcardAchievements)
            {
                achievement.ProgressCurrent = 0;
                achievement.UnlockedStatus = false;
            }
        }

        public bool CheckPostcardAchievement(Postcard postcard)
        {
            foreach (PostcardAchievement achievement in PostcardAchievements)
            {
                if (!achievement.UnlockedStatus)
                {
                    bool allRulesMet = true;
                    foreach (CustomAchievement rule in achievement.rules)
                    {
                        if (!CheckRule(rule, postcard))
                        {
                            allRulesMet = false;
                            break;
                        }
                    }

                    if (allRulesMet)
                    {
                        var engine = postcard.Manager.engine;
                        achievement.SetEngine(engine);
                        achievement.UnlockAchievement();
                        return true;
                    }
                }
            }
            return false;
        }

        private bool CheckRule(CustomAchievement rule, Postcard postcard)
        {
            if (rule.stickerType == StickerManager.StickerType.None)
            {
                return postcard.stickers.Count >= rule.totalStickersRequired;
            }
            else
            {
                int matchingStickersCount = postcard.stickers.Count(s => s.StickerType == rule.stickerType);
                return postcard.stickers.Count >= rule.totalStickersRequired && matchingStickersCount >= rule.totalStickersRequired;
            }
        }
    }
}