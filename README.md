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

2. Use client methods to interact with pixelplanet.fun

# Quick examples

* Place pixel

``` C#
var pixel = new Pixel
{
    X = 10000,
    Y = 5000,
    Color = 3
};
var response = await _client.PlacePixel(0, pixel).ConfigureAwait(false);
if (response.ReturnCode == ReturnCode.Success)
{
  // ...
}
```

* Register area for tracking and subscribe to PixelChangeEvent to receive updates

``` C#
var area = new Area
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
byte[] data = await client.GetChunk(11, 22, 0);
```
