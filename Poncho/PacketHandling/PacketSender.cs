using Poncho.Enums;
using Poncho.Utility;
using System.Text;

namespace Poncho.PacketHandling
{
    public class PacketSender
    {
        private Stream _stream;
        private List<byte> _data;

        public PacketSender(Stream stream)
        {
            this._stream = stream;
            this._data = new List<byte>();
        }

        public PacketSender LoginResult(int result)
        {
            writePacket(ClientBoundPacket.UserId, BitConverter.GetBytes(result));
            return this;
        }

        public PacketSender Notification(string message)
        {
            writePacket(ClientBoundPacket.Notification, stringBytes(message));
            return this;
        }

        public PacketSender MenuIcon(string imageUrl, string onClickUrl)
        {
            writePacket(ClientBoundPacket.MainMenuIcon, stringBytes($"{imageUrl}|{onClickUrl}"));
            return this;
        }

        public PacketSender Protocol(uint protocol)
        {
            writePacket(ClientBoundPacket.ProtocolVersion, BitConverter.GetBytes(protocol));
            return this;
        }

        public PacketSender Permissions(Permissions p)
        {
            writePacket(ClientBoundPacket.Privileges, new byte[] { (byte)p });
            return this;
        }

        public PacketSender Presence(int id, string username, Permissions permissions, int rank)
        {
            using MemoryStream ms = new MemoryStream();

            ms.Write(BitConverter.GetBytes(id));
            ms.Write(stringBytes(username));
            ms.WriteByte(24); // timezone?
            ms.WriteByte(0); // location privatized
            ms.WriteByte((byte)permissions);
            ms.Write(BitConverter.GetBytes(0f));
            ms.Write(BitConverter.GetBytes(0f));
            ms.Write(BitConverter.GetBytes(0));

            writePacket(ClientBoundPacket.UserPresence, ms.ToArray());
            return this;
        }

        public PacketSender JoinChannel(string channel)
        {
            var prefixed = channel.StartsWith('#')? channel : $"#{channel}";
            prefixed = prefixed.ToLowerInvariant();
            writePacket(ClientBoundPacket.ChannelJoinSuccess, stringBytes(prefixed));

            return this;
        }

        public PacketSender OpenChannel(string channel, string topic, int users)
        {
            using MemoryStream ms = new MemoryStream();
            var prefixed = channel.StartsWith('#') ? channel : $"#{channel}";
            prefixed = prefixed.ToLowerInvariant();

            ms.Write(stringBytes(prefixed));
            ms.Write(stringBytes(topic));
            ms.Write(BitConverter.GetBytes(users));

            writePacket(ClientBoundPacket.ChannelAutoJoin, ms.ToArray());

            return this;
        }

        public PacketSender ChannelInfo(string channel, string topic, int users)
        {
            using MemoryStream ms = new MemoryStream();
            var prefixed = channel.StartsWith('#') ? channel : $"#{channel}";
            prefixed = prefixed.ToLowerInvariant();

            ms.Write(stringBytes(prefixed));
            ms.Write(stringBytes(topic));
            ms.Write(BitConverter.GetBytes(users));

            writePacket(ClientBoundPacket.ChannelInfo, ms.ToArray());

            return this;
        }

        public PacketSender ChannelInfoEnd()
        {
            writePacket(ClientBoundPacket.ChannelInfoEnd, new byte[0]);
            return this;
        }

        public PacketSender TestStats(Status status, string statusText, int id)
        {
            using MemoryStream ms = new MemoryStream();
            using BinaryWriter bw = new BinaryWriter(ms);

            bw.Write(id);
            bw.Write((byte)status);
            bw.Write(stringBytes(statusText));
            bw.Write(stringBytes("")); // beatmap hash
            bw.Write((uint)0); // mods
            bw.Write((byte)0); // playmode
            bw.Write((uint)0); // beatmapid
            bw.Write((ulong)69696969); // rankedscore
            bw.Write((float)100); // accuracy
            bw.Write((uint)420420420); // playcount
            bw.Write((ulong)1); // totalscore
            bw.Write((uint)1); // position
            bw.Write((ushort)69); // PP

            writePacket(ClientBoundPacket.UserState, ms.ToArray());

            return this;
        }

        public PacketSender SendChat(string username, string message, string channel, int sender)
        {
            using MemoryStream ms = new MemoryStream();
            ms.Write(stringBytes(username));
            ms.Write(stringBytes(message));
            ms.Write(stringBytes(channel));
            ms.Write(BitConverter.GetBytes(sender));

            writePacket(ClientBoundPacket.SendMessage, ms.ToArray());

            return this;
        }

        public PacketSender SendFriends(int[] friends)
        {
            using MemoryStream ms = new MemoryStream();

            for(int i = 0; i < friends.Length; i++)
                ms.Write(BitConverter.GetBytes(friends[i]));

            writePacket(ClientBoundPacket.FriendsList, ms.ToArray());
            return this;
        }

        private void writePacket(ClientBoundPacket id, byte[] data)
        {
            this._data.AddRange(BitConverter.GetBytes((ushort)id));
            this._data.Add(0);
            this._data.AddRange(BitConverter.GetBytes((uint)data.Length));
            this._data.AddRange(data);
        }

        private byte[] stringBytes(string message)
        {
            using MemoryStream ms = new MemoryStream();
            ms.WriteByte((byte)0x0b);
            LEB128.WriteLEB128Unsigned(ms, (ulong)message.Length);
            var utf8bytes = Encoding.UTF8.GetBytes(message);
            ms.Write(utf8bytes, 0, utf8bytes.Length);

            return ms.ToArray();
        }

        public async Task FlushAsync()
        {
            await _stream.WriteAsync(_data.ToArray(), 0, _data.Count);
            await _stream.FlushAsync();
        }
    }
}
