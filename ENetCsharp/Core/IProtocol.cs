
namespace ENetCsharp
{
    public interface IProtocol
    {
        void Write(PacketOrganizer packet);
        void Read(PacketOrganizer packet);
    }
}
