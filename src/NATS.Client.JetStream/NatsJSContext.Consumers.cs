using System.Runtime.CompilerServices;
using NATS.Client.JetStream.Models;

namespace NATS.Client.JetStream;

public partial class NatsJSContext
{
    public ValueTask<NatsJSConsumer> CreateConsumerAsync(
        string stream,
        string consumer,
        ConsumerConfigurationAckPolicy ackPolicy = ConsumerConfigurationAckPolicy.@explicit,
        CancellationToken cancellationToken = default) =>
        CreateConsumerAsync(
            new ConsumerCreateRequest
            {
                StreamName = stream,
                Config = new ConsumerConfiguration
                {
                    Name = consumer,
                    DurableName = consumer,
                    AckPolicy = ackPolicy,
                },
            },
            cancellationToken);

    public async ValueTask<NatsJSConsumer> CreateConsumerAsync(
        ConsumerCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrEmpty(request.Config.DeliverSubject))
        {
            throw new NatsJSException("This API only support pull consumers. " +
                                      "'deliver_subject' option applies to push consumers");
        }

        if (request.Config.AckPolicy == ConsumerConfigurationAckPolicy.none)
        {
            throw new NatsJSException("This API only support pull consumers. " +
                                      "'ack_policy' must be set to 'explicit' or 'all' for pull consumers");
        }

        var response = await JSRequestResponseAsync<ConsumerCreateRequest, ConsumerInfo>(
            subject: $"{Opts.Prefix}.CONSUMER.CREATE.{request.StreamName}.{request.Config.Name}",
            request,
            cancellationToken);
        return new NatsJSConsumer(this, response);
    }

    public async ValueTask<NatsJSConsumer> GetConsumerAsync(string stream, string consumer, CancellationToken cancellationToken = default)
    {
        var response = await JSRequestResponseAsync<object, ConsumerInfo>(
            subject: $"{Opts.Prefix}.CONSUMER.INFO.{stream}.{consumer}",
            request: null,
            cancellationToken);
        return new NatsJSConsumer(this, response);
    }

    public async IAsyncEnumerable<NatsJSConsumer> ListConsumersAsync(
        string stream,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var response = await JSRequestResponseAsync<ConsumerListRequest, ConsumerListResponse>(
            subject: $"{Opts.Prefix}.CONSUMER.LIST.{stream}",
            new ConsumerListRequest { Offset = 0 },
            cancellationToken);
        foreach (var consumer in response.Consumers)
            yield return new NatsJSConsumer(this, consumer);
    }

    public async ValueTask<bool> DeleteConsumerAsync(string stream, string consumer, CancellationToken cancellationToken = default)
    {
        var response = await JSRequestResponseAsync<object, ConsumerDeleteResponse>(
            subject: $"{Opts.Prefix}.CONSUMER.DELETE.{stream}.{consumer}",
            request: null,
            cancellationToken);
        return response.Success;
    }
}
