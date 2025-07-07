using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class MultiManualManager : MonoBehaviour
{
    public TMP_Dropdown equipmentDropdown;
    public TextMeshProUGUI titleText, instructionText, toolText, warningText;
    public GameObject[] allParts;

    public MachineManual[] manualsArray;

    // Toggle-related fields
    public GameObject manualPanel;   // The group of UI for manual steps
    public GameObject chatPanel;     // The AI Chat Panel object
    public Toggle aiToggle;          // The toggle UI element

    public GeminiAssistant geminiAssistant;  // Reference to GeminiAssistant

    public AudioSource audioSource;  // For voice-over audio playback

    private Dictionary<string, List<MaintenanceStep>> manuals;
    private List<MaintenanceStep> currentSteps;
    private int index = 0;

    void Start()
    {
        Debug.Log("Starting MultiManualManager...");

        // Defensive checks
        if (equipmentDropdown == null) Debug.LogError("equipmentDropdown is NOT assigned!");
        if (aiToggle == null) Debug.LogError("aiToggle is NOT assigned!");
        if (manualPanel == null) Debug.LogError("manualPanel is NOT assigned!");
        if (chatPanel == null) Debug.LogError("chatPanel is NOT assigned!");
        if (geminiAssistant == null) Debug.LogWarning("geminiAssistant reference not assigned!");
        if (audioSource == null) Debug.LogWarning("audioSource not assigned, voice-over disabled.");

        LoadManuals();

        if (equipmentDropdown != null)
        {
            equipmentDropdown.onValueChanged.AddListener(delegate { OnMachineChanged(); });
            equipmentDropdown.value = 0;
            OnMachineChanged();
        }

        if (aiToggle != null)
        {
            aiToggle.onValueChanged.AddListener(OnToggleChanged);
            OnToggleChanged(aiToggle.isOn); // Ensure correct visibility
        }

        if (geminiAssistant != null)
        {
            geminiAssistant.OnAIFailure += HandleAIFailure;
        }
    }

    void LoadManuals()
    {
        manuals = new Dictionary<string, List<MaintenanceStep>>();

        foreach (MachineManual m in manualsArray)
        {
            if (m != null)
                manuals[m.machineName] = new List<MaintenanceStep>(m.steps);
        }

        Debug.Log("Loaded manuals from ScriptableObject.");
    }

    public void OnMachineChanged()
    {
        if (equipmentDropdown == null || !equipmentDropdown.options.Exists(o => o.text == equipmentDropdown.options[equipmentDropdown.value].text))
        {
            Debug.LogError("Dropdown value invalid.");
            return;
        }

        string key = equipmentDropdown.options[equipmentDropdown.value].text;

        if (!manuals.ContainsKey(key))
        {
            Debug.LogError("No manual for machine: " + key);
            return;
        }

        currentSteps = manuals[key];
        index = 0;
        ShowStep(index);
    }

    public void Next()
    {
        if (currentSteps != null && index < currentSteps.Count - 1)
        {
            index++;
            ShowStep(index);
        }
    }

    public void Previous()
    {
        if (currentSteps != null && index > 0)
        {
            index--;
            ShowStep(index);
        }
    }

    public void Repeat()
    {
        if (currentSteps != null)
        {
            ShowStep(index);
        }
    }

    void ShowStep(int i)
    {
        if (currentSteps == null || i >= currentSteps.Count) return;

        var step = currentSteps[i];

        if (titleText != null) titleText.text = $"Step {step.step}: {step.title}";
        if (instructionText != null) instructionText.text = step.instruction;
        if (toolText != null) toolText.text = $"Tool: {step.tool}";
        if (warningText != null) warningText.text = $"Warning: {step.warning}";

        Highlight(step.highlight);

        PlayVoiceOver(step.instruction);
    }

    void Highlight(string name)
    {
        foreach (var part in allParts)
        {
            if (part == null) continue;

            var r = part.GetComponent<Renderer>();
            if (r != null)
            {
                if (part.name.ToLower() == name.ToLower())
                    r.material.color = Color.yellow;
                else
                    r.material.color = Color.white;
            }
        }
    }

    void OnToggleChanged(bool useAI)
    {
        if (manualPanel != null) manualPanel.SetActive(!useAI);
        if (chatPanel != null) chatPanel.SetActive(useAI);
    }

    void HandleAIFailure()
    {
        Debug.LogWarning("AI failed, falling back to manual panel.");
        if (aiToggle != null)
        {
            aiToggle.isOn = false;   // Switch toggle off
            OnToggleChanged(false);  // Show manual panel
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
        Debug.Log("Voice-over (manual step): " + text);

        // Example: audioSource.PlayOneShot(someAudioClip);
    }
}
