namespace Poncho.Attributes
{
    public class PacketIdAttribute : Attribute
    {
        public ushort Id { get; private set; }

        public PacketIdAttribute(ushort id)
        {
            this.Id = id;
        }
    }
}
