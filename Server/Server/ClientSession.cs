using ServerCore;
using System.Net;
using System.Text;

namespace Server
{
    public abstract class Packet
    {
        public ushort size;
        public ushort packetId;

        public abstract ArraySegment<byte> Write();
        public abstract void Read(ArraySegment<byte> s);
    }

    class PlayerInfoReq : Packet
    {
        public long playerId;
        public string name;

        public struct SkillInfo
        {
            public int id;
            public short level;
            public float duration;

            public bool Write(Span<byte> s, ref ushort count)
            {
                bool success = true;
                success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), id);
                count += sizeof(int);
                success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), level);
                count += sizeof(short);
                success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), duration);
                count += sizeof(float);

                return success;
            }

            public void Read(ReadOnlySpan<byte> s, ref ushort count)
            {
                id = BitConverter.ToInt32(s.Slice(count, s.Length - count));
                count += sizeof(int);
                level = BitConverter.ToInt16(s.Slice(count, s.Length - count));
                count += sizeof(short);
                duration = BitConverter.ToSingle(s.Slice(count, s.Length - count));
                count += sizeof(float);
            }
        }

        public List<SkillInfo> skills = new List<SkillInfo>();

        public PlayerInfoReq()
        {
            this.packetId = (ushort)PacketID.PlayerInfoReq;
        }

        public override void Read(ArraySegment<byte> segment)
        {
            ushort count = 0;

            ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);

            count += sizeof(ushort); // packet size offset
            count += sizeof(ushort); // packetId offset

            // read playerId
            // 범위를 초과하면 에러가 발생하도록 함
            this.playerId = BitConverter.ToInt64(s.Slice(count, s.Length - count));
            count += sizeof(long);

            // read string byte length
            ushort nameLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
            count += sizeof(ushort);

            // read name string
            this.name = Encoding.Unicode.GetString(s.Slice(count, nameLen));
            count += nameLen;

            // read skill list
            ushort skillLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
            count += sizeof(ushort);
            skills.Clear(); // 혹시나 skills에 더미 값이 있을 때를 위한 초기화
            for (int i = 0; i < skillLen; i++)
            {
                SkillInfo skill = new SkillInfo();
                skill.Read(s, ref count);
                skills.Add(skill);
            }
        }

        public override ArraySegment<byte> Write()
        {
            ArraySegment<byte> segment = SendBufferHelper.Open(4096);
            bool success = true;
            ushort count = 0;

            Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

            count += sizeof(ushort); // packet size offset
            // insert packetId
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.packetId);
            count += sizeof(ushort);
            // insert playerID
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.playerId);
            count += sizeof(long);

            // insert string
            // name의 UTF-16 바이트 배열 길이가 들어갈 공간 2바이트를 남겨두고 그 뒤에 string을 byte 배열로 저장
            ushort nameLen = (ushort)Encoding.Unicode.GetBytes(this.name, 0, this.name.Length, segment.Array, segment.Offset + count + sizeof(ushort));
            // name의 UTF-16 바이트 배열의 길이 저장
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), nameLen);
            count += sizeof(ushort);
            count += nameLen;

            // insert skill list
            // 리스트 갯수 입력
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)skills.Count);
            count += sizeof(ushort);

            // 리스트 내용물 입력
            foreach (SkillInfo skill in skills)
            {
                success &= skill.Write(s, ref count);
            }

            // 패킷의 전체 바이트 길이 입력
            success &= BitConverter.TryWriteBytes(s, count);


            if (success == false)
                return null;

            return SendBufferHelper.Close(count);
        }
    }

    public enum PacketID
    {
        PlayerInfoReq = 1,
        PlayerInfoOk = 2,
    }

    class ClientSession : PacketSession // Session Inteface를 통해 다양한 세션 타입 정의 가능
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected : {endPoint}");

            //Packet packet = new Packet() { size = 4, packetId = 9 };

            //ArraySegment<byte> openSegment = SendBufferHelper.Open(4096);
            //byte[] buffer = BitConverter.GetBytes(packet.size);
            //byte[] buffer2 = BitConverter.GetBytes(packet.packetId);
            //Array.Copy(buffer, 0, openSegment.Array, openSegment.Offset, buffer.Length);
            //Array.Copy(buffer2, 0, openSegment.Array, openSegment.Offset + buffer.Length, buffer2.Length);
            //ArraySegment<byte> sendBuff = SendBufferHelper.Close(buffer.Length + buffer2.Length);

            //Send(sendBuff);
            Thread.Sleep(5000);
            Disconnect();
        }
        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            ushort count = 0;
            ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
            count += 2;
            ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
            count += 2;

            switch ((PacketID)id)
            {
                case PacketID.PlayerInfoReq:
                    {
                        PlayerInfoReq p = new PlayerInfoReq();
                        p.Read(buffer);
                        Console.WriteLine($"PlayerInfoReq: {p.playerId} {p.name}");
                        
                        foreach(PlayerInfoReq.SkillInfo skill in p.skills)
                        {
                            Console.WriteLine($"Skill({skill.id})({skill.level})({skill.duration})");
                        }
                    
                    }
                    break;
            }
            Console.WriteLine($"RecvPacketId: {id}, Size: {size}");
        }

        public override void OnDisconnect(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisconnected : {endPoint}");
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transferred bytes: {numOfBytes}");

        }
    }
}
