using MoreMountains.Tools;
using UnityEngine;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using MoreMountains.InventoryEngine;

namespace LoGaCulture.LUTE
{
    [System.Serializable]
    public class PostcardAchievement : MMAchievement
    {
        [Header("Postcard Achievement Rules")]
        [Tooltip("Rules are defined by the total stickers required and if a specific sticker type is required; an achievement is unlocked when all rules are met.")]
        public List<CustomAchievement> rules = new List<CustomAchievement>();
        [Tooltip("The stickers that will be given to the player when the achievement is unlocked.")]
        [SerializeField] protected List<StickerItem> unlockedStickers = new List<StickerItem>();
        [Tooltip("Custom event to trigger when the achievement is unlocked.")]
        [SerializeField] protected UnityEngine.Events.UnityEvent OnAchievementUnlocked;

        private BasicFlowEngine engine;
        public override void UnlockAchievement()
        {
            base.UnlockAchievement();
            // Give the player the stickers
            if (engine != null)
            {
                for (int i = 0; i < unlockedStickers.Count; i++)
                {
                    var newItemPicker = engine.gameObject.AddComponent<ItemPicker>();
                    newItemPicker.Item = unlockedStickers[i];
                    newItemPicker.Quantity = 1;
                    newItemPicker.Pick();
                    MMGameEvent.Trigger("Save");
                }
            }
            // Trigger the custom event
            if(OnAchievementUnlocked != null)
            {
                OnAchievementUnlocked.Invoke();
            }
        }

        public void SetEngine(BasicFlowEngine _engine)
        {
            // Set the engine to the current engine
            engine = _engine;
        }
    }

    [System.Serializable]
    public class CustomAchievement
    {
        public int totalStickersRequired;
        public StickerManager.StickerType stickerType;
    }
}
