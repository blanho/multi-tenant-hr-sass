using HrSaas.Modules.Notifications.Application.Interfaces;
using HrSaas.Modules.Notifications.Domain.Enums;

namespace HrSaas.Modules.Notifications.Infrastructure.Channels;

public sealed class ChannelProviderFactory(IEnumerable<IChannelProvider> providers) : IChannelProviderFactory
{
    private readonly Dictionary<NotificationChannel, IChannelProvider> _providers =
        providers.ToDictionary(p => p.Channel);

    public IChannelProvider GetProvider(NotificationChannel channel)
    {
        if (_providers.TryGetValue(channel, out var provider))
            return provider;

        throw new NotSupportedException($"No channel provider registered for {channel}");
    }

    public IReadOnlyList<NotificationChannel> GetAvailableChannels() =>
        _providers.Keys.ToList().AsReadOnly();
}
