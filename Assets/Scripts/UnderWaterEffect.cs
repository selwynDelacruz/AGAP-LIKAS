using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class UnderWaterEffect : MonoBehaviour
{
    public Transform waterPlane; // Assign the water plane
    public Volume postProcessVolume; // Assign the Global Volume
    private DepthOfField depthOfField;
    private bool isUnderwater = false;

    void Start()
    {
        // Get the Depth of Field effect from the Volume
        if (postProcessVolume.profile.TryGet(out depthOfField))
        {
            depthOfField.active = false; // Start with blur disabled
        }
    }

    void Update()
    {
        if (transform.position.y < waterPlane.position.y && !isUnderwater)
        {
            EnterUnderwater();
        }
        else if (transform.position.y >= waterPlane.position.y && isUnderwater)
        {
            ExitUnderwater();
        }
    }

    void EnterUnderwater()
    {
        isUnderwater = true;
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color32(91, 97, 102, 255);
        RenderSettings.fogDensity = 0.05f;

        if (depthOfField != null)
        {
            depthOfField.active = true; // Enable blur effect
            depthOfField.focusDistance.value = 0.5f;
            depthOfField.aperture.value = 8;
            depthOfField.focalLength.value = 50;
        }
    }

    void ExitUnderwater()
    {
        isUnderwater = false;
        RenderSettings.fog = false;

        if (depthOfField != null)
        {
            depthOfField.active = false; // Disable blur effect
        }
    }
}
