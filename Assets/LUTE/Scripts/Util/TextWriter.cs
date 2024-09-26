using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

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
    private Action onComplete;

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

    protected virtual void NotifyGlyph()
    {
        for (int i = 0; i < writerListeners.Count; i++)
        {
            var writerListener = writerListeners[i];
            writerListener.OnGlyph();
        }
    }

    public void WriteText(
        string text,
        TextMeshProUGUI textUI,
        Action onComplete,
        float typingSpeed,
        float waitTime,
        bool skipLine,
        bool waitForClick
    )
    {
        this.onComplete = onComplete;
        this.allowSkippingLine = skipLine;
        this.waitForClick = waitForClick;
        if (displayRoutine != null)
            StopCoroutine(displayRoutine);
        displayRoutine = StartCoroutine(DisplayText(text, textUI, onComplete, waitTime, typingSpeed));
    }

    public IEnumerator DisplayText(string text, TextMeshProUGUI textUI, Action onComplete, float waitTime, float typingSpeed = 0.04f)
    {
        isTyping = true;

        textUI.text = "";

        //can hide your continue icon here if need be

        bool addingRichText = false;

        foreach (char c in text)
        {
            if (clicked)
            {
                textUI.text = text;
                clicked = false;
                break;
            }
            NotifyGlyph();
            if (c == '<' || addingRichText)
            {
                addingRichText = true;
                textUI.text += c;
                if (c == '>')
                {
                    addingRichText = false;
                }
            }
            else
            {
                textUI.text += c;
                yield return new WaitForSeconds(typingSpeed);
            }
        }

        //can show your continue icon here if need be

        isTyping = false;

        if (!waitForClick)
        {
            yield return new WaitForSeconds(waitTime);
            if (onComplete != null)
            {
                onComplete();
            }
        }
    }

    private void Update()
    {
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
                    onComplete?.Invoke();
            }
        }
    }

    public bool IsTyping() => isTyping;

    public void Stop()
    {
        if (isTyping || isWaitingForInput)
        {
            StopCoroutine(displayRoutine);
            isTyping = false;
        }
    }
}