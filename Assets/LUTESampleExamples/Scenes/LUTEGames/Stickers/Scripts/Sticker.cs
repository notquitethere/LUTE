using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Base class to hold information and methods for a sticker in the game
/// </summary>
public class Sticker : MonoBehaviour, IDragHandler
{
    [Tooltip("The category that this sticker belongs to")]
    [SerializeField] protected StickerManager.StickerType stickerType;
    [Tooltip("The name of the sticker")]
    [SerializeField] protected string stickerName;
    [Tooltip("The description of the sticker")]
    [SerializeField] protected string stickerDescription;
    [Tooltip("The image of the sticker")]
    [SerializeField] protected Sprite stickerImage;

    protected RectTransform stickerRect;

    public RectTransform StickerRect { get { return stickerRect; } set { stickerRect = value; } }

    private bool Moveable() => true;
    private bool Rotatable() => true;
    private bool Scaleable() => true;

    private bool isFlipped;

    public Sticker Initialise(StickerItem sticker)
    {
        if (sticker == null)
            return null;

        stickerName = sticker.ItemName;
        stickerDescription = sticker.ShortDescription;
        stickerType = sticker.StickerType;
        stickerImage = sticker.Icon;

        if (stickerImage != null)
            SetStickerIcon();

        this.name = stickerName;
        return this;
    }

    private void SetStickerIcon()
    {
        var image = GetComponent<UnityEngine.UI.Image>();
        if (image == null)
            return;

        image.sprite = stickerImage;
    }

    public void FlipSticker()
    {
        var image = GetComponent<UnityEngine.UI.Image>();
        if (image == null)
            return;

        isFlipped = !isFlipped;

        if (isFlipped)
        {
            image.enabled = false;
        }
        else
        {
            image.enabled = true;
        }
    }    

    public void OnDrag(PointerEventData eventData)
    {
        var parentCard = transform.parent;
        bool canMove = Moveable();
        if (parentCard == null)
            return;
        if (canMove)
        {
            transform.position = eventData.position;
        }    
    }
}