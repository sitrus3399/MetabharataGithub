namespace NyxMachina.Shared.EventFramework.Core.Messenger
{
    /// <summary>
    /// Event Messenger Interface
    /// </summary>
    public interface IEventMessenger : IEventMessengerPublish, IEventMessengerSubscribe, IEventMessengerUnsubscribe { }
}