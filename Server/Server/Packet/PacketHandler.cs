using Server;
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

        // BroadCast를 즉시 하지 않고, JobQueue로 처리
        GameRoom room = clientSession.Room; // 작업 예약 후 Room이 null로 바뀌어 Exception이 발생할 수도 있으므로 객체 주소를 복사해놓는다
        room.Push(
            () => room.BroadCast(clientSession, chatPacket.chat)
        );
    }
}
