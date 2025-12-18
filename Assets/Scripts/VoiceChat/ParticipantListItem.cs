using UnityEngine;
using TMPro;

/// <summary>
/// Simple component for participant list item
/// Attached to the prefab used in InstructorVoiceMonitorUI
/// </summary>
public class ParticipantListItem : MonoBehaviour
{
    [Header("References")]
    public TMP_Text nameText;
    public GameObject speakingIndicator; // Optional animated indicator

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
