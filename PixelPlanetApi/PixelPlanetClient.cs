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
    /// <summary>
    /// Provides a convinient access to pixelplanet API
    /// </summary>
    public class PixelPlanetClient : IDisposable
    {
        private const string BaseDomain = "pixelplanet.fun";
        private static string ApiUrl => $"https://{BaseDomain}/api";
        private static string WsUrl => $"wss://{BaseDomain}/ws";

        private readonly HttpClientHandler _handler;
        private readonly HttpClient _client;
        private readonly Dictionary<byte, WebsocketConnection> _connections = new Dictionary<byte, WebsocketConnection>();

        /// <summary>
        /// Gets available canvases.
        /// </summary>
        /// <value>
        /// The canvases.
        /// </value>
        /// <seealso cref="Canvas"/>
        public Dictionary<byte, Canvas> Canvases { get; private set; } = new Dictionary<byte, Canvas>();

        /// <summary>
        /// Occurs when pixel change message received.
        /// </summary>
        public event EventHandler<PixelChangeEventArgs>? PixelChangeEvent;

        /// <summary>
        /// Creates new <see cref="PixelPlanetClient"/> instance.
        /// </summary>
        /// <param name="handlerFactory">
        /// Pass handler factory to modify <see cref="HttpClientHandler"/> object and set cookies, proxies etc.
        /// </param>
        /// <returns><see cref="PixelPlanetClient"/> instance</returns>
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

        /// <summary>
        /// Places the pixel.
        /// </summary>
        /// <param name="canvasId">The canvas identifier.</param>
        /// <param name="pixel">The <see cref="Pixel"/> object.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Sets collection of <see cref="Area"/> to be tracked for pixel changes.
        /// </summary>
        /// <param name="areas">The <see cref="Area"/> collection.</param>
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

        /// <summary>
        /// Sets client to track changes for single area.
        /// </summary>
        /// <param name="area">The area.</param>
        public void TrackArea(Area area)
        {
            var (canvas, connection) = GetCanvasConnection(area.CanvasId);
            var chunks = canvas.GetChunksForArea(area);

            connection.SetTrackedChunks(chunks);
        }

        #region HTTP methods

        /// <summary>
        /// Gets the chunk.
        /// </summary>
        /// <param name="cx">Chunk X coordinate.</param>
        /// <param name="cy">Chunk Y coordinate.</param>
        /// <param name="canvasId">Canvas identifier.</param>
        /// <returns>Chunk data byte array.</returns>
        public async Task<byte[]> GetChunk(byte cx, byte cy, byte canvasId)
        {
            var url = $"https://{BaseDomain}/chunks/{canvasId}/{cx}/{cy}.bmp";

            return await _client.GetByteArrayAsync(url).ConfigureAwait(false);
        }

        /// <summary>
        /// Updates the captcha token.
        /// </summary>
        /// <param name="token">The token.</param>
        public async Task UpdateCaptchaToken(string token)
        {
            var captchaUpdate = new CaptchaUpdate { Token = token };
            using var content = new StringContent(JsonConvert.SerializeObject(captchaUpdate), Encoding.UTF8, "application/json");

            await _client.PostAsync($"{ApiUrl}/captcha", content).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetches user stats and canvases information.
        /// </summary>
        /// <returns>Me response model form pixelplanet</returns>
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