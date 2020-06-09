# PixelPlanetApi
A simple library to interact with pixelplanet.fun API

# Installation
Install as regular NuGet package:
``` PowerShell
Install-Package PixelPlanetApi
```

# Usage

1. Create a PixelPlanetClient instance

    ``` C#
    PixelPlanetClient client = await PixelPlanetClient.Create();
    ```

    You can pass HttpClient handler configuration as argument. This allows you to use proxy, cookies etc.

    ``` C#
    PixelPlanetClient _client = await PixelPlanetClient.Create(h =>
    {
        h.CookieContainer = new CookieContainer();
        h.CookieContainer.Add(new Cookie
        {
            Name = "pixelplanet.session",
            Value = "session",
            Domain = "pixelplanet.fun",
            Path = "/",
        });
    });
    ```

2. Use client methods to interact with pixelplanet.fun

# Quick examples

* Place pixel

    ``` C#
    Pixel pixel = new Pixel
    {
        X = 10000,
        Y = 5000,
        Color = 3
    };
    PixelReturn response = await _client.PlacePixel(0, pixel).ConfigureAwait(false);
    if (response.ReturnCode == ReturnCode.Success)
    {
      // ...
    }
    ```

* Register area for tracking and subscribe to PixelChangeEvent to receive updates

    ``` C#
    Area area = new Area
    {
        X1 = 8000,
        Y1 = 4000,
        X2 = 15000,
        Y2 = 6000,
        CanvasId = 0
    };
    client.TrackArea(area);
    client.PixelChangeEvent += (sender, args) => 
    {
      // ...
    };
    ```

* Get chunk data 

    ``` C#
    byte[] data = await client.GetChunkData(11, 22, 0);
    ```

* Get user statistics 

    ``` C#
    MeResponse me = await _client.FetchMe();
    ```

* Pay attention to `Canvas` type. It has a lot of useful methods to work with canvas data.

    ``` C#
    Canvas canvas = _client.Canvases[0]; // Earth canvas
    (byte cx, byte cy) = canvas.GetChunkOfPixel(1000, 1500); // Chunk coordinates for the pixel
    int offset = GetOffsetOfPixel(2222, 800); // Offset is the count of pixels from the start of chunk.
    short x = GetAbsoluteCoordinate(10, 200);
    HashSet<(byte, byte)> areaChunks = canvas.GetChunksForArea(area);
    ```

* See other PixelPlanetClient public methods. I will try to make more convenient documentation soon
