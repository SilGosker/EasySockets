﻿namespace EasySockets;

/// <summary>
///     The interface for a basic EasySocket class.
/// </summary>
public interface IEasySocket : IDisposable, IInternalEasySocket
{
	/// <summary>
	///     The room that the EasySocket is connected to.
	/// </summary>
	public new string RoomId { get; }

	/// <summary>
	///     The unique identifier of the EasySocket.
	/// </summary>
	public new string ClientId { get; }
	/// <summary>
	///     The event that is fired when the EasySocket should start receiving messages.
	/// </summary>
	/// <returns>
	///     A task that should keep the EasySocket middleware alive during the lifetime of the clients connection
	/// </returns>
	public Task ReceiveMessagesAsync();

	/// <summary>
	///     Checks the connection status of the EasySocket.
	/// </summary>
	/// <returns>
	///     <c>true</c> if the EasySocket is connected; otherwise, <c>false</c>.
	/// </returns>
	public bool IsConnected();

	/// <summary>
	///     The event that will be emitted just before the socket receives its messages
	/// </summary>
	/// <returns>A task that represent the asynchronous event</returns>
	public Task OnConnect();

	/// <summary>
	///     The event that will be emitted just before the backing websocket is disposed and removed from the server
	/// </summary>
	/// <returns>A task that represent the asynchronous event</returns>
	public Task OnDisconnect();

	/// <summary>
	///     Sends a message to the client connected to the server
	/// </summary>
	/// <param name="message">The content of the message</param>
	/// <returns>A task that represents the asynchronous operation</returns>
	public Task SendToClientAsync(string message);
	
    /// <summary>
	///    Closes the websocket connection and disposes the EasySocket instance
	/// </summary>
	/// <returns>
	///   A task that represents the asynchronous closing operation
	/// </returns>
    public Task CloseAsync();

}