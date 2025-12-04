using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Lobby
{
    /// <summary>
    /// Scans LAN for available lobbies
    /// </summary>
    public class LobbyScanner : MonoBehaviour
    {
        private const int LISTEN_PORT = 7778;
        private const int SCAN_TIMEOUT = 5000; // 5 seconds

        private UdpClient udpClient;
        private Thread scanThread;
        private bool isScanning = false;
        private string targetLobbyCode;
        private System.Action<string, int> onLobbyFound;
        private System.Action onScanTimeout;

        [Header("Debug Settings")]
        [SerializeField] private bool showDebugLogs = true;

        private int packetsReceived;

        /// <summary>
        /// Start scanning for a specific lobby code
        /// </summary>
        /// <param name="lobbyCode">Lobby code to search for</param>
        /// <param name="onLobbyFound">Callback when lobby is found (IP, Port)</param>
        /// <param name="onScanTimeout">Callback when scan times out</param>
        public void StartScanning(string lobbyCode, System.Action<string, int> onLobbyFound, System.Action onScanTimeout)
        {
            if (isScanning)
            {
                if (showDebugLogs)
                    Debug.LogWarning("[LobbyScanner] Already scanning");
                return;
            }

            targetLobbyCode = lobbyCode.ToUpper();
            this.onLobbyFound = onLobbyFound;
            this.onScanTimeout = onScanTimeout;
            packetsReceived = 0;

            try
            {
                udpClient = new UdpClient(LISTEN_PORT);
                udpClient.Client.ReceiveTimeout = SCAN_TIMEOUT;
                isScanning = true;

                scanThread = new Thread(ScanLoop) { IsBackground = true };
                scanThread.Start();

                if (showDebugLogs)
                    Debug.Log($"[LobbyScanner] Started scanning for lobby: {targetLobbyCode} on port {LISTEN_PORT}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[LobbyScanner] Failed to start scanning: {e.Message}");
                
                // Ensure main thread dispatcher exists
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    onScanTimeout?.Invoke();
                });
            }
        }

        /// <summary>
        /// Stop scanning for lobbies
        /// </summary>
        public void StopScanning()
        {
            isScanning = false;

            if (scanThread != null && scanThread.IsAlive)
            {
                scanThread.Join(1000);
            }

            if (udpClient != null)
            {
                udpClient.Close();
                udpClient = null;
            }

            if (showDebugLogs)
                Debug.Log("[LobbyScanner] Stopped scanning");
        }

        private void ScanLoop()
        {
            IPEndPoint remoteEndpoint = new IPEndPoint(IPAddress.Any, 0);
            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();

            while (isScanning && stopwatch.ElapsedMilliseconds < SCAN_TIMEOUT)
            {
                try
                {
                    byte[] data = udpClient.Receive(ref remoteEndpoint);
                    packetsReceived++;
                    string message = Encoding.UTF8.GetString(data);

                    if (showDebugLogs && packetsReceived <= 10)
                        Debug.Log($"[LobbyScanner] Packet {packetsReceived} from {remoteEndpoint.Address}: {message}");

                    // Parse message: "LOBBYCODE|IP|PORT"
                    string[] parts = message.Split('|');
                    if (parts.Length == 3)
                    {
                        string receivedCode = parts[0];
                        string ip = parts[1];
                        int port = int.Parse(parts[2]);

                        // Check if this is the lobby we're looking for
                        if (receivedCode == targetLobbyCode)
                        {
                            if (showDebugLogs)
                                Debug.Log($"[LobbyScanner] Found target lobby {receivedCode} at {ip}:{port}");
                            
                            // Notify on main thread
                            UnityMainThreadDispatcher.Instance().Enqueue(() =>
                            {
                                onLobbyFound?.Invoke(ip, port);
                            });

                            StopScanning();
                            return;
                        }
                        else if (showDebugLogs && packetsReceived <= 10)
                        {
                            Debug.Log($"[LobbyScanner] Ignored lobby code {receivedCode} (looking for {targetLobbyCode})");
                        }
                    }
                    else if (showDebugLogs && packetsReceived <= 10)
                    {
                        Debug.Log($"[LobbyScanner] Malformed packet: {message}");
                    }
                }
                catch (SocketException)
                {
                    // Timeout on receive - this is expected
                }
                catch (System.Exception e)
                {
                    if (isScanning)
                    {
                        Debug.LogError($"[LobbyScanner] Scan error: {e.Message}");
                    }
                }
            }

            // Scan timed out
            if (showDebugLogs)
                Debug.LogWarning($"[LobbyScanner] Scan timed out after {SCAN_TIMEOUT}ms. Packets received: {packetsReceived}");
            
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                onScanTimeout?.Invoke();
            });

            StopScanning();
        }

        private void OnDestroy()
        {
            StopScanning();
        }

        private void OnApplicationQuit()
        {
            StopScanning();
        }
    }
}
