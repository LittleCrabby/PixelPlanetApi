namespace PixelPlanetApi.Enums
{
    public enum ReturnCode
    {
        Success,
        InvalidCanvas,
        InvalidCoordinateX,
        InvalidCoordinateY,
        InvalidCoordinateZ,
        InvalidColor,
        RegisteredUsersOnly,
        NotEnoughPlacedForThisCanvas,
        ProtectedPixel,
        IpOverused,
        Captcha,
        ProxyDetected,
    }
}