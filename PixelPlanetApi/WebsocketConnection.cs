using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using PixelPlanetApi.Enums;
using PixelPlanetApi.Eventing;
using Websocket.Client;

namespace PixelPlanetApi
{
    internal class WebsocketConnection : IDisposable
    {
        private readonly HashSet<(byte, byte)> _trackedChunks = new HashSet<(byte, byte)>();
        private readonly ManualResetEventSlim _websocketResetEvent = new ManualResetEventSlim(false);
        private readonly WebsocketClient _client;
        private readonly byte _canvasIndex;

        public event EventHandler<PixelChangeRelativeEventArgs>? PixelChangedEvent;

        public event EventHandler<PixelRetrunEventArgs>? PixelReturnedEvent;

        public WebsocketConnection(Uri url, byte canvasIndex, Func<ClientWebSocket>? clientFactory = null)
        {
            _canvasIndex = canvasIndex;

            _client = new WebsocketClient(url, clientFactory)
            {
                IsTextMessageConversionEnabled = false,
                ReconnectTimeout = TimeSpan.FromSeconds(30),
            };

            _client.ReconnectionHappened.Subscribe((i) =>
            {
                byte[] data = new byte[2] { (byte)Opcode.RegisterCanvas, _canvasIndex };
                _client.Send(data);
                _websocketResetEvent.Set();
            });
            _client.MessageReceived.Subscribe(WebSocket_OnMessage);
            _client.Start();
        }

        public void SetTrackedChunks(HashSet<(byte, byte)> chunks)
        {
            foreach (var tracked in _trackedChunks)
            {
                if (!chunks.Contains(tracked))
                    DeRegisterChunk(tracked);
            }

            chunks.ExceptWith(_trackedChunks);
            _trackedChunks.UnionWith(chunks);

            foreach (var chunk in chunks)
                RegisterChunk(chunk);
        }

        public void PlacePixel(byte cx, byte cy, int offset, byte color)
        {
            _websocketResetEvent.Wait();

            var rx = (byte)(offset >> 0);
            var ry = (byte)(offset >> 8);
            var rz = (byte)(offset >> 16);

            var data = new byte[7]
            {
                (byte) Opcode.PixelUpdate,
                cx,
                cy,
                rz,
                ry,
                rx,
                color
            };
            _client.Send(data);
        }

        private void RegisterChunk((byte, byte) chunk)
        {
            _websocketResetEvent.Wait();

            _trackedChunks.Remove(chunk);
            byte[] data = new byte[3]
            {
                (byte) Opcode.RegisterChunk,
                chunk.Item1,
                chunk.Item2
            };
            _client.Send(data);
        }

        private void DeRegisterChunk((byte, byte) chunk)
        {
            _websocketResetEvent.Wait();

            _trackedChunks.Remove(chunk);
            byte[] data = new byte[3]
            {
                (byte) Opcode.DeRegisterChunk,
                chunk.Item1,
                chunk.Item2
            };
            _client.Send(data);
        }

        private void WebSocket_OnMessage(ResponseMessage response)
        {
            var data = response.Binary;

            if (data.Length == 0) return;

            switch ((Opcode)data[0])
            {
                case Opcode.PixelUpdate:
                    PixelChangedEvent?.Invoke(this, new PixelChangeRelativeEventArgs
                    {
                        CX = data[1],
                        CY = data[2],
                        RZ = data[3],
                        RY = data[4],
                        RX = data[5],
                        Color = data[6],
                        Canvas = _canvasIndex
                    });
                    break;

                case Opcode.PixelReturn:
                    var code = data[1];

                    var waitArray = new byte[4] { data[2], data[3], data[4], data[5] };
                    var coolDownArray = new byte[2] { data[6], data[7] };

                    // https://en.wikipedia.org/wiki/Endianness
                    if (BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(waitArray);
                        Array.Reverse(coolDownArray);
                    }

                    PixelReturnedEvent?.Invoke(this, new PixelRetrunEventArgs()
                    {
                        ReturnCode = (ReturnCode)code,
                        WaitSeconds = BitConverter.ToUInt32(waitArray, 0),
                        CoolDownSeconds = BitConverter.ToInt16(coolDownArray, 0)
                    });
                    break;

                default:
                    return;
            }
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
                    _websocketResetEvent.Dispose();
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