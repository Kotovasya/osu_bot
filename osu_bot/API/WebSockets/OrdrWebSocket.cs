// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace osu_bot.API.WebSockets
{
    public class OrdrWebSocket
    {
        private const string URL = "https://ordr-ws.issou.best";

        private readonly ClientWebSocket ws;

        public OrdrWebSocket()
        {
            ws = new ClientWebSocket();
        }

        public async Task ListenAsync()
        {
            CancellationToken token = CancellationToken.None;
            await ws.ConnectAsync(new Uri(URL), token);

            while (ws.State == WebSocketState.Open)
            {
                ArraySegment<byte> bytesRecived = new();
                WebSocketReceiveResult result = await ws.ReceiveAsync(bytesRecived, token);
                string resultString = Encoding.UTF8.GetString(bytesRecived);
                Console.WriteLine(resultString);
            }
        }
    }
}
