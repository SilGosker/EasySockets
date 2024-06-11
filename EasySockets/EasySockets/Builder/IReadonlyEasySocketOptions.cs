using System.Text;

namespace EasySockets.Builder;

public interface IReadonlyEasySocketOptions
{
    /// <summary>
    ///     The size of the byte array used to receive messages.<br />
    ///     When expecting to receive large messages, it is recommended to increase this value.<br />
    ///     Default is 100 bytes (100B).
    /// </summary>
    public int ReceiveBufferSize { get; }

    /// <summary>
    ///     The size of the byte array used to send messages.<br />
    ///     When expecting to send large messages back to the client, it is recommended to increase this value.<br />
    ///     Default is 100 bytes (100B).
    /// </summary>
    public int SendBufferSize { get; }

    /// <summary>
    ///     The encoding that will be used to encode en decode messages from and to bytes. <br /><br />
    ///     The default is <see cref="Encoding.UTF8" />
    /// </summary>
    public Encoding Encoding { get; }

    /// <summary>
    ///     A human readable description as to why the socket is closing or is closed. <br /><br />
    ///     The default is <c>"Closing"</c>.
    /// </summary>
    public string ClosingStatusDescription { get; }

    /// <summary>
    ///     Whether or not exceptions and warnings should be logged. <br /><br />
    ///     The default is <c>true</c>.
    /// </summary>
    public bool LoggingEnabled { get; }
}