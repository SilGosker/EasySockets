using EasySockets.Enums;
using EasySockets.Services.Caching;
using Microsoft.Extensions.Logging;

namespace EasySockets.Events;

public abstract class EventSocket<TEvent> : EasySocket, IEventSocket
    where TEvent : IEasySocketEvent
{
    private IReadOnlyList<EventSocketEventInfo> _events = null!;

    IReadOnlyList<EventSocketEventInfo> IInternalEventSocket.Events
    {
        set => _events = value;
    }

    public Task SendToClientAsync(string @event, string message)
    {
        return SendToClientAsync(@event, message, CancellationToken.None);
    }

    public Task SendToClientAsync(string @event, string message, CancellationToken cancellationToken)
    {
        var bound = BindEvent(@event, message);
        if (bound == null)
        {
            if (CanLog(LogLevel.Warning))
                Logger!.LogWarning("Failed to bind event {Event} with message {Message}", @event, message);
            return Task.CompletedTask;
        }

        return SendToClientAsync(bound, cancellationToken);
    }

    /// <inheritdoc cref="SendToClientAsync(string,string)" />
    public Task SendToClientAsync(TEvent @event, string message)
    {
        return SendToClientAsync(message, @event.Event);
    }

    /// <summary>
    ///     Sends a message with an event to the members matching the all members specified by the
    ///     <paramref name="filter" />.
    /// </summary>
    /// <param name="filter">The broadcast level the message will reach.</param>
    /// <param name="event">The event identifier or name.</param>
    /// <param name="message">The message to be sent.</param>
    /// <returns>The task representing the parallel asynchronous sending.</returns>
    public Task Broadcast(BroadCastFilter filter, string @event, string message)
    {
        var bound = BindEvent(@event, message);
        if (bound == null)
        {
            if (CanLog(LogLevel.Warning))
                Logger!.LogWarning("Failed to bind event {Event} with message {Message}", @event, message);

            return Task.CompletedTask;
        }

        return Broadcast(filter, bound);
    }

    /// <inheritdoc cref="Broadcast(BroadCastFilter,string,string)" />
    public Task Broadcast(BroadCastFilter filter, TEvent @event, string message)
    {
        return Broadcast(filter, @event.Event, message);
    }

    /// <summary>
    ///     Sends a message to all members of the sockets room matching the <see cref="EasySocket.RoomId" />
    /// </summary>
    /// <inheritdoc cref="Broadcast(BroadCastFilter, string, string)" />
    public Task Broadcast(string @event, string message)
    {
        var bound = BindEvent(@event, message);
        if (bound == null)
        {
            if (CanLog(LogLevel.Warning))
                Logger!.LogWarning("Failed to bind event {Event} with message {Message}", @event, message);

            return Task.CompletedTask;
        }

        return Broadcast(BroadCastFilter.RoomMembers, bound);
    }

    public sealed override Task OnMessage(string message)
    {
        var @event = ExtractEvent(message);
        if (@event is null)
        {
            if (CanLog(LogLevel.Warning)) Logger!.LogWarning("Failed to extract event from message {Message}", message);
            return OnFailedEventBinding(message);
        }

        var eventInfo = _events.FirstOrDefault(e => e.Contains(@event.Event));
        if (eventInfo is null)
        {
            if (CanLog(LogLevel.Warning))
                Logger!.LogWarning("Failed to find event {Event} in registered events", @event.Event);
            return Task.CompletedTask;
        }

        return eventInfo.InvokeAsync(this, @event, message);
    }

    /// <summary>
    ///     Extracts the event from the message. <br />
    ///     For example, the <paramref name="message" /> could contain JSON like this:
    ///     <code>
    /// {
    ///   "message": "foo",
    ///   "event": "bar"
    /// }
    ///     </code>
    ///     This function should return <c>bar</c> as the result, as that is the extracted event.
    /// </summary>
    /// <param name="message">The full message received from the client</param>
    /// <returns>The event as a string, or <c>null</c> if the operation failed.</returns>
    public abstract TEvent? ExtractEvent(string message);

    /// <summary>
    ///     Creates a message based on the given parameters. <br />
    ///     In the example used in <see cref="ExtractEvent" />,<br /> the code
    ///     should return the following JSON:
    ///     <code>
    /// {
    ///   "message": "foo",
    ///   "event": "bar"
    /// }
    ///     </code>
    /// </summary>
    /// <param name="event">The event id/name</param>
    /// <param name="message">The message to be sent</param>
    /// <returns>The full message to be sent over the connection, or null if the operation failed</returns>
    public abstract string? BindEvent(string @event, string message);

    /// <summary>
    ///     When the event extraction fails or is not registered, this function will be called.
    /// </summary>
    /// <param name="message">The full message received from the backing websocket</param>
    /// <returns>A task representing the execution.</returns>
    public virtual Task OnFailedEventBinding(string message)
    {
        return Task.CompletedTask;
    }
}