namespace Poncho.Enums
{
    [Flags]
    public enum Permissions : byte
    {
        User = 1 << 0,
        Moderator = 1 << 1,
        Supporter = 1 << 2,
        Owner = 1 << 3,
        Developer = 1 << 4
    }
}
