using System;
using ENetCsharp;

namespace ServerTest
{
    public struct TestProtocol : IProtocol
    {
        public int Number;
        public string Text;
        public float FloatNumber;
        public uint PlayerID;

        public void Read(PacketOrganizer packet)
        {
            Number = packet.ReadInt();
            Text = packet.ReadString();
            FloatNumber = packet.ReadFloat();
            PlayerID = (uint)packet.ReadLong();
        }

        public void Write(PacketOrganizer packet)
        {
            packet.Write(Number);
            packet.Write(Text);
            packet.Write(FloatNumber);
            packet.Write((long)PlayerID);
        }

        public override string ToString()
        {
            return $"{Number}, {Text}, {FloatNumber}, {PlayerID}";
        }
    }
}
