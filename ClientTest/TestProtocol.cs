using ENetCsharp;
using System;

namespace ClientTest
{
    public struct TestProtocol : IProtocol
    {
        public int Number;
        public string Text;
        public float FloatNumber;

        public void Read(PacketOrganizer packet)
        {
            Number = packet.ReadInt();
            Text = packet.ReadString();
            FloatNumber = packet.ReadFloat();
        }

        public void Write(PacketOrganizer packet)
        {
            packet.Write(Number);
            packet.Write(Text);
            packet.Write(FloatNumber);
        }

        public override string ToString()
        {
            return $"{Number}, {Text}, {FloatNumber}";
        }
    }
}
