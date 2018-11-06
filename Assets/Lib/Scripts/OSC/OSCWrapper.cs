using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using UnityOSC;
using System.Linq;

namespace Kosu.UnityLibrary
{
    /// <summary>
    /// UnityOSC送信クラス
    /// </summary>
    public class OSCSender
    {

        public string ClientId { get; private set; }

        bool _isInit;

        public void Init(string clientId, int port, string ipStr)
        {
            _isInit = true;
            IPAddress ip;
            ClientId = clientId;

            if (ipStr.IsNullOrEmpty())
            {
                return;
            }

            ip = IPAddress.Parse(ipStr);

            if (OSCHandler.Instance.Clients.ContainsKey(clientId))
            {
                return;
            }

            OSCHandler.Instance.CreateClient(clientId, ip, port);
        }

        public void Send<T>(string address, T value)
        {
            if (!_isInit)
            {
                Debug.LogError("not init");
                return;
            }

            OSCHandler.Instance.SendMessageToClient(ClientId, address, value);
        }

        public void Send<T>(string address, List<T> values)
        {
            if (!_isInit)
            {
                return;
            }

            OSCHandler.Instance.SendMessageToClient(ClientId, address, values);
        }

        public void Send(string address, params object[] objects)
        {
            OSCHandler.Instance.SendMessageToClient(ClientId, address, objects.ToList());
        }

        public void SendWithErrorHandling(string address, System.Action onError, params object[] objects)
        {
            OSCHandler.Instance.SendMessageToClient(ClientId, address, objects.ToList(), onError);
        }

        public void Close()
        {
            if (ClientId == null)
            {
                return;
            }

            OSCHandler.Instance.Close(ClientId);
        }

        public void RepeatSend<T>(string address, int repeat, float delay, List<T> values)
        {
            if (!_isInit)
            {
                return;
            }

            if (delay <= 0)
            {
                delay = 1 / 60f;
            }

            if (repeat <= 0)
            {
                repeat = 1;
            }

            UniRxUtility.StartCoroutine(() => DelaySendEnumrator(address, repeat, delay, values), () =>
            {
                Debug.Log("Delay Call Executed : Address = " + address);
            });
        }

        public void RepeatSend(string address, int repeat, float delay, params object[] args)
        {
            if (!_isInit)
            {
                return;
            }

            if (delay <= 0)
            {
                delay = 1 / 60f;
            }

            if (repeat <= 0)
            {
                repeat = 1;
            }

            UniRxUtility.StartCoroutine(() => DelaySendEnumrator(address, repeat, delay, args), () =>
            {
                Debug.Log("Delay Call Executed : Address = " + address);
            });
        }

        private IEnumerator DelaySendEnumrator(string address, int repeat, float delay, params object[] args)
        {
            for (int i = 0; i < repeat; i++)
            {
                OSCHandler.Instance.SendMessageToClient(ClientId, address, args.ToList());
                yield return new WaitForSeconds(delay);
            }
        }

        private IEnumerator DelaySendEnumrator<T>(string address, int repeat, float delay, List<T> values)
        {
            for (int i = 0; i < repeat; i++)
            {
                OSCHandler.Instance.SendMessageToClient(ClientId, address, values);
                yield return new WaitForSeconds(delay);
            }
        }

    }

    /// <summary>
    /// UnityOSC受信側クラス
    /// </summary>
    public class OSCReciever
    {

        public event System.Action<OSCMessage> OnListenOSCMessage;

        public event System.Action<OSCBundle> OnListenOSCBundle;

        bool _isInit;

        OSCPacket _lastPacket;

        private Queue<OSCPacket> _packets;

        private string _serverId;

        public void Init(string serverId, int port)
        {
            if (_isInit)
            {
                return;
            }

            _isInit = true;
            _serverId = serverId;

            if (!OSCHandler.Instance.Servers.ContainsKey(serverId))
            {
                OSCHandler.Instance.CreateServer(serverId, port);
            }

            _packets = new Queue<OSCPacket>();
            OSCHandler.Instance.PacketRecievedEvent += OnPacketRecieved;
        }

        public void Close()
        {
            if (OSCHandler.Instance != null)
            {
                OSCHandler.Instance.PacketRecievedEvent -= OnPacketRecieved;
            }

            if (_packets != null)
            {
                _packets.Clear();
                _packets = null;
            }
        }

        public void UpdateListen()
        {
            if (!_isInit)
            {
                return;
            }

            while (_packets.Count > 0)
            {
                OSCPacket packet = _packets.Dequeue();

                if (packet == null)
                {
                    return;
                }

                if (packet.IsBundle())
                {
                    OSCBundle bundle = packet as OSCBundle;
                    OnListenOSCBundle.SafeInvoke(bundle);
                }
                else
                {
                    OSCMessage message = packet as OSCMessage;
                    OnListenOSCMessage.SafeInvoke(message);
                }
            }
        }

        private void OnPacketRecieved(string serverId, OSCPacket packet)
        {
            if (_serverId != serverId)
            {
                return;
            }

            _packets.Enqueue(packet);
        }

    }
}