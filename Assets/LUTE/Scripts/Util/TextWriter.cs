using LoGaCulture.LUTE;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WriterState
{
    Invalid,
    Start,
    Pause,
    Resume,
    End,
}

public class TextWriter : MonoBehaviour
{
    //need to hook this up properly
    public virtual bool IsWaitingForInput { get { return isWaitingForInput; } }

    protected bool isWaitingForInput;
    protected List<IWriterListener> writerListeners = new List<IWriterListener>();


    private Coroutine displayRoutine;
    private bool allowSkippingLine = false;
    private bool clicked = false;
    private bool isTyping;
    private bool waitForClick;
    private bool allowClickAnywhere = false;
    private Action onComplete;

    protected TextAdapter textAdapter = new TextAdapter();

    public bool IsTyping { get { return IsTyping; } }

    protected int visibleCharacterCount = 0;
    protected int readAheadStartIndex = 0;
    public AudioWriter AttachedAudioWriter { get; set; }


    protected virtual void OnEnable()
    {
        LoGaCulture.LUTE.WriterSignals.OneWriterClick += OnWriterClick;
    }
    protected virtual void OnDisable()
    {
        LoGaCulture.LUTE.WriterSignals.OneWriterClick -= OnWriterClick;
    }

    protected virtual void Awake()
    {
        // Cache the list of child writer listeners
        var allComponents = GetComponentsInChildren<Component>();
        for (int i = 0; i < allComponents.Length; i++)
        {
            var component = allComponents[i];
            IWriterListener writerListener = component as IWriterListener;
            if (writerListener != null)
            {
                writerListeners.Add(writerListener);
            }
        }
    }

    protected virtual void NotifyStart(AudioClip clip)
    {
        WriterSignals.DoWriterState(this, WriterState.Start);
        for (int i = 0; i < writerListeners.Count; i++)
        {
            var writerListener = writerListeners[i];
            writerListener.OnStart(clip);
        }
    }

    protected virtual void NotifyGlyph()
    {
        for (int i = 0; i < writerListeners.Count; i++)
        {
            var writerListener = writerListeners[i];
            writerListener.OnGlyph();
        }
    }

    /// <summary>
    /// Writes text using a typewriter effect to a UI text object.
    /// </summary>
    /// <param name="text">Text to be written</param>
    /// <param name="clear">If true clears the previous text.</param>
    /// <param name="waitForInput">Writes the text and then waits for player input before calling onComplete.</param>
    /// <param name="stopAudio">Stops any currently playing audioclip.</param>
    /// <param name="waitForVO">Wait for the Voice over to complete before proceeding</param>
    /// <param name="clip">Audio clip to play when text starts writing.</param>
    /// <param name="onComplete">Callback to call when writing is finished.</param>
    public virtual IEnumerator Write(string text, bool clear, bool waitForInput, bool stopAudio, bool waitForVO, AudioClip clip, System.Action onComplete)
    {
        if (clear)
        {
            textAdapter.Text = "";
            visibleCharacterCount = 0;
        }

        if (!textAdapter.HasTextObject())
        {
            yield break;
        }

        NotifyStart(clip);

        string tokenText = TextVariationHandler.SelectVariations(text);

        if (waitForInput)
        {
            tokenText += "{wi}";
        }

        if (waitForVO)
        {
            tokenText += "{wvo}";
        }

        //List<TextTagToken> tokens = TextTagParser.Tokenize(tokenText);

        gameObject.SetActive(true);
        //yield return StartCoroutine(ProcessTokens(tokens, stopAudio, onComplete));

    }

    //public void WriteText(
    //    string text,
    //    TextMeshProUGUI textUI,
    //    Action onComplete,
    //    float typingSpeed,
    //    float waitTime,
    //    bool skipLine,
    //    bool waitForClick,
    //    bool allowClickAnywhere = false
    //)
    //{
    //    this.onComplete = onComplete;
    //    this.allowSkippingLine = skipLine;
    //    this.waitForClick = waitForClick;
    //    this.allowClickAnywhere = allowClickAnywhere;
    //    if (displayRoutine != null)
    //        StopCoroutine(displayRoutine);
    //    displayRoutine = StartCoroutine(DisplayText(text, textUI, onComplete, waitTime, typingSpeed));
    //}

    //public IEnumerator DisplayText(string text, TextMeshProUGUI textUI, Action onComplete, float waitTime, float typingSpeed = 0.04f)
    //{
    //    isTyping = true;

    //    textUI.text = "";

    //    //can hide your continue icon here if need be

    //    bool addingRichText = false;

    //    foreach (char c in text)
    //    {
    //        if (clicked)
    //        {
    //            textUI.text = text;
    //            clicked = false;
    //            break;
    //        }
    //        NotifyGlyph();
    //        if (c == '<' || addingRichText)
    //        {
    //            addingRichText = true;
    //            textUI.text += c;
    //            if (c == '>')
    //            {
    //                addingRichText = false;
    //            }
    //        }
    //        else
    //        {
    //            textUI.text += c;
    //            yield return new WaitForSeconds(typingSpeed);
    //        }
    //    }

    //    //can show your continue icon here if need be

    //    isTyping = false;

    //    if (!waitForClick)
    //    {
    //        yield return new WaitForSeconds(waitTime);
    //        if (onComplete != null)
    //        {
    //            onComplete();
    //        }
    //    }
    //}

    private void OnWriterClick()
    {
        if (allowClickAnywhere)
            return;

        if (isTyping)
        {
            if (allowSkippingLine)
                clicked = true;
        }
        else
        {
            if (waitForClick)
            {
                if (displayRoutine != null)
                    StopCoroutine(displayRoutine);
                isTyping = false;
                if (DialogueBox.GetDialogueBox() != null && DialogueBox.GetDialogueBox().FadeWhenDone)
                {
                    DialogueBox.GetDialogueBox().FadeWhenDone = true;
                    DialogueBox.GetDialogueBox().WaitForClick = false;
                }
                onComplete?.Invoke();
            }
        }
    }

    private void Update()
    {
        if (!allowClickAnywhere)
            return;

        //when a click is detected 
        //if the text is still typing, skip to the end of the text if allowed
        //if the text is not typing, continue to the next text if allowed
        if (Input.GetMouseButtonDown(0)) // this works on android (perhaps not iOS!)
        {
            if (isTyping)
            {
                if (allowSkippingLine)
                    clicked = true;
            }
            else
            {
                if (waitForClick)
                {
                    StopCoroutine(displayRoutine);
                    isTyping = false;
                    DialogueBox.GetDialogueBox().FadeWhenDone = true;
                    onComplete?.Invoke();
                }
            }
        }
    }

    public void Stop()
    {
        if (isTyping || isWaitingForInput)
        {
            StopCoroutine(displayRoutine);
            isTyping = false;
        }
    }
}