using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    /*
     * 패킷을 모두 조립했을 때 실행할 콜백 메소드를 모아놓은 클래스
     */
    class PacketHandler
    {
        // PlayerInfoReq 패킷 핸들러
        public static void PlayerInfoReqHandler(PacketSession session, IPacket packet)
        {
            PlayerInfoReq p = packet as PlayerInfoReq;

            Console.WriteLine($"PlayerInfoReq: {p.playerId} {p.name}");

            foreach (PlayerInfoReq.Skill skill in p.skills)
            {
                Console.WriteLine($"Skill({skill.id})({skill.level})({skill.duration})");
            }
        }
    }
}
