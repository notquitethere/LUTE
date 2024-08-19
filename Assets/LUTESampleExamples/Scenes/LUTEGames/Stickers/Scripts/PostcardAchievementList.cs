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
            bool rulesMet = false;
            if (rule.stickerType == StickerManager.StickerType.None)
            {
                rulesMet = postcard.stickers.Count >= rule.totalStickersRequired;
            }
            else
            {
                int matchingStickersCount = postcard.stickers.Count(s => s.StickerType == rule.stickerType);
                rulesMet = postcard.stickers.Count >= rule.totalStickersRequired && matchingStickersCount >= rule.totalStickersRequired;
            }

            if (rulesMet)
            {
                // check the text then return based on this
                if (rule.checkNameText)
                {
                    if (postcard.PostcardName.Contains(rule.nameTextContains) || string.IsNullOrEmpty(rule.nameTextContains) && !postcard.PostcardName.Contains(rule.nameTextDoesNotContain) || string.IsNullOrEmpty(rule.nameTextDoesNotContain))
                    {
                        rulesMet = true;
                    }
                    else
                    {
                        rulesMet = false;
                    }
                }
                if (rule.checkAuthorText)
                {
                    if (postcard.PostcardCreator.Contains(rule.authorTextContains) || string.IsNullOrEmpty(rule.authorTextContains) && !postcard.PostcardCreator.Contains(rule.authorTextDoesNotContain) || string.IsNullOrEmpty(rule.authorTextDoesNotContain))
                    {
                        rulesMet = true;
                    }
                    else
                    {
                        rulesMet = false;
                    }
                }
                if (rule.checkDescText)
                {
                    if (postcard.PostcardDesc.Contains(rule.descTextContains) || string.IsNullOrEmpty(rule.descTextContains) && !postcard.PostcardDesc.Contains(rule.descTextDoesNotContain) || string.IsNullOrEmpty(rule.descTextDoesNotContain))
                    {
                        rulesMet = true;
                    }
                    else
                    {
                        rulesMet = false;
                    }
                }
            }

            return rulesMet;
        }
    }
}