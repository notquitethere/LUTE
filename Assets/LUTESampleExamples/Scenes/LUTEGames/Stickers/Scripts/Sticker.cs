using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Base class to hold information and methods for a sticker in the game
/// </summary>
public class Sticker : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Tooltip("The category that this sticker belongs to")]
    [SerializeField] protected StickerManager.StickerType stickerType;
    [Tooltip("The name of the sticker")]
    [SerializeField] protected string stickerName;
    [Tooltip("The description of the sticker")]
    [SerializeField] protected string stickerDescription;
    [Tooltip("The image of the sticker")]
    [SerializeField] protected Sprite stickerImage;
    [Tooltip("The speed at which the sticker rotates")]
    [SerializeField] protected float rotatespeed = 100f;
    [Tooltip("The maximum scale of the sticker")]
    [SerializeField] protected float maxScale = 2f;
    [Tooltip("The minimum scale of the sticker")]
    [SerializeField] protected float minScale = 0.5f;
    [Tooltip("The speed at which the sticker scales")]
    [SerializeField] protected float scaleSpeed = 0.1f;
    [Tooltip("Feedback to be played when placing sticker")]
    [SerializeField] protected MMFeedbacks placeStickerFeedback;
    protected RectTransform binIcon;
    protected Postcard stickerPostcard;

    protected Vector3 stickerPos;
    protected Vector3 stickerScale;
    protected Quaternion stickerRot;

    public StickerManager.StickerType StickerType { get { return stickerType; } set { stickerType = value; } }
    public string StickerName { get { return stickerName; } set {  stickerName = value; } }
    public string StickerDescription { get { return stickerDescription;  } set { stickerDescription = value; } }
    public Sprite StickerImage { get { return stickerImage; } set { stickerImage = value; } }
    public Vector3 StickerPosition { get { return stickerPos; } set { stickerPos = value; } }
    public Vector3 StickerScale { get {  return stickerScale; } set { stickerScale  = value; } }
    public Quaternion StickerRotation { get { return stickerRot; } set { stickerRot = value; } }

    private bool Moveable() => true;
    private bool Rotatable() => true;
    private bool Scaleable() => true;

    private bool isFlipped;
    private bool mouseOver;

    private RectTransform mrect => GetComponent<RectTransform>();
    private RectTransform parentRect => mrect.parent as RectTransform;

    private void LateUpdate()
    {
        if(mouseOver)
        {
            if(Rotatable())
            {
                if (Input.GetKey(KeyCode.D))
                {
                    transform.Rotate(Vector3.back, rotatespeed * Time.deltaTime);
                    stickerRot = transform.rotation;
                }
                if (Input.GetKey(KeyCode.A))
                {
                    transform.Rotate(Vector3.back, -rotatespeed * Time.deltaTime);
                    stickerRot = transform.rotation;
                }
            }
            if(Scaleable())
            {            
                if (Input.GetKey(KeyCode.W))
                {
                    transform.localScale += Vector3.one * scaleSpeed * Time.deltaTime;
                    stickerScale = transform.localScale;
                }
                if (Input.GetKey(KeyCode.S))
                {
                    transform.localScale -= Vector3.one * scaleSpeed * Time.deltaTime;
                    stickerScale = transform.localScale;
                }
            }
        }

        if (transform.localScale.x > maxScale)
            transform.localScale = new Vector3(maxScale, maxScale, maxScale);
        if (transform.localScale.x < minScale)
            transform.localScale = new Vector3(minScale, minScale, minScale);

        KeepInsideCanvas();
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
        stickerPos = mrect.position;
        stickerScale = transform.localScale;
        stickerRot = transform.rotation;
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
        mrect.position = sticker.stickerPos;
        transform.localScale = sticker.stickerScale;
        transform.rotation = sticker.stickerRot;
        this.name = stickerName;

        stickerPos = mrect.position;
        stickerScale = transform.localScale;
        stickerRot = transform.rotation;

        if (stickerImage != null)
            SetStickerIcon();


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
        mrect.position = sticker.Position;
        transform.localScale = sticker.StickerScale;
        transform.rotation = sticker.StickerRot;
        this.name = stickerName;

        stickerPos = mrect.position;
        stickerScale = transform.localScale;
        stickerRot = transform.rotation;

        if (stickerImage != null)
            SetStickerIcon();

        return this;
    }

    private void SetStickerIcon()
    {
        var image = GetComponent<UnityEngine.UI.Image>();
        if (image == null)
            return;

        image.sprite = stickerImage;
    }

    public void SetBinIcon(RectTransform binIcon)
    {
        this.binIcon = binIcon;
    }

    public void SetPostcard(Postcard postcard)
    {
        stickerPostcard = postcard;
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
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        binIcon?.gameObject.SetActive(true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        var parentCard = transform.parent;
        bool canMove = Moveable();
        if (parentCard == null)
            return;
        if (canMove)
        {
            mrect.position = eventData.position;
            stickerPos = eventData.position;

            if (RectTransformUtility.RectangleContainsScreenPoint(binIcon, eventData.position, eventData.pressEventCamera))
            {
                stickerPostcard.PlayBinFeedback();
            }
            else
            {
                stickerPostcard.StopBinFeedback();
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Check mouse position to see if it is over the bin icon
        if (RectTransformUtility.RectangleContainsScreenPoint(binIcon, eventData.position, eventData.pressEventCamera))
        {
            // Delete the sticker
            stickerPostcard?.RemoveSticker(this);
        }
        else
        {
            // If not deleting then play nice feedback for placing a sticker down
            placeStickerFeedback?.PlayFeedbacks();
        }
        // Finally hide the bin icon
        binIcon?.gameObject.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        mouseOver = true;
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        mouseOver = false;
    }

    void KeepInsideCanvas()
    {

        Vector2 apos = mrect.anchoredPosition;

        // Get the width and height of mrect
        float mrectWidth = mrect.rect.width;
        float mrectHeight = mrect.rect.height;

        // X-axis clamping
        float xpos = apos.x;
        float leftEdge = -parentRect.rect.width + mrectWidth / 2;
        float rightEdge = parentRect.rect.width - mrectWidth / 2;
        xpos = Mathf.Clamp(xpos, leftEdge, rightEdge);

        // Y-axis clamping
        float ypos = apos.y;
        float bottomEdge = -parentRect.rect.height + mrectHeight / 2;
        float topEdge = parentRect.rect.height - mrectHeight / 2;
        ypos = Mathf.Clamp(ypos, bottomEdge, topEdge);

        // Apply the clamped positions
        apos.x = xpos;
        apos.y = ypos;
        mrect.anchoredPosition = apos;
    }
}