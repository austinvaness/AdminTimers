using ProtoBuf;
using Sandbox.ModAPI;

namespace avaness.AdminTimers
{
    [ProtoContract]
    public class ChatMessage
    {
        [ProtoMember(1)]
        public ulong steamUserId;
        [ProtoMember(2)]
        public string msg;

        public ChatMessage()
        {

        }

        public ChatMessage(ulong steamUserId, string msg)
        {
            this.steamUserId = steamUserId;
            this.msg = msg;
        }

        public void Send()
        {
            MyAPIGateway.Multiplayer.SendMessageToServer(Constants.chatPacket, MyAPIGateway.Utilities.SerializeToBinary(this));
        }
    }
}