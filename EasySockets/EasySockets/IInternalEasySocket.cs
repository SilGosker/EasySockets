using System.Net.WebSockets;
using EasySockets.Builder;
using EasySockets.Enums;
using Microsoft.Extensions.Logging;

namespace EasySockets;

public interface IInternalEasySocket
{
    internal ILogger<EasySocket> Logger { set; }
    internal string RoomId { set; }
    internal string ClientId { set; }
    internal WebSocket WebSocket { set; }
    internal EasySocketOptions Options { set; }
    internal Func<IEasySocket, BroadCastFilter, string, Task> Emit { set; }
    internal Action<IEasySocket> DisposeAtSocketHandler { set; }
    internal Task ReceiveMessagesAsync();
}