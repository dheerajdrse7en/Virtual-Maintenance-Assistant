using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;

public class GeminiAssistant : MonoBehaviour
{
    public TMP_InputField chatInputField;
    public TextMeshProUGUI chatResponseText;
    public Button sendButton;

    [SerializeField] private string apiKey = "AIzaSyAZZqsYer6b0_t1OUZtUJF8ktco_rPgC08"; // Replace with your Gemini Makersuite key

    public AudioSource audioSource;

    private string endpoint => $"https://generativelanguage.googleapis.com/v1/models/chat-bison-001:generateContent?key={apiKey}";

    public System.Action OnAIFailure;

    void Start()
    {
        if (sendButton != null)
            sendButton.onClick.AddListener(SendMessageToGemini);

        LoadCachedResponse();
    }

    public void SendMessageToGemini()
    {
        string userInput = chatInputField.text;
        if (string.IsNullOrEmpty(userInput)) return;

        sendButton.interactable = false;
        chatResponseText.text = "Thinking...";

        StartCoroutine(SendToGemini(userInput));
    }

    IEnumerator SendToGemini(string userInput)
    {
        string jsonBody = JsonUtility.ToJson(new GeminiRequest(userInput));

        UnityWebRequest request = new UnityWebRequest(endpoint, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            chatResponseText.text = "Error: " + request.error;
            OnAIFailure?.Invoke();
        }
        else
        {
            string responseText = ParseGeminiResponse(request.downloadHandler.text);
            chatResponseText.text = "AI: " + responseText;

            PlayerPrefs.SetString("LastAIResponse", responseText);  // Cache offline
            PlayerPrefs.Save();

            PlayVoiceOver(responseText);
        }

        sendButton.interactable = true;
    }

    string ParseGeminiResponse(string json)
    {
        // Look for "text": "..." response from Gemini
        int index = json.IndexOf("\"text\":\"");
        if (index == -1) return "[No response found]";
        index += 8; // length of "\"text\":\""
        int end = json.IndexOf("\"", index);
        if (end == -1) end = json.Length - 1;
        string result = json.Substring(index, end - index);
        return result.Replace("\\n", "\n").Replace("\\\"", "\"");
    }

    public void LoadCachedResponse()
    {
        if (PlayerPrefs.HasKey("LastAIResponse"))
        {
            string cached = PlayerPrefs.GetString("LastAIResponse");
            chatResponseText.text = "Cached AI: " + cached;
            Debug.Log("Loaded cached AI response.");
        }
    }

    void PlayVoiceOver(string text)
    {
        if (audioSource == null)
        {
            Debug.LogWarning("AudioSource missing, skipping voice-over.");
            return;
        }

        // Placeholder: Implement TTS here or play prerecorded clips.
        Debug.Log("Voice-over (AI): " + text);

        // Example: audioSource.PlayOneShot(someAudioClip);
    }

    [System.Serializable]
    public class GeminiRequest
    {
        public Content[] contents;

        public GeminiRequest(string userInput)
        {
            contents = new Content[]
            {
                new Content
                {
                    parts = new Part[]
                    {
                        new Part { text = userInput }
                    }
                }
            };
        }

        [System.Serializable]
        public class Content
        {
            public Part[] parts;
        }

        [System.Serializable]
        public class Part
        {
            public string text;
        }
    }
}
