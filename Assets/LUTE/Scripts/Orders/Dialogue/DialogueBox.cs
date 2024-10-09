using LoGaCulture.LUTE;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Display story text in a visual novel style Dialogue box.
/// </summary>
/// 
public class DialogueBox : MonoBehaviour, IPointerClickHandler
{
    [Tooltip("The continue button UI object")]
    [SerializeField] protected Button continueButton;
    [Tooltip("The text UI object")]
    [SerializeField] protected TextMeshProUGUI textDisplay;
    [Tooltip("The name text UI object")]
    [SerializeField] protected TextMeshProUGUI nameText;
    [Tooltip("Duration to fade dialogue in/out")]
    [SerializeField] protected float fadeDuration = 0.25f;
    [SerializeField] protected string storyText = "";
    [Tooltip("Close any other open dialogue boxes when this one is active")]
    [SerializeField] protected bool closeOtherDialogues;
    [Tooltip("The character UI object")]
    [SerializeField] protected Image characterImage;
    [Tooltip("Adjust width of story text when Character Image is displayed (to avoid overlapping)")]
    [SerializeField] protected bool fitTextWithImage = true;
    [Tooltip("Allow clicking anywhere to proceed to next text or make users click the box to progress")]
    [SerializeField] protected bool allowClickAnywhere = false;
    [Tooltip("Use a button to progress the text - this button is set on the dialogue box prefab")]
    [SerializeField] protected bool buttonToProgress = false;

    public virtual Image CharacterImage { get { return characterImage; } }

    protected TextWriter writer;
    protected CanvasGroup canvasGroup;
    protected bool fadeWhenDone = true;
    protected bool waitForClick = true;
    protected float targetAlpha = 0f;
    protected float fadeCoolDownTimer = 0f;
    // Cache active boxes to avoid expensive scene search
    protected static List<DialogueBox> activeDialogueBoxes = new List<DialogueBox>();
    protected Sprite currentCharacterImage;
    protected float startStoryTextWidth;
    protected float startStoryTextInset;
    protected static Character speakingCharacter;

    public virtual RectTransform StoryTextRectTrans
    {
        get
        {
            return storyText != null ? textDisplay.rectTransform : textDisplay.GetComponent<RectTransform>();
        }
    }

    protected virtual void Awake()
    {
        if (!activeDialogueBoxes.Contains(this))
        {
            activeDialogueBoxes.Add(this);
        }
    }

    protected virtual void OnDestroy()
    {
        activeDialogueBoxes.Remove(this);
    }

    protected virtual TextWriter GetWriter()
    {
        if (writer != null)
        {
            return writer;
        }

        writer = GetComponent<TextWriter>();
        if (writer == null)
        {
            writer = gameObject.AddComponent<TextWriter>();
        }

        return writer;
    }

    protected virtual TextMeshProUGUI GetTextDisplay()
    {
        if (textDisplay != null)
        {
            return textDisplay;
        }

        textDisplay = GetComponentInChildren<TextMeshProUGUI>();
        if (textDisplay == null)
        {
            textDisplay = gameObject.AddComponent<TextMeshProUGUI>(); //may need to be added onto a canvas group for correct displaying
        }

        return textDisplay;
    }

    //this will conflict with the above method, so we'll comment it out for now
    // protected virtual TextMeshProUGUI GetTextNameDisplay()
    // {
    //     if (nameText != null)
    //     {
    //         return nameText;
    //     }

    //     nameText = GetComponentInChildren<TextMeshProUGUI>();
    //     if (nameText == null)
    //     {
    //         nameText = gameObject.AddComponent<TextMeshProUGUI>(); //may need to be added onto a canvas group for correct displaying
    //     }

    //     return textDisplay;
    // }

    protected virtual CanvasGroup GetCanvasGroup()
    {
        if (canvasGroup != null)
        {
            return canvasGroup;
        }

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        return canvasGroup;
    }

    protected virtual void Start()
    {
        // Dialogue always starts invisible, will be faded in when writing starts
        GetCanvasGroup().alpha = 0f;

        // Add a raycaster if none already exists so we can handle input
        GraphicRaycaster raycaster = GetComponent<GraphicRaycaster>();
        if (raycaster == null)
        {
            gameObject.AddComponent<GraphicRaycaster>();
        }

        if (currentCharacterImage == null)
        {
            // Character image is hidden by default.
            SetCharacterImage(null);
        }
    }

    protected virtual void LateUpdate()
    {
        UpdateAlpha();

        if (continueButton != null)
        {
            //continueButton.gameObject.SetActive(GetWriter().IsWaitingForInput);
        }
    }

    protected virtual void UpdateAlpha()
    {
        if (GetWriter().IsTyping())
        {
            targetAlpha = 1f;
            fadeCoolDownTimer = 0.1f;
        }
        else if (!waitForClick && fadeWhenDone && Mathf.Approximately(fadeCoolDownTimer, 0f))
        {
            targetAlpha = 0f;
        }
        else
        {
            // Add a short delay before we start fading in case there's another text order in the next frame or two
            // This avoids a noticeable flicker between consecutive text orders
            fadeCoolDownTimer = Mathf.Max(0f, fadeCoolDownTimer - Time.deltaTime);
        }

        CanvasGroup canvasGroup = GetCanvasGroup();
        if (fadeDuration <= 0f)
        {
            canvasGroup.alpha = targetAlpha;
        }
        else
        {
            float delta = (1f / fadeDuration) * Time.deltaTime;
            float alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, delta);
            canvasGroup.alpha = alpha;

            if (alpha <= 0f)
            {
                // Deactivate object once invisible
                gameObject.SetActive(false);
            }
        }
    }

    protected virtual void ClearStoryText()
    {
        storyText = "";
    }

    protected virtual void SetButtonClick()
    {
        WriterSignals.WriterClick();
    }

    public virtual void SetStoryText(string text)
    {
        storyText = text;
    }

    /// Currently active Dialogue used to display Say text
    public static DialogueBox ActiveDialogueBox { get; set; }

    public static DialogueBox GetDialogueBox()
    {
        if (ActiveDialogueBox == null)
        {
            DialogueBox dialogueBox = null;
            if (activeDialogueBoxes.Count > 0)
            {
                dialogueBox = activeDialogueBoxes[0];
            }
            if (dialogueBox != null)
            {
                ActiveDialogueBox = dialogueBox;
            }
            if (ActiveDialogueBox == null)
            {
                // Create a new dialogue box
                GameObject prefab = Resources.Load<GameObject>("Prefabs/DialogueBox");
                if (prefab != null)
                {
                    GameObject go = Instantiate(prefab) as GameObject;
                    go.SetActive(false);
                    go.name = "DialogueBox";
                    ActiveDialogueBox = go.GetComponent<DialogueBox>(); ;
                }
            }
        }
        return ActiveDialogueBox;
    }

    public virtual void SetActive(bool state)
    {
        gameObject.SetActive(state);
    }

    public virtual void SetCharacter(Character character)
    {
        if (character == null)
        {
            if (characterImage != null)
            {
                characterImage.gameObject.SetActive(false);
            }
            if (nameText.text != null)
            {
                nameText.text = "";
            }
            speakingCharacter = null;
        }
        else
        {

            string characterName = character.CharacterName;

            if (characterName == "")
            {
                // Use game object name as default
                characterName = character.GetObjectName();
            }

            SetCharacterName(characterName, character.NameColour);
        }
    }

    public virtual void SetCharacterImage(Sprite sprite)
    {
        if (characterImage == null)
            return;

        if (sprite != null)
        {
            characterImage.overrideSprite = sprite;
            characterImage.gameObject.SetActive(true);
            currentCharacterImage = sprite;
        }
        else
        {
            characterImage.gameObject.SetActive(false);

            if (startStoryTextWidth != 0)
            {
                StoryTextRectTrans.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, startStoryTextInset, startStoryTextWidth);
            }
        }

        if (fitTextWithImage && storyText != null && characterImage.gameObject.activeSelf)
        {
            if (Mathf.Approximately(startStoryTextWidth, 0f))
            {
                startStoryTextWidth = StoryTextRectTrans.rect.width;
                startStoryTextInset = StoryTextRectTrans.offsetMin.x;
            }

            if (StoryTextRectTrans.position.x < characterImage.rectTransform.position.x)
            {
                StoryTextRectTrans.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left,
                startStoryTextInset,
                startStoryTextWidth - characterImage.rectTransform.rect.width);
            }
            else
            {
                StoryTextRectTrans.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right,
                startStoryTextInset,
                startStoryTextWidth - characterImage.rectTransform.rect.width);
            }
        }
    }

    public virtual void SetCharacterName(string name, Color color)
    {
        if (color == null)
            color = Color.black;
        if (nameText != null)
        {
            nameText.text = name;
            nameText.color = color;
        }
    }

    public virtual void StartDialogue(float typingSpeed, float waitTime, bool skipLine, bool waitForClick, bool fadeWhenDone, Action onComplete, bool allowClickAnywhere = false, bool useButton = false)
    {
        buttonToProgress = false;
        // you may wish to hide your button here also
        if (useButton && continueButton != null)
        {
            buttonToProgress = useButton;

            continueButton.onClick.AddListener(SetButtonClick);
            continueButton.gameObject.SetActive(true);
        }

        StartCoroutine(DoDialogue(onComplete, typingSpeed, waitTime, skipLine, waitForClick, fadeWhenDone, allowClickAnywhere, useButton));
    }

    public virtual IEnumerator DoDialogue(Action onComplete, float typingSpeed, float waitTime, bool skipLine, bool waitForClick, bool fadeWhenDone, bool allowClickAnywhere = false, bool useButton = false)
    {
        var tw = GetWriter();

        if (writer.IsTyping() || writer.IsWaitingForInput)
        {
            tw.Stop();
            while (writer.IsTyping() || writer.IsWaitingForInput)
            {
                yield return null;
            }
        }

        if (closeOtherDialogues)
        {
            for (int i = 0; i < activeDialogueBoxes.Count; i++)
            {
                var db = activeDialogueBoxes[i];
                if (db.gameObject != gameObject)
                {
                    db.SetActive(false);
                }
            }
        }
        gameObject.SetActive(true);

        this.fadeWhenDone = fadeWhenDone;
        this.waitForClick = waitForClick;

        // AudioClip SFX = null;
        // if (VOClip != null)
        // {
        //     //play voiceover clip
        // }

        //get the ui text component
        var displayText = GetTextDisplay();

        tw.WriteText(storyText, displayText, onComplete, typingSpeed, waitTime, skipLine, waitForClick, allowClickAnywhere);
    }

    public virtual bool FadeWhenDone { get { return fadeWhenDone; } set { fadeWhenDone = value; } }
    public virtual bool WaitForClick { get { return waitForClick; } set { waitForClick = value; } }

    /// Stop the dialogue while its writing text
    public virtual void Stop()
    {
        fadeWhenDone = true;
        GetWriter().Stop();
    }

    /// Stops writing text and clears text
    public virtual void Clear()
    {
        ClearStoryText();

        // Kill any active write coroutine
        StopAllCoroutines();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!buttonToProgress)
        {
            WriterSignals.WriterClick();
        }
    }
}
