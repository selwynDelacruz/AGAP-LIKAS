using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Lobby
{
    /// <summary>
    /// Broadcasts lobby information on LAN for discovery
    /// </summary>
    public class LobbyBroadcaster : MonoBehaviour
    {
        private const int BROADCAST_PORT = 7778;
        private const float BROADCAST_INTERVAL = 1f; // Broadcast every 1 second

        private UdpClient udpClient;
        private Thread broadcastThread;
        private bool isBroadcasting = false;
        private string lobbyCode;
        private string hostIP;
        private int hostPort;
        private int packetsSent;

        [Header("Debug Settings")]
        [SerializeField] private bool showDebugLogs = true;

        /// <summary>
        /// Start broadcasting lobby information
        /// </summary>
        /// <param name="code">Lobby code</param>
        /// <param name="ip">Host IP address</param>
        /// <param name="port">Host port</param>
        public void StartBroadcasting(string code, string ip, int port)
        {
            if (isBroadcasting)
            {
                if (showDebugLogs)
                    Debug.LogWarning("[LobbyBroadcaster] Already broadcasting");
                return;
            }

            lobbyCode = code;
            hostIP = ip;
            hostPort = port;
            packetsSent = 0;

            try
            {
                udpClient = new UdpClient();
                udpClient.EnableBroadcast = true;
                isBroadcasting = true;

                broadcastThread = new Thread(BroadcastLoop);
                broadcastThread.IsBackground = true;
                broadcastThread.Start();

                if (showDebugLogs)
                    Debug.Log($"[LobbyBroadcaster] Started broadcasting: {lobbyCode}|{hostIP}|{hostPort}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[LobbyBroadcaster] Failed to start broadcasting: {e.Message}");
            }
        }

        /// <summary>
        /// Stop broadcasting lobby information
        /// </summary>
        public void StopBroadcasting()
        {
            isBroadcasting = false;

            if (broadcastThread != null && broadcastThread.IsAlive)
            {
                broadcastThread.Join(1000); // Wait up to 1 second
            }

            if (udpClient != null)
            {
                udpClient.Close();
                udpClient = null;
            }

            if (showDebugLogs)
                Debug.Log($"[LobbyBroadcaster] Stopped broadcasting after sending {packetsSent} packets");
        }

        private void BroadcastLoop()
        {
            IPEndPoint broadcastEndpoint = new IPEndPoint(IPAddress.Broadcast, BROADCAST_PORT);

            while (isBroadcasting)
            {
                try
                {
                    // Format: "LOBBYCODE|IP|PORT"
                    string message = $"{lobbyCode}|{hostIP}|{hostPort}";
                    byte[] data = Encoding.UTF8.GetBytes(message);

                    udpClient.Send(data, data.Length, broadcastEndpoint);
                    packetsSent++;

                    if (showDebugLogs && packetsSent % 5 == 0)
                    {
                        Debug.Log($"[LobbyBroadcaster] Broadcast packets sent: {packetsSent} (latest message: {message})");
                    }

                    Thread.Sleep((int)(BROADCAST_INTERVAL * 1000));
                }
                catch (System.Exception e)
                {
                    if (isBroadcasting)
                    {
                        Debug.LogError($"[LobbyBroadcaster] Broadcast error: {e.Message}");
                    }
                }
            }
        }

        private void OnDestroy()
        {
            StopBroadcasting();
        }

        private void OnApplicationQuit()
        {
            StopBroadcasting();
        }
    }
}
