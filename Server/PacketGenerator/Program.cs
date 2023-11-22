using System;
using System.Xml;

namespace PacketGenerator
{
    class Program
    {
        static string genPackets;

        static void Main(string[] args)
        {
            XmlReaderSettings settings = new XmlReaderSettings()
            {
                IgnoreComments = true,
                IgnoreWhitespace = true,
            };

            using (XmlReader r = XmlReader.Create("PDL.xml", settings))
            {
                r.MoveToContent();

                while(r.Read())
                {
                    if (r.Depth == 1 && r.NodeType == XmlNodeType.Element)
                    {
                        ParsePacket(r);
                    }
                }

                File.WriteAllText("GenPacket.cs", genPackets);
            }

        }

        public static void ParsePacket(XmlReader r)
        {
            // 혹시 모를 상황에 대한 안전장치
            if (r.NodeType == XmlNodeType.EndElement) 
                return;

            if (r.Name.ToLower() != "packet")
            {
                Console.WriteLine("Invalid packet node");
                return;
            }

            string packetname = r["name"];
            if (string.IsNullOrEmpty(packetname))
            {
                Console.WriteLine("Packet without name");
                return;
            }

            Tuple<string, string, string> t = ParseMembers(r);
            genPackets += string.Format(PacketFormat.packetFormat, packetname, t.Item1, t.Item2, t.Item3);
        }

        // {1} 멤버 변수들
        // {2} 멤버 변수 Read
        // {3} 멤버 변수 Write
        public static Tuple<string, string, string> ParseMembers(XmlReader r)
        {
            string packetName = r["name"];

            string memberCode = "";
            string readCode = "";
            string writeCode = "";

            int depth = r.Depth + 1;
            while (r.Read())
            {
                if (r.Depth != depth)
                    break;

                string memberName = r["name"];
                if (string.IsNullOrEmpty(memberName))
                {
                    Console.WriteLine("Member without name");
                    return null;
                }

                if (string.IsNullOrEmpty(memberCode) == false)
                    memberCode += Environment.NewLine; // 멤버 별로 엔터 입력
                if (string.IsNullOrEmpty(readCode) == false)
                    readCode += Environment.NewLine; 
                if (string.IsNullOrEmpty(writeCode) == false)
                    writeCode += Environment.NewLine; 


                string memberType = r.Name.ToLower();
                switch (memberType)
                {
                    case "bool":
                    case "byte":
                    case "short":
                    case "ushort":
                    case "int":
                    case "long":
                    case "float":
                    case "double":
                        memberCode += string.Format(PacketFormat.memberFormat, memberType, memberName);
                        readCode += string.Format(PacketFormat.readFormat, memberName, toMemberType(memberType), memberType);
                        writeCode += string.Format(PacketFormat.writeFormat, memberName, memberType);
                        break;
                    case "string":
                        memberCode += string.Format(PacketFormat.memberFormat, memberType, memberName);
                        readCode += string.Format(PacketFormat.readStringFormat, memberName);
                        writeCode += string.Format(PacketFormat.writeStringFormat, memberName);
                        break;
                    case "list":
                        Tuple<string, string, string> t = ParseList(r);
                        memberCode += t.Item1;
                        readCode += t.Item2;
                        writeCode += t.Item3;
                        break;
                    default:
                        break;
                }
            }

            memberCode = memberCode.Replace("\n", "\n\t");
            readCode = readCode.Replace("\n", "\n\t\t");
            writeCode = writeCode.Replace("\n", "\n\t\t");
            return new Tuple<string, string, string>(memberCode, readCode, writeCode);
        }

        public static Tuple<string, string, string> ParseList(XmlReader r)
        {
            string listName = r["name"];
            if (string.IsNullOrEmpty(listName))
            {
                Console.WriteLine("List without name");
                return null;
            }

            Tuple<string, string, string> t = ParseMembers(r);

            string memberCode = string.Format(PacketFormat.memberListFormat,
                FirstCharacterToUpper(listName),
                FirstCharacterToLower(listName),
                t.Item1,
                t.Item2,
                t.Item3);

            string readCode = string.Format(PacketFormat.readListFormat,
                FirstCharacterToUpper(listName),
                FirstCharacterToLower(listName));

            string writeCode = string.Format(PacketFormat.writeListFormat,
                FirstCharacterToUpper(listName),
                FirstCharacterToLower(listName));

            return new Tuple<string, string, string>(memberCode, readCode, writeCode);
        }

        public static string toMemberType(string memberType)
        {
            switch(memberType)
            {
                case "bool":
                    return "ToBoolean";
                //case "byte": byte는 변환이 따로 정의되어 있지 않다.
                case "short":
                    return "ToInt16";
                case "ushort":
                    return "ToUInt16";
                case "int":
                    return "ToInt32";
                case "long":
                    return "ToInt64";
                case "float":
                    return "ToSingle";
                case "double":
                    return "ToDouble";
                default:
                    return "";
            }
        }

        public static string FirstCharacterToUpper(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";

            return input[0].ToString().ToUpper() + input.Substring(1);
        }

        public static string FirstCharacterToLower(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";

            return input[0].ToString().ToLower() + input.Substring(1);
        }
    }
}