namespace SLOY.Core.Messaging;

public interface IMessageBus
{
    void Subscribe<T>(string topic, Action<T> handler);
    void Unsubscribe<T>(string topic, Action<T> handler);
    void Publish<T>(string topic, T message);
    void PublishAsync<T>(string topic, T message);
}