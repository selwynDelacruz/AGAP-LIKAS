using UnityEngine;

public class WaterManager : MonoBehaviour
{
    public float wavesHeight = 0.5f;
    public float wavesFrequency = 0.1f;
    public float waveSpeed = 0.05f;
    public Transform WaterPlaneGO;

    Material FloodMat;
    
    Texture2D wavesDisplacement;
    void Start()
    {
        SetVariables();
    }

    void SetVariables()
    {
        FloodMat = WaterPlaneGO.GetComponent<Renderer>().sharedMaterial;
        wavesDisplacement = (Texture2D)FloodMat.GetTexture("_WavesDisplacement");
    }

    public float WaterHeightAtPosition(Vector3 position)
    {
        return WaterPlaneGO.position.y + wavesDisplacement.GetPixelBilinear(position.x * wavesFrequency, position.z * wavesFrequency + Time.time * waveSpeed).g
        * wavesHeight * WaterPlaneGO.localScale.x;
    }

    void OnValidate()
    {
        if (!FloodMat)
            SetVariables();

        updateMaterial();
    }

    void updateMaterial()
    {
            FloodMat.SetFloat("_WavesHeight", wavesHeight);
            FloodMat.SetFloat("_WavesFrequency", wavesFrequency);
            FloodMat.SetFloat("_WavesSpeed", waveSpeed);
    }
}
