using Server.Session;
using ServerCore;

/*
    * 패킷을 모두 조립했을 때 실행할 콜백 메소드를 모아놓은 클래스
    */
class PacketHandler
{
    // PlayerInfoReq 패킷 핸들러
    public static void C_ChatHandler(PacketSession session, IPacket packet)
    {
        C_Chat chatPacket = packet as C_Chat;
        ClientSession clientSession = session as ClientSession;

        if (clientSession.Room == null)
            return;

        clientSession.Room.BroadCast(clientSession, chatPacket.chat);
    }
}
