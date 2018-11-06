
namespace UnityOSC
{
    [System.Serializable]
    public class OSCSendData
    {

        public string clientId;

        public string ipAddress;

        public int port;

    }

    [System.Serializable]
    public class OSCRecieveData
    {
        public string serverId;

        public int port;
    }
}
