using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInteractUI : MonoBehaviour
{
    [SerializeField] private GameObject containerGO;
    [SerializeField] private PlayerInteract playerInteract;
    [SerializeField] private TextMeshProUGUI interactTextMeshProUGUI;

    [Header("Hold Progress Bar")]
    [SerializeField] private GameObject progressBarContainer;
    [SerializeField] private Image progressBarFill;

    private void Update()
    {
        if (playerInteract.GetInteractableObject() != null)
        {
            Show(playerInteract.GetInteractableObject());

            // Show progress bar when holding
            if (playerInteract.IsHolding())
            {
                ShowProgressBar();
                UpdateProgressBar(playerInteract.GetHoldProgress());
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
