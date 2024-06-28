using MoreMountains.Feedbacks;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Base class to hold information and methods for a postcard in the sticker game system
/// </summary>
public class Postcard : MonoBehaviour
{
    [Tooltip("The name of the postcard")]
    [SerializeField] protected string postcardName;
    [Tooltip("The description of the postcard")]
    [SerializeField] protected string postcardDescription;
    [Tooltip("The creator of the postcard")]
    [SerializeField] protected string postcardCreator;
    [Tooltip("The total number of stickers that can be on this postcard")]
    [SerializeField] protected int totalStickers;
    protected static List<Postcard> activePostcards = new List<Postcard>();
    [Tooltip("The text that will be displayed for the postcard name")]
    [SerializeField] protected TextMeshProUGUI nameText;
    [Tooltip("The text that will be displayed for the postcard description")]
    [SerializeField] protected TextMeshProUGUI descriptionText;
    [Tooltip("The text that will be displayed for the postcard creator")]
    [SerializeField] protected TextMeshProUGUI creatorText;
    [SerializeField] protected Animator anim;

    // The stickers that are on this postcard
    private List<Sticker> stickers = new List<Sticker>();
    // The layout group that the stickers will be placed in
    private Transform stickerCanvas;
    // Whether the postcard has been discarded recently
    private bool isDiscarded;
    // Whether the postcard has been flipped
    private bool isFlipped;

    /// <summary>
    /// Returns the progress of the postcard (how many stickers are on it and their type based on the StickerManager achievement list)
    private int PostcardProgress()
    {
        return 0;
    }

    public static Postcard ActivePostcard { get; set; }

    protected virtual void Awake()
    {
        if (!activePostcards.Contains(this))
        {
            activePostcards.Add(this);
        }
    }

    public static Postcard GetPostcard(string name, string description, StickerItem sticker, string creator)
    {
        if (ActivePostcard == null)
        {
            Postcard postcard = null;
            if (activePostcards.Count > 0)
            {
                postcard = activePostcards[0];
            }
            if (postcard != null)
            {
                ActivePostcard = postcard;
            }
            if (ActivePostcard == null)
            {
                // Create a new postcard 
                var postCardPrefab = Resources.Load<GameObject>("Prefabs/BlankPostcard");
                if (postCardPrefab != null)
                {
                    var postcardGO = Instantiate(postCardPrefab).GetComponent<Postcard>();
                    postcardGO.name = name;
                    postcardGO.postcardName = name;
                    postcardGO.postcardDescription = description;
                    postcardGO.postcardCreator = creator;
                    postcardGO.stickerCanvas = postcardGO.GetComponentInChildren<GridLayoutGroup>().transform;
                    postcardGO.SetPostcardText(name, description, creator);
                    return postcardGO;
                }
            }
        }
        if(ActivePostcard.isDiscarded)
        {
            ActivePostcard.SetPostcardBase(name, description, creator);
            ActivePostcard.SetPostcardText(name, description, creator);
        }
        return ActivePostcard;
    }

    private void SetPostcardBase(string name, string desc, string creator)
    {
        if(ActivePostcard != null)
        {
            ActivePostcard.name = name;
            ActivePostcard.postcardName = name;
            ActivePostcard.postcardDescription = desc;
            ActivePostcard.postcardCreator = creator;
        }

        ActivePostcard.isDiscarded = false;
    }

    private void SetPostcardText(string name, string desc, string creator)
    {
        if(nameText != null)
        {
            nameText.text = name;
        }
        if (descriptionText != null)
        {
            descriptionText.text = desc;
        }
        if (creatorText != null)
        {
            creatorText.text = "Create By " + creator;
        }
    }

    public virtual void SetActive(bool state)
    {
        gameObject.SetActive(state);
    }

    public Sticker AddSticker(StickerItem sticker)
    {
        if(sticker == null)
            return null;
        if (stickerCanvas == null)
            return null;
        // Find the sticker prefab in the resources
        var stickerPrefab = Resources.Load<GameObject>("Prefabs/BlankSticker");
        if (stickerPrefab == null)
            return null;
        // Instantiate the sticker prefab and set its parent to the sticker canvas
        var stickerInstance = Instantiate(stickerPrefab, stickerCanvas).GetComponent<Sticker>();
        // Use sticker class to set image and name etc of the sticker (i.e., initialise the sticker)
        stickerInstance.Initialise(sticker);

        // Add the sticker to the list of stickers
        stickers.Add(stickerInstance);

        return stickerInstance;
    }

    public Sticker RemoveSticker(Sticker sticker)
    {
        if (sticker == null)
            return null;
        stickers.Remove(sticker);
        return sticker;
    }
    public bool SubmitDesign()
    {
        return StickerManager.SubmitDesign(stickers);
    }

    public void Discard(MMFeedbacks discardFeedback)
    {
        isDiscarded = true;
        foreach(var sticker in stickers)
        {
            Destroy(sticker.gameObject);
        }
        stickers.Clear();
        postcardName = "";
        postcardDescription = "";

        if(isFlipped)
        {
            FlipPostcard();
            stickerCanvas.transform.Rotate(0, -180, 0);
        }

        discardFeedback?.PlayFeedbacks();
        SetActive(false);
    }

    public void FlipPostcard()
    {
        if(nameText == null && descriptionText == null && creatorText == null)
        {
            return;
        }

        var descriptorGroup = nameText.transform.parent.GetComponent<CanvasGroup>();
        if(descriptorGroup == null)
        {
            return;
        }

        if (!isFlipped)
        {
            if (anim != null)
            {
                anim.SetTrigger("Flip");
            }

            foreach (var sticker in stickers)
            {
                sticker.FlipSticker();
            }
            descriptorGroup.alpha = 1;
            isFlipped = true;
        }
        else
        {
            if (anim != null)
            {
                anim.SetTrigger("Unflip");
            }

            foreach (var sticker in stickers)
            {
                sticker.FlipSticker();
            }
            descriptorGroup.alpha = 0;
            isFlipped = false;
        }
    }
}