using CommunityToolkit.Mvvm.Messaging;

namespace PagesManager.Tests.Helpers;

public class MessengerSpy
{
    public List<object> Sent { get; } = new();

    public IMessenger Messenger { get; } = new WeakReferenceMessenger();

    public void RegisterAll<T>() where T : class
    {
        Messenger.Register<MessengerSpy, T>(this, (r, m) => r.Sent.Add(m));
    }

    public IEnumerable<T> OfType<T>() => Sent.OfType<T>();
}