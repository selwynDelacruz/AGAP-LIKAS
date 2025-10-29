using UnityEngine;

public class WaterManager : MonoBehaviour
{
    [Header("Wave Settings")]
    [Tooltip("If true, reads wave properties from material. If false, uses local values and writes to material.")]
    public bool readFromMaterial = false;
    
    public float wavesHeight = 0.5f;
    public float wavesFrequency = 0.1f;
    public float waveSpeed = 0.05f;
    public Transform WaterPlaneGO;

    Material FloodMat;
    
    Texture2D wavesDisplacement;
    
    void Start()
    {
        SetVariables();
        
        // Read initial values from material if enabled
        if (readFromMaterial)
        {
            ReadFromMaterial();
        }
        else
        {
            updateMaterial();
        }
    }

    void SetVariables()
    {
        if (WaterPlaneGO == null)
        {
            Debug.LogError("WaterManager: WaterPlaneGO is not assigned!", this);
            return;
        }
        
        FloodMat = WaterPlaneGO.GetComponent<Renderer>().sharedMaterial;
        
        if (FloodMat == null)
        {
            Debug.LogError("WaterManager: Could not find material on WaterPlaneGO!", this);
            return;
        }
        
        wavesDisplacement = (Texture2D)FloodMat.GetTexture("_WavesDisplacement");
        
        if (wavesDisplacement == null)
        {
            Debug.LogWarning("WaterManager: _WavesDisplacement texture not found in material!", this);
        }
    }

    /// <summary>
    /// Reads wave properties from the Flood material shader
    /// </summary>
    void ReadFromMaterial()
    {
        if (FloodMat == null) return;

        // Read shader properties if they exist
        if (FloodMat.HasProperty("_WavesHeight"))
            wavesHeight = FloodMat.GetFloat("_WavesHeight");
        
        if (FloodMat.HasProperty("_WavesFrequency"))
            wavesFrequency = FloodMat.GetFloat("_WavesFrequency");
        
        if (FloodMat.HasProperty("_WavesSpeed"))
            waveSpeed = FloodMat.GetFloat("_WavesSpeed");
    }

    public float WaterHeightAtPosition(Vector3 position)
    {
        if (wavesDisplacement == null || WaterPlaneGO == null)
            return WaterPlaneGO != null ? WaterPlaneGO.position.y : 0f;

        return WaterPlaneGO.position.y + wavesDisplacement.GetPixelBilinear(
            position.x * wavesFrequency, 
            position.z * wavesFrequency + Time.time * waveSpeed).g * wavesHeight * WaterPlaneGO.localScale.x;
    }

    void OnValidate()
    {
        if (!FloodMat)
            SetVariables();

        if (!readFromMaterial)
        {
            updateMaterial();
        }
    }

    void updateMaterial()
    {
        if (FloodMat == null) return;
        
        FloodMat.SetFloat("_WavesHeight", wavesHeight);
        FloodMat.SetFloat("_WavesFrequency", wavesFrequency);
        FloodMat.SetFloat("_WavesSpeed", waveSpeed);
    }

    void Update()
    {
        // Continuously sync with material if reading mode is enabled
        if (readFromMaterial)
        {
            ReadFromMaterial();
        }
    }
}
