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
        private const int SCAN_TIMEOUT = 10000; // Increased to 10 seconds

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
                // Try to create UDP client with port reuse
                udpClient = new UdpClient();
                udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, LISTEN_PORT));
                udpClient.Client.ReceiveTimeout = 2000; // 2 second receive timeout per attempt
                isScanning = true;

                scanThread = new Thread(ScanLoop) { IsBackground = true };
                scanThread.Start();

                if (showDebugLogs)
                    Debug.Log($"[LobbyScanner] Started scanning for lobby: {targetLobbyCode} on port {LISTEN_PORT}");
            }
            catch (SocketException se)
            {
                Debug.LogError($"[LobbyScanner] Socket error (port {LISTEN_PORT} may be in use): {se.Message}");
                Debug.LogWarning("[LobbyScanner] TIP: Try using Direct Connect with the host's IP address instead.");
                
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    onScanTimeout?.Invoke();
                });
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[LobbyScanner] Failed to start scanning: {e.Message}");
                
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
                try
                {
                    udpClient.Close();
                }
                catch { }
                udpClient = null;
            }

            if (showDebugLogs)
                Debug.Log($"[LobbyScanner] Stopped scanning. Total packets received: {packetsReceived}");
        }

        private void ScanLoop()
        {
            IPEndPoint remoteEndpoint = new IPEndPoint(IPAddress.Any, 0);
            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();

            if (showDebugLogs)
                Debug.Log($"[LobbyScanner] Scan loop started. Looking for: {targetLobbyCode}");

            while (isScanning && stopwatch.ElapsedMilliseconds < SCAN_TIMEOUT)
            {
                try
                {
                    byte[] data = udpClient.Receive(ref remoteEndpoint);
                    packetsReceived++;
                    string message = Encoding.UTF8.GetString(data);

                    if (showDebugLogs)
                        Debug.Log($"[LobbyScanner] Received packet #{packetsReceived} from {remoteEndpoint.Address}: {message}");

                    // Parse message: "LOBBYCODE|IP|PORT"
                    string[] parts = message.Split('|');
                    if (parts.Length == 3)
                    {
                        string receivedCode = parts[0].ToUpper();
                        string ip = parts[1];
                        
                        if (!int.TryParse(parts[2], out int port))
                        {
                            Debug.LogWarning($"[LobbyScanner] Invalid port in packet: {parts[2]}");
                            continue;
                        }

                        if (showDebugLogs)
                            Debug.Log($"[LobbyScanner] Parsed lobby - Code: {receivedCode}, IP: {ip}, Port: {port}");

                        // Check if this is the lobby we're looking for
                        if (receivedCode == targetLobbyCode)
                        {
                            Debug.Log($"[LobbyScanner] ? FOUND target lobby {receivedCode} at {ip}:{port}");
                            
                            isScanning = false;
                            
                            // Notify on main thread
                            UnityMainThreadDispatcher.Instance().Enqueue(() =>
                            {
                                onLobbyFound?.Invoke(ip, port);
                            });

                            return;
                        }
                        else
                        {
                            if (showDebugLogs)
                                Debug.Log($"[LobbyScanner] Lobby code mismatch: received '{receivedCode}' but looking for '{targetLobbyCode}'");
                        }
                    }
                    else
                    {
                        if (showDebugLogs)
                            Debug.LogWarning($"[LobbyScanner] Malformed packet (expected 3 parts, got {parts.Length}): {message}");
                    }
                }
                catch (SocketException se)
                {
                    // Timeout on receive - this is expected, continue scanning
                    if (se.SocketErrorCode != SocketError.TimedOut && showDebugLogs)
                    {
                        Debug.LogWarning($"[LobbyScanner] Socket exception: {se.SocketErrorCode}");
                    }
                }
                catch (System.Exception e)
                {
                    if (isScanning && showDebugLogs)
                    {
                        Debug.LogError($"[LobbyScanner] Scan error: {e.Message}");
                    }
                }
            }

            // Scan timed out
            Debug.LogWarning($"[LobbyScanner] ? Scan timed out after {SCAN_TIMEOUT/1000}s. Packets received: {packetsReceived}");
            Debug.LogWarning("[LobbyScanner] Possible causes: 1) Firewall blocking UDP port 7778, 2) Different network/subnet, 3) Host not broadcasting");
            Debug.LogWarning("[LobbyScanner] TIP: Use Direct Connect with host's IP address (e.g., 192.168.1.x)");
            
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                onScanTimeout?.Invoke();
            });
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
