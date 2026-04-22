using Assets.Scripts.Core.Interfaces;
using System.Collections.Generic;

namespace Assets.Scripts.Core
{
    public interface IEvent{}
    public struct LoginEvent : IEvent
    {
        public LoginType LoginType;
    }

    public struct LogoutEvent : IEvent
    {
        public LoginType LoginType;
    }
    public struct LogoutSucceeded : IEvent
    {
        public LoginType LoginType;
    }

    public struct SwitchAccountEvent : IEvent { }
    public struct LinkAccountEvent : IEvent { }
    
    public struct ChangeButtonVisibilityEvent : IEvent
    {
        public ButtonType ButtonType;
        public LoginType LoginType;
        public bool IsActive;
    }


    public struct ChangeGameStateEvent : IEvent
    {
        public GameState GameState;
    }

    public struct GameStateChangedEvent : IEvent
    {
        public GameState GameState;
    }
    
  
    public struct LoginSucceded : IEvent
    {
        public CloudSyncReason SyncReason;
        public CloudProviderType CloudProviderType;
        public bool IsGuest;
    }
 

    public struct RewardClaimed : IEvent 
    {
        public string AchievementId;
    }

    public struct RewardClaimRequest : IEvent
    {
        public string AchievementId;
    }
    

    public static class EventBus<T> where T: IEvent
    {
        static readonly HashSet<IEventBinding<T>> _bindings = new HashSet<IEventBinding<T>>();

        public static void Register(IEventBinding<T> binding) => _bindings.Add(binding);
        public static void Deregister(IEventBinding<T> binding) => _bindings.Remove(binding);


        public static void Raise(T @event)
        {
            foreach(var binding in _bindings)
            {
                binding.OnEvent?.Invoke(@event);
                binding.OnEventNoArgs?.Invoke();
            }
        }
    }
}
