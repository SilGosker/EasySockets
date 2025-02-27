using System.Text;
using EasySockets.Authentication;
using EasySockets.Helpers;

namespace EasySockets.Builder;

public sealed class EasySocketOptions : IReadonlyEasySocketOptions
{
    private int _bufferSize = 100;

    private string _closingStatusDescription = "Closing";

    private Encoding _encoding = Encoding.UTF8;
    private int _receiveBufferSize = 100;
    private int _sendBufferSize = 100;
    internal List<Type> Authenticators { get; set; } = new();

    /// <summary>
    ///     The size of chunks when receiving or sending messages. <br />
    ///     Default is 100 bytes (100B).
    /// </summary>
    [Obsolete($"Use {nameof(ReceiveBufferSize)} and {nameof(SendBufferSize)} instead. This property will be removed in future versions.")]
    public int BufferSize
    {
        get => _bufferSize;
        set
        {
            ThrowHelper.ThrowIfCannotFitEncodingChars(value, _encoding);
            _bufferSize = value;
            _receiveBufferSize = value;
            _sendBufferSize = value;
        }
    }

    public int ReceiveBufferSize
    {
        get => _receiveBufferSize;
        set
        {
            ThrowHelper.ThrowIfCannotFitEncodingChars(value, _encoding);

            _receiveBufferSize = value;
        }
    }

    public int SendBufferSize
    {
        get => _sendBufferSize;
        set
        {
            ThrowHelper.ThrowIfCannotFitEncodingChars(value, _encoding);

            _sendBufferSize = value;
        }
    }

    public Encoding Encoding
    {
        get => _encoding;
        set
        {
            ThrowHelper.ThrowIfNull(value);
            _encoding = value;
        }
    }

    public string ClosingStatusDescription
    {
        get => _closingStatusDescription;
        set
        {
            ThrowHelper.ThrowIfNull(value);
            _closingStatusDescription = value;
        }
    }

    public bool LoggingEnabled { get; set; } = true;

    /// <summary>
    ///     Adds a single authenticator to the sockets authentication pipeline.
    /// </summary>
    /// <typeparam name="TAuthenticator">The type of the authenticator</typeparam>
    public void AddAuthenticator<TAuthenticator>()
        where TAuthenticator : class, IEasySocketAuthenticator
    {
        Authenticators.Add(typeof(TAuthenticator));
    }

    /// <summary>
    ///     Adds a single async authenticator to the sockets authentication pipeline.
    /// </summary>
    /// <typeparam name="TAuthenticator">The type of the authenticator</typeparam>
    public void AddAsyncAuthenticator<TAuthenticator>()
        where TAuthenticator : class, IEasySocketAsyncAuthenticator
    {
        Authenticators.Add(typeof(TAuthenticator));
    }
}