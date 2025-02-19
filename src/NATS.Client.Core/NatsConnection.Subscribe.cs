using System.Collections.Concurrent;
using NATS.Client.Core.Internal;

namespace NATS.Client.Core;

public partial class NatsConnection
{
    /// <inheritdoc />
    public async ValueTask<INatsSub> SubscribeAsync(string subject, NatsSubOpts? opts = default, CancellationToken cancellationToken = default)
    {
        var sub = new NatsSub(this, SubscriptionManager, subject, opts);
        await SubAsync(subject, opts, sub, cancellationToken).ConfigureAwait(false);
        return sub;
    }

    /// <inheritdoc />
    public async ValueTask<INatsSub<T>> SubscribeAsync<T>(string subject, NatsSubOpts? opts = default, CancellationToken cancellationToken = default)
    {
        var serializer = opts?.Serializer ?? Opts.Serializer;
        var sub = new NatsSub<T>(this, SubscriptionManager.GetManagerFor(subject), subject, opts, serializer);
        await SubAsync(subject, opts, sub, cancellationToken).ConfigureAwait(false);
        return sub;
    }
}
