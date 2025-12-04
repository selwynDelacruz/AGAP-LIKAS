using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Debug script to monitor NetworkManager state
/// Attach to any GameObject to see network status
/// </summary>
public class NetworkManagerDebugger : MonoBehaviour
{
    [Header("Display Settings")]
    public bool showOnScreenDebug = true;
    public KeyCode toggleKey = KeyCode.F1;

    [Header("Display Position")]
    public Rect debugWindowRect = new Rect(10, 10, 350, 250);

    private bool _displayDebug = true;
    private GUIStyle _boxStyle;
    private GUIStyle _labelStyle;
    private GUIStyle _headerStyle;
    private bool _stylesInitialized = false;

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            _displayDebug = !_displayDebug;
        }
    }

    private void InitializeStyles()
    {
        if (_stylesInitialized) return;

        _boxStyle = new GUIStyle(GUI.skin.box);
        _boxStyle.normal.background = MakeTexture(2, 2, new Color(0, 0, 0, 0.8f));

        _labelStyle = new GUIStyle(GUI.skin.label);
        _labelStyle.normal.textColor = Color.white;
        _labelStyle.fontSize = 12;

        _headerStyle = new GUIStyle(GUI.skin.label);
        _headerStyle.normal.textColor = Color.yellow;
        _headerStyle.fontSize = 14;
        _headerStyle.fontStyle = FontStyle.Bold;

        _stylesInitialized = true;
    }

    private void OnGUI()
    {
        if (!showOnScreenDebug || !_displayDebug)
            return;

        InitializeStyles();

        GUILayout.BeginArea(debugWindowRect, _boxStyle);
        GUILayout.BeginVertical();

        // Header
        GUILayout.Label("NETWORK STATUS", _headerStyle);
        GUILayout.Space(5);

        // NetworkManager Status
        if (NetworkManager.Singleton == null)
        {
            GUILayout.Label("<color=red>NetworkManager: NOT INITIALIZED</color>", _labelStyle);
        }
        else
        {
            GUILayout.Label("<color=green>NetworkManager: INITIALIZED</color>", _labelStyle);
            GUILayout.Space(5);

            // Network State
            GUILayout.Label($"Is Host: {NetworkManager.Singleton.IsHost}", _labelStyle);
            GUILayout.Label($"Is Server: {NetworkManager.Singleton.IsServer}", _labelStyle);
            GUILayout.Label($"Is Client: {NetworkManager.Singleton.IsClient}", _labelStyle);
            GUILayout.Label($"Is Listening: {NetworkManager.Singleton.IsListening}", _labelStyle);
            
            GUILayout.Space(5);

            // Connection Info
            if (NetworkManager.Singleton.IsListening)
            {
                int connectedClients = NetworkManager.Singleton.ConnectedClients.Count;
                GUILayout.Label($"<color=cyan>Connected Clients: {connectedClients}</color>", _labelStyle);
                
                var transport = NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
                if (transport != null)
                {
                    GUILayout.Label($"Port: {transport.ConnectionData.Port}", _labelStyle);
                    GUILayout.Label($"Address: {transport.ConnectionData.Address}", _labelStyle);
                }
            }

            GUILayout.Space(5);

            // Lobby Info
            string lobbyCode = PlayerPrefs.GetString("LobbyCode", "N/A");
            GUILayout.Label($"<color=yellow>Lobby Code: {lobbyCode}</color>", _labelStyle);
        }

        GUILayout.Space(10);
        GUILayout.Label($"<color=gray>Press {toggleKey} to toggle</color>", _labelStyle);

        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    private Texture2D MakeTexture(int width, int height, Color color)
    {
        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }

        Texture2D texture = new Texture2D(width, height);
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }
}
