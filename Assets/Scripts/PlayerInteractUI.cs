using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInteractUI : MonoBehaviour
{
    [SerializeField] private GameObject containerGO;
    [SerializeField] private PlayerInteract playerInteract;
    [SerializeField] private PlayerInteract boatInteract;
    [SerializeField] private TextMeshProUGUI interactTextMeshProUGUI;

    [Header("Hold Progress Bar")]
    [SerializeField] private GameObject progressBarContainer;
    [SerializeField] private Image progressBarFill;

    private PlayerInteract currentActiveInteract;

    private void Update()
    {
        // Determine which interact mode is active
        currentActiveInteract = GetActivePlayerInteract();

        if (currentActiveInteract != null && currentActiveInteract.GetInteractableObject() != null)
        {
            Show(currentActiveInteract.GetInteractableObject());

            // Show progress bar when holding
            if (currentActiveInteract.IsHolding())
            {
                ShowProgressBar();
                UpdateProgressBar(currentActiveInteract.GetHoldProgress());
            }
            else
            {
                HideProgressBar();
            }
        }
        else
        {
            Hide();
            HideProgressBar();
        }
    }

    /// <summary>
    /// Determines which PlayerInteract component is currently active based on enabled state
    /// </summary>
    /// <returns>The active PlayerInteract component, or null if none are active</returns>
    private PlayerInteract GetActivePlayerInteract()
    {
        // Check boat mode first
        if (boatInteract != null && boatInteract.enabled && boatInteract.gameObject.activeInHierarchy)
        {
            return boatInteract;
        }

        // Check player/swim mode
        if (playerInteract != null && playerInteract.enabled && playerInteract.gameObject.activeInHierarchy)
        {
            return playerInteract;
        }

        return null;
    }

    private void Show(IInteractable interactable)
    {
        containerGO.SetActive(true);
        interactTextMeshProUGUI.text = interactable.GetInteractText();
    }

    private void Hide()
    {
        containerGO.SetActive(false);
    }

    private void ShowProgressBar()
    {
        if (progressBarContainer != null)
        {
            progressBarContainer.SetActive(true);
        }
    }

    private void HideProgressBar()
    {
        if (progressBarContainer != null)
        {
            progressBarContainer.SetActive(false);
        }
    }

    private void UpdateProgressBar(float progress)
    {
        if (progressBarFill != null)
        {
            progressBarFill.fillAmount = progress;
        }
    }
}
