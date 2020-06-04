namespace PixelPlanetApi.Enums
{
    public enum Opcode
    {
        RegisterCanvas = 0xA0,
        RegisterChunk = 0xA1,
        DeRegisterChunk = 0xA2,
        RegisterMultipleChunks = 0xA3,
        RequestChatHistory = 0xA5,
        ChangedMe = 0xA6,
        OnlineCounter = 0xA7,
        PixelUpdate = 0xC1,
        CooldownPacket = 0xC2,
        PixelReturn = 0xC3
    }
}