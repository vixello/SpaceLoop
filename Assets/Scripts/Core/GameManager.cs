using UnityEngine;
using VContainer;

namespace Assets.Scripts.Core
{
    public enum GameState
    {
        Pause,
        Loading,
        Start
    }

    public class GameManager : MonoBehaviour
    {
        private GameState _state;
        private EventBinding<ChangeGameStateEvent> _gameStateChangedBinding;

        void Start()
        {
            _state = GameState.Loading;

            _gameStateChangedBinding = new EventBinding<ChangeGameStateEvent>(OnChangeGameState);
            EventBus<ChangeGameStateEvent>.Register(_gameStateChangedBinding); 
        }

        private void OnDestroy()
        {
            EventBus<ChangeGameStateEvent>.Register(_gameStateChangedBinding);
        }

        private void OnChangeGameState(ChangeGameStateEvent e)
        {
            if(_state != e.GameState)
            {
                _state = e.GameState;
                EventBus<GameStateChangedEvent>.Raise(new GameStateChangedEvent { GameState = _state });
            }
        }
    }
}
