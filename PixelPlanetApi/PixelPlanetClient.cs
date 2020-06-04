using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PixelPlanetApi.Eventing;
using PixelPlanetApi.Models;

namespace PixelPlanetApi
{
    public class PixelPlanetClient : IDisposable
    {
        private const string BaseDomain = "pixelplanet.fun";
        private static string ApiUrl => $"https://{BaseDomain}/api";
        private static string WsUrl => $"wss://{BaseDomain}/ws";

        private readonly HttpClientHandler _handler;
        private readonly HttpClient _client;
        private readonly Dictionary<byte, WebsocketConnection> _connections = new Dictionary<byte, WebsocketConnection>();

        public Dictionary<byte, Canvas> Canvases { get; private set; } = new Dictionary<byte, Canvas>();

        public event EventHandler<PixelChangeEventArgs>? PixelChangeEvent;

        public static async Task<PixelPlanetClient> Create(Action<HttpClientHandler>? handlerFactory = null)
        {
            var client = new PixelPlanetClient(handlerFactory);
            var me = await client.FetchMe().ConfigureAwait(false);

            client.Canvases = me.Canvases.Select(c => new Canvas(c)).ToDictionary(c => c.Id);

            return client;
        }

        private PixelPlanetClient(Action<HttpClientHandler>? handlerFactory = null)
        {
            _handler = new HttpClientHandler();
            handlerFactory?.Invoke(_handler);

            _client = new HttpClient(_handler);
        }

        public async Task<PixelReturn> PlacePixel(byte canvasId, Pixel pixel)
        {
            var (canvas, connection) = GetCanvasConnection(canvasId);

            var (cx, cy) = canvas.GetChunkOfPixel(pixel.X, pixel.Y, pixel.Z);
            var offset = canvas.GetOffsetOfPixel(pixel.X, pixel.Y, pixel.Z);

            var tcs = new TaskCompletionSource<PixelReturn>();
            EventHandler<PixelRetrunEventArgs>? pixelReturnedEventHandler = null;

            pixelReturnedEventHandler = (sender, args) =>
            {
                connection.PixelReturnedEvent -= pixelReturnedEventHandler;
                tcs.SetResult(new PixelReturn
                {
                    ReturnCode = args.ReturnCode,
                    WaitSeconds = args.WaitSeconds,
                    CoolDownSeconds = args.CoolDownSeconds
                });
            };

            connection.PixelReturnedEvent += pixelReturnedEventHandler;
            connection.PlacePixel(cx, cy, offset, pixel.Color);

            return await tcs.Task.ConfigureAwait(false);
        }

        public void TrackMultipleAreas(IEnumerable<Area> areas)
        {
            var groups = areas.GroupBy(a => a.CanvasId);

            foreach (var areasGroup in groups)
            {
                var (canvas, connection) = GetCanvasConnection(areasGroup.Key);
                var chunks = new HashSet<(byte, byte)>();

                foreach (var area in areasGroup)
                {
                    chunks.UnionWith(canvas.GetChunksForArea(area));
                }

                connection.SetTrackedChunks(chunks);
            }
        }

        public void TrackArea(Area area)
        {
            var (canvas, connection) = GetCanvasConnection(area.CanvasId);
            var chunks = canvas.GetChunksForArea(area);

            connection.RegisterChunks(chunks);
        }

        #region HTTP methods

        public async Task<byte[]> GetChunk(byte c1, byte c2, byte canvas)
        {
            var url = $"https://{BaseDomain}/chunks/{canvas}/{c1}/{c2}.bmp";

            return await _client.GetByteArrayAsync(url).ConfigureAwait(false);
        }

        public async Task UpdateCaptchaToken(string token)
        {
            var captchaUpdate = new CaptchaUpdate { Token = token };
            using var content = new StringContent(JsonConvert.SerializeObject(captchaUpdate), Encoding.UTF8, "application/json");

            await _client.PostAsync($"{ApiUrl}/captcha", content).ConfigureAwait(false);
        }

        public async Task<MeResponse> FetchMe()
        {
            var responseString = await _client.GetStringAsync($"{ApiUrl}/me").ConfigureAwait(false);
            var me = JsonConvert.DeserializeObject<MeResponse>(responseString);

            return me;
        }

        #endregion HTTP methods

        private (Canvas, WebsocketConnection) GetCanvasConnection(byte canvasId)
        {
            if (!Canvases.TryGetValue(canvasId, out var canvas))
                throw new ArgumentException("CanvasId not found");

            if (!_connections.TryGetValue(canvasId, out var connection))
            {
                connection = new WebsocketConnection(
                    new Uri(WsUrl),
                    canvasId,
                    () => new ClientWebSocket
                    {
                        Options = {
                        UseDefaultCredentials =_handler.UseDefaultCredentials,
                        Credentials =_handler.Credentials,
                        Proxy =_handler.Proxy,
                        Cookies = _handler.CookieContainer
                        }
                    });
                connection.PixelChangedEvent += OnPixelChange;

                _connections.Add(canvasId, connection);
            }

            return (canvas, connection);
        }

        private void OnPixelChange(object sender, PixelChangeRelativeEventArgs pixelChangeRelative)
        {
            var canvas = Canvases[pixelChangeRelative.CanvasId];
            var pixel = new Pixel { Color = pixelChangeRelative.Color };

            if (pixelChangeRelative.RZ == null)
            {
                pixel.Y = canvas.GetAbsoluteCoordinate(pixelChangeRelative.CY, pixelChangeRelative.RY);
            }
            else
            {
                pixel.Y = pixelChangeRelative.RY;
                pixel.Z = canvas.GetAbsoluteCoordinate(pixelChangeRelative.CY, pixelChangeRelative.RZ.Value);
            }

            PixelChangeEvent?.Invoke(this, new PixelChangeEventArgs
            {
                CanvasId = pixelChangeRelative.CanvasId,
                Pixel = pixel
            });
        }

        #region IDisposable Support

        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _client.Dispose();
                    _handler.Dispose();

                    foreach (var connection in _connections)
                    {
                        connection.Value.Dispose();
                    }
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion IDisposable Support
    }
}