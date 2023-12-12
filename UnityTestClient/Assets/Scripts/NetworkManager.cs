using DummyClient;
using ServerCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    ServerSession _session = new ServerSession();

    public void Send(ArraySegment<byte> sendBuff)
    {
        _session.Send(sendBuff);
    }

    // Start is called before the first frame update
    void Start()
    {
        string host = Dns.GetHostName();
        IPHostEntry ipHost = Dns.GetHostEntry(host);
        IPAddress ipAddr = ipHost.AddressList[0];
        IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

        Connector connector = new Connector();
        connector.Connect(endPoint, () => { return _session; }, 1); // count 만큼 더미 생성
    }

    void Update()
    {
        List<IPacket> list = PacketQueue.Instance.PopAll();
        foreach (IPacket packet in list)
        {
            // 결국 PacketHandler로 패킷 처리를 하지만, 백그라운드 쓰레드가 아닌 유니티 메인 쓰레드에서 실행하도록 연결
            PacketManager.Instance.HandlePacket(_session, packet);
        }
    }
}
