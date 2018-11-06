using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Collections;

namespace Kosu.UnityLibrary
{
    public class UDPSender
    {

        private UdpClient _udp;

        private string _host;

        private int _port;

        private bool _isInit;

        public UDPSender() { }

        public UDPSender(string host, int port)
        {
            _udp = new UdpClient();
            _host = host;
            _port = port;
            _isInit = true;
        }

        public void Init(string host, int port)
        {
            if (_isInit)
            {
                return;
            }

            _udp = new UdpClient();
            _host = host;
            _port = port;
            _isInit = true;
        }

        public void Send(string data)
        {
            if (!_isInit)
            {
                return;
            }

            if (_host.IsNullOrEmpty())
            {
                _host = "127.0.0.1";
            }

            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(_host), _port);
            byte[] dgram = System.Text.Encoding.UTF8.GetBytes(data);
            _udp.Send(dgram, dgram.Length, remoteEP);
        }

    }


    public class UDPReciever
    {

        private UdpClient _udp;

        public System.Action<string> OnDataRacieved;

        private Thread _th;

        private bool _isInit;

        public UDPReciever() { }

        private System.IDisposable _udpRecieveLoop;

        public UDPReciever(int port)
        {
            _udp = new UdpClient(port);
            //_udp.Client.ReceiveTimeout = 1;
            //_udp.Client.ReceiveTimeout = 1000;
            _isInit = true;
        }

        public void Init(int port)
        {
            if (_isInit)
            {
                return;
            }

            _udp = new UdpClient(port);
            //_udp.Client.ReceiveTimeout = 1;
            //_udp.Client.ReceiveTimeout = 1000;
            _isInit = true;
        }

        public void StartRecieveLoop()
        {
            if (!_isInit)
            {
                return;
            }

            _th = new Thread(new ThreadStart(Recieve));
            _th.Start();
            //_udpRecieveLoop = UniRxUtility.StartCoroutine(RecieveCoroutine);
        }

        public void StopRecieveLoop()
        {
            if (_th != null)
            {
                _th.Abort();
            }
            if (_udp != null)
            {
                _udp.Close();
            }

            //_udpRecieveLoop.Dispose();
            _isInit = false;
        }

        private void Recieve()
        {
            while (true)
            {
                try
                {
                    IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                    byte[] data = _udp.Receive(ref remoteEP);
                    string text = System.Text.Encoding.UTF8.GetString(data);
                    OnDataRacieved.SafeInvoke(text);
                }
                catch (SocketException)
                {
                    //Debug.Log(e.Message);
                }
            }
        }

        private IEnumerator RecieveCoroutine()
        {
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
            byte[] data = null;
            string text = null;

            while (true)
            {
                data = _udp.Receive(ref remoteEP);
                text = System.Text.Encoding.UTF8.GetString(data);
                OnDataRacieved.SafeInvoke(text);
                yield return null;
                data = null;
                text = null;
            }
        }

    }
}
