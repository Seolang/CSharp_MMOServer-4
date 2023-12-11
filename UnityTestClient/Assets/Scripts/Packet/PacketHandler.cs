using DummyClient;
using ServerCore;
using UnityEngine;

/*
    * 패킷을 모두 조립했을 때 실행할 콜백 메소드를 모아놓은 클래스
    */
class PacketHandler
{
    public static void S_ChatHandler(PacketSession session, IPacket packet)
    {
        S_Chat chatPacket = packet as S_Chat;
        ServerSession serverSession = session as ServerSession;

        //if (chatPacket.playerId == 1)
        {
            Debug.Log(chatPacket.chat);

            // 유니티는 게임을 구동하는 메인쓰레드 이외의 쓰레드에서 게임과 관련된 로직을 수행하는 것을 금지함 (제대로 동작하지 않음)
            // 따라서 Handler를 메인쓰레드에서 동작하도록 설정해 주어야 함
            GameObject go = GameObject.Find("Player");
            if (go == null)
                Debug.Log("Player not found");
            else
                Debug.Log("Player found");

        }
    }
}
