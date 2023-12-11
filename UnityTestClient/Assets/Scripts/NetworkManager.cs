using DummyClient;
using ServerCore;
using System;
using System.Collections;
using System.Net;
using System.Threading;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    ServerSession _session = new ServerSession();

    // Start is called before the first frame update
    void Start()
    {
        string host = Dns.GetHostName();
        IPHostEntry ipHost = Dns.GetHostEntry(host);
        IPAddress ipAddr = ipHost.AddressList[0];
        IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

        Connector connector = new Connector();
        connector.Connect(endPoint, () => { return _session; }, 1); // count 만큼 더미 생성

        StartCoroutine("CoSendPacket");
    }

    void Update()
    {
        IPacket packet = PacketQueue.Instance.Pop();
        if (packet != null)
        {
            // 결국 PacketHandler로 패킷 처리를 하지만, 백그라운드 쓰레드가 아닌 유니티 메인 쓰레드에서 실행하도록 연결
            PacketManager.Instance.HandlePacket(_session, packet);
        }
    }

    IEnumerator CoSendPacket()
    {
        while (true)
        {
            yield return new WaitForSeconds(3.0f);

            C_Chat chatPacket = new();
            chatPacket.chat = "Hello Unity!";
            ArraySegment<byte> segment = chatPacket.Write();

            _session.Send(segment);
        }
    }
}
