using System.Collections.Concurrent;
using EasySockets.Enums;
using EasySockets.Events;

namespace EasySockets.Services;

internal sealed class EasySocketService : IEasySocketService
{
    private readonly ConcurrentDictionary<string, List<IEasySocket>> _rooms = new();

    public bool Any()
    {
        return !_rooms.IsEmpty;
    }

    public bool Any(string roomId)
    {
        return _rooms.ContainsKey(roomId);
    }

    public bool Any(string roomId, string clientId)
    {
        return _rooms.TryGetValue(roomId, out var easySockets) &&
               easySockets.Any(o => o.ClientId == clientId && o.IsConnected());
    }

    public int Count()
    {
        return _rooms.Sum(e => e.Value.Count);
    }

    public int Count(string roomId)
    {
        return _rooms.TryGetValue(roomId, out var easySockets) ? easySockets.Count : 0;
    }

    public Task ForceLeaveAsync(string roomId)
    {
        return ForceLeaveAsync(roomId, CancellationToken.None);
    }

    public Task ForceLeaveAsync(string roomId, CancellationToken cancellationToken)
    {
        if (_rooms.TryGetValue(roomId, out var easySockets))
        {
            return Task.WhenAll(easySockets.Select(e => e.CloseAsync(cancellationToken)));
        }

        return Task.CompletedTask;
    }

    public Task ForceLeaveAsync(string roomId, string clientId)
    {
        return ForceLeaveAsync(roomId, clientId, CancellationToken.None);
    }

    public Task ForceLeaveAsync(string roomId, string clientId, CancellationToken cancellationToken)
    {
        if (_rooms.TryGetValue(roomId, out var easySockets))
        {
            return Task.WhenAll(easySockets.Where(e => e.ClientId == clientId)
                .Select(e => e.CloseAsync(cancellationToken)));
        }

        return Task.CompletedTask;
    }

    public Task SendToRoomAsync(string roomId, string message)
    {
        return SendToRoomAsync(roomId, message, CancellationToken.None);
    }

    public Task SendToRoomAsync(string roomId, string message, CancellationToken cancellationToken)
    {
        if (_rooms.TryGetValue(roomId, out var easySockets))
        {
            return Task.WhenAll(easySockets.Select(e => e.SendToClientAsync(message, cancellationToken)));
        }

        return Task.CompletedTask;
    }

    public Task SendToRoomAsync(string roomId, string @event, string message)
    {
        return SendToRoomAsync(roomId, @event, message, CancellationToken.None);
    }

    public Task SendToRoomAsync(string roomId, string @event, string message, CancellationToken cancellationToken)
    {
        if (_rooms.TryGetValue(roomId, out var easySockets))
        {
            return Task.WhenAll(easySockets.Select(e => e is IEventSocket o
                ? o.SendToClientAsync(@event, message, cancellationToken)
                : Task.CompletedTask));
        }

        return Task.CompletedTask;
    }

    public Task SendToClientAsync(string roomId, string clientId, string message)
    {
        return SendToClientAsync(roomId, clientId, message, CancellationToken.None);
    }

    public Task SendToClientAsync(string roomId, string clientId, string message, CancellationToken cancellationToken)
    {
        if (_rooms.TryGetValue(roomId, out var easySockets))
        {
            return Task.WhenAll(easySockets.Where(e => e.ClientId == clientId)
                .Select(e => e.SendToClientAsync(message, cancellationToken)));
        }

        return Task.CompletedTask;
    }

    public Task SendToClientAsync(string roomId, string clientId, string @event, string message)
    {
        return SendToClientAsync(roomId, clientId, @event, message, CancellationToken.None);
    }

    public Task SendToClientAsync(string roomId, string clientId, string @event, string message,
        CancellationToken cancellationToken)
    {
        if (_rooms.TryGetValue(roomId, out var easySockets))
        {
            return Task.WhenAll(easySockets.Where(e => e.ClientId == clientId)
                           .Select(e => e is IEventSocket o
                                              ? o.SendToClientAsync(@event, message, cancellationToken)
                                                                 : Task.CompletedTask));
        }

        return Task.CompletedTask;
    }

    public IEnumerable<IGrouping<string, IEasySocket>> GetGroupings()
    {
        return _rooms.SelectMany(e => e.Value).GroupBy(e => e.RoomId);
    }

    internal async Task AddSocket(IEasySocket socket)
    {
        if (!socket.IsConnected()) return;

        socket.Emit = BroadCast;
        socket.DisposeAtSocketHandler = RemoveSocket;

        if (!_rooms.TryAdd(socket.RoomId, new List<IEasySocket>() { socket }))
        {
            _rooms[socket.RoomId].Add(socket);
        }

        await socket.OnConnect().ConfigureAwait(false);
        await socket.ReceiveMessagesAsync().ConfigureAwait(false);
    }

    private Task BroadCast(IEasySocket? sender, BroadCastFilter broadCastFilter, string? message)
    {
        if (string.IsNullOrEmpty(message) || sender == null || broadCastFilter == BroadCastFilter.Everyone)
            return Task.CompletedTask;

        var simpleSockets = _rooms.SelectMany(e => e.Value);

        if (broadCastFilter.HasFlag(BroadCastFilter.EqualRoomId))
            simpleSockets = _rooms.TryGetValue(sender.RoomId, out var easySockets)
                ? easySockets
                : Enumerable.Empty<IEasySocket>();

        if (broadCastFilter.HasFlag(BroadCastFilter.EqualType))
            simpleSockets = simpleSockets.Where(e => e.GetType() == sender.GetType());

        if (broadCastFilter.HasFlag(BroadCastFilter.Members))
            simpleSockets = simpleSockets.Where(e => e.ClientId != sender.ClientId);

        return Task.WhenAll(simpleSockets.Select(e => e.SendToClientAsync(message)));
    }

    internal void RemoveSocket(IEasySocket caster)
    {
        if (_rooms.TryGetValue(caster.RoomId, out var easySockets))
        {
            easySockets.RemoveAll(e => e.ClientId == caster.ClientId);
            if (easySockets.Count < 1) _rooms.TryRemove(caster.RoomId, out _);
        }
    }
}