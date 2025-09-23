using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public struct MessageInfo
{
    public string text; 
    public Color color;

    public MessageInfo(string text, Color color)
    {
        this.text = text;
        this.color = color;
    }
}

public class MessageBox : MonoBehaviour
{
    public static MessageBox instance;
    public TextMeshProUGUI messageText;           // UI Text 組件引用
    public float displayTime = 3f;     // 消息顯示時間（秒）
    public Color defaultColor = Color.white;  // 預設文字顏色
    public TMPro.TextMeshProUGUI titleText; // 獨立的標題 Text
    public float titleDisplayTime = 3f;

    private Queue<MessageInfo> messageQueue = new Queue<MessageInfo>();
    private bool isShowingMessage = false;
    private Coroutine titleCoroutine;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
           //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(instance);
            instance = this;
        }

        // 初始化時清空文字
        if (messageText != null)
        {
            messageText.text = "";
        }
    }

    public void ShowMessage(string message)
    {
        ShowMessage(message, defaultColor);
    }

    public void ShowMessage(string message, Color color)
    {
        MessageInfo messageInfo = new MessageInfo(message, color);
        messageQueue.Enqueue(messageInfo);
        
        if (!isShowingMessage)
        {
            StartCoroutine(ShowMessageSequence());
        }
    }

    private IEnumerator ShowMessageSequence()
    {
        isShowingMessage = true;

        while (messageQueue.Count > 0)
        {
            MessageInfo currentMessage = messageQueue.Dequeue();
            
            // 顯示消息
            if (messageText != null)
            {
                messageText.text = currentMessage.text;
                messageText.color = currentMessage.color;
            }

            // 等待顯示時間
            yield return new WaitForSeconds(displayTime);
        }

        // 清空文字
        if (messageText != null)
        {
            messageText.text = "";
            messageText.color = defaultColor; // 重置顏色為預設值
        }

        isShowingMessage = false;
    }

    public void ShowTitle(string message)
    {
        ShowTitle(message, defaultColor);
    }

    public void ShowTitle(string message, Color color)
    {
        print(message);
        if (titleCoroutine != null)
        {
            StopCoroutine(titleCoroutine);
        }
        titleCoroutine = StartCoroutine(ShowTitleSequence(message, color));
    }

    private IEnumerator ShowTitleSequence(string message, Color color)
    {
        if (titleText != null)
        {
            titleText.text = message;
            titleText.color = color;
        }
        yield return new WaitForSeconds(titleDisplayTime);
        if (titleText != null)
        {
            titleText.text = "";
            titleText.color = defaultColor;
        } 
        titleCoroutine = null; 
    }
}