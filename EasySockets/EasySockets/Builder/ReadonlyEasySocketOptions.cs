using System.Text;

namespace EasySockets.Builder;

public readonly struct ReadonlyEasySocketOptions : IReadonlyEasySocketOptions
{
    public ReadonlyEasySocketOptions()
    {
    }

    public int ReceiveBufferSize { get; init; } = 100;

    public int SendBufferSize { get; init; } = 100;

    public Encoding Encoding { get; init; } = Encoding.UTF8;

    public string ClosingStatusDescription { get; init; } = "Closing";

    public bool LoggingEnabled { get; init; } = true;
}