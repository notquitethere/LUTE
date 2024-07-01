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

    protected Vector3 stickerPos;

    public StickerManager.StickerType StickerType { get { return stickerType; } set { stickerType = value; } }
    public string StickerName { get { return stickerName; } set {  stickerName = value; } }
    public string StickerDescription { get { return stickerDescription;  } set { stickerDescription = value; } }
    public Sprite StickerImage { get { return stickerImage; } set { stickerImage = value; } }
    public Vector3 StickerPosition { get { return stickerPos; } set { stickerPos = value; } }

    private bool Moveable() => true;
    private bool Rotatable() => true;
    private bool Scaleable() => true;

    private bool isFlipped;

    public Sticker()
    {

    }

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
        stickerPos = transform.position;
        return this;
    }

    public Sticker Initialise(Sticker sticker)
    {
        if(sticker == null)
            return null;

        stickerName = sticker.stickerName;
        stickerDescription = sticker.stickerDescription;
        stickerType = sticker.stickerType;
        stickerImage = sticker.stickerImage;
        transform.position = sticker.stickerPos;

        if (stickerImage != null)
            SetStickerIcon();

        stickerPos = transform.position;
        return this;
    }

    public Sticker Initialise(PostcardVar.StickerVar sticker)
    {
        if (sticker == null)
            return null;

        stickerName = sticker.Name;
        stickerDescription = sticker.Desc;
        stickerType = sticker.Type;
        stickerImage = sticker.Image;
        transform.position = sticker.Position;

        if (stickerImage != null)
            SetStickerIcon();

        stickerPos = transform.position;
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
            stickerPos = eventData.position;
        }    
    }
}