using FlowTrack.Shared.Domain;
using RabbitMQ.Client;

namespace FlowTrack.Shared.Infrastructure.RabbitMq;

[Service(Lifetime.Singleton)]
public sealed class RabbitMqConnection : IAsyncDisposable
{
    private readonly ConnectionFactory _connectionFactory;
    private IConnection? _connection;
    private readonly IEnvStore _env;

    public RabbitMqConnection(IEnvStore env)
    {
        _env = env;

        var connectionParams = GetConnectionParams();

        _connectionFactory = new ConnectionFactory
        {
            HostName = connectionParams.Host,
            Port = connectionParams.Port,
            UserName = connectionParams.UserName,
            Password = connectionParams.Password,
            ClientProvidedName = connectionParams.ClientProvidedName,
        };
    }

    public async Task<IConnection> GetConnectionAsync()
    {
        if (_connection is not null && _connection.IsOpen)
        {
            return _connection;
        }

        _connection = await _connectionFactory.CreateConnectionAsync();

        return _connection;
    }

    public async Task<IChannel> CreateChannelAsync()
    {
        var connection = await GetConnectionAsync();
        var channel = await connection.CreateChannelAsync();

        return channel;
    }

    public async ValueTask DisposeAsync()
    {
        _connection?.Dispose();
    }

    private RabbitMqConnectionParams GetConnectionParams()
    {
        return new RabbitMqConnectionParams
        {
            Host = _env.Get("RABBITMQ_HOST") ?? "localhost",
            Port = int.TryParse(_env.Get("RABBITMQ_PORT"), out var port) ? port : 5672,
            UserName = _env.Get("RABBITMQ_USERNAME") ?? "guest",
            Password = _env.Get("RABBITMQ_PASSWORD") ?? "guest",
            ClientProvidedName = _env.Get("RABBITMQ_CLIENT_PROVIDED_NAME") ?? "FlowTrackApp",
        };
    }
}
