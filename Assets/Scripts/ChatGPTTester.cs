using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatGPTTester : MonoBehaviour
{
    [SerializeField]
    private Button askButton;

    [SerializeField]
    private Button compilerButton;

    [SerializeField]
    private TextMeshProUGUI responseTimeText;

    [SerializeField]
    private TextMeshProUGUI chatGPTAnswer;

    [SerializeField]
    private TextMeshProUGUI chatGPTQuestionText;

    [SerializeField]
    private ChatGPTQuestion chatGPTQuestion;

    private string gptPrompt;

    [SerializeField]
    private TextMeshProUGUI scenarioTitleText;

    [SerializeField]
    private TextMeshProUGUI scenarioQuestionText;

    [SerializeField]
    private bool immediateCompilation = false;
    
    [Space]
    
    [Header("ChatGPT Response Objects")]
    [SerializeField]
    private ChatGPTResponse chatGPTResponseCache;

    public Color CompileButtonColor
    {
        set
        {
            compilerButton.GetComponent<Image>().color = value;
        }
    }

    private void Awake()
    {
        responseTimeText.text = string.Empty;
        compilerButton.interactable = false;

        // Step 1 - Ask a question (request payload) > call execute
        askButton.onClick.AddListener(() =>
        {
            compilerButton.interactable = false;
            CompileButtonColor = Color.white;
            Execute();
        });
    }

    public void Execute()
    {
        // ui
        gptPrompt = $"{chatGPTQuestion.promptPrefixConstant} {chatGPTQuestion.prompt}";
        scenarioTitleText.text = chatGPTQuestion.scenarioTitle;
        askButton.interactable = false;
        ChatGPTProgress.Instance.StartProgress("Generating source code please wait");

        // animations
        ChatGPTAssistant.Instance.SetCharacterAssistantRoll(true);
        
        // handle replacements
        Array.ForEach(chatGPTQuestion.replacements, r =>
        {
            gptPrompt = gptPrompt.Replace("{" + $"{r.replacementType}" + "}", r.value);
        });

        // handle reminders
        if (chatGPTQuestion.reminders.Length > 0)
        {
            gptPrompt += $", {string.Join(',', chatGPTQuestion.reminders)}";
        }
        scenarioQuestionText.text = gptPrompt;

        // Step 2 - communicate with ChatGPT API (outgoing request)
        StartCoroutine(ChatGPTClient.Instance.Ask(gptPrompt, (response) =>
        {
            // UI
            askButton.interactable = true;
            CompileButtonColor = Color.green;
            compilerButton.interactable = true;
            chatGPTResponseCache = response;
            responseTimeText.text = $"Time: {response.ResponseTotalTime} ms";
            ChatGPTProgress.Instance.StopProgress();
            Logger.Instance.LogInfo(chatGPTResponseCache.SourceCode);

            // Step 3 - (incoming request) - send explanations to a TTS provider
            ChatGPTAssistant.Instance.ChatGPTAISpeak(response.Explanation);

            // immediate compilation optional
            if (immediateCompilation)
                ProcessAndCompileResponse();
        }));
    }

    // Step 4 - (optional) - compile source code received by ChatGPT API
    public void ProcessAndCompileResponse() => RoslynCodeRunner.Instance.RunCode(chatGPTResponseCache.SourceCode);
}