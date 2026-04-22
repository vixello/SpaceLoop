using Assets.Scripts.Core.Interfaces;
using Assets.Scripts.Core;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using VContainer.Unity;
using UnityEngine;

public class SceneTransitionService : ISceneTransitionService, IStartable
{
    private EventBinding<SceneLoadedEvent> _sceneLoadedBinding;

    private readonly HashSet<string> _loadedSceneKeys = new(); 
    private readonly HashSet<string> _expectedSceneKeys = new(); 

    private UniTaskCompletionSource _transitionTCS = null;

    public void Start()
    {
        _sceneLoadedBinding = new EventBinding<SceneLoadedEvent>(SceneLoaded);
        EventBus<SceneLoadedEvent>.Register(_sceneLoadedBinding);
    }

    private void SceneLoaded(SceneLoadedEvent evt)
    {
        Debug.Log($"SceneLoaded fired: {evt.SceneName}");

        if (_transitionTCS == null)
        {
            Debug.Log("SceneLoaded ignored: no active transition");
            return;
        }

        if (!_expectedSceneKeys.Contains(evt.RuntimeKey))
        {
            Debug.Log($"SceneLoaded ignored: {evt.SceneName} not expected");
            return;
        }

        _loadedSceneKeys.Add(evt.RuntimeKey);
        Debug.Log($"Loaded {_loadedSceneKeys.Count}/{_expectedSceneKeys.Count}");

        if (_loadedSceneKeys.Count == _expectedSceneKeys.Count)
        {
            Debug.Log("All scenes loaded, completing transition");
            _transitionTCS.TrySetResult();
            Cleanup();
        }
    }


    public UniTask RunFastTransition(SceneTransitionSO transition)
    {
        //Cleanup();
        if (_transitionTCS != null) 
        { 
            Debug.LogWarning("SceneTransitionService: Transition already in progress.");
            return _transitionTCS.Task; 
        }
        Cleanup();

        foreach (var stl in transition.ScenesToLoad)
            _expectedSceneKeys.Add(stl.SceneReference.RuntimeKey.ToString());

        _transitionTCS = new UniTaskCompletionSource();

        EventBus<FastSceneTransitionEvent>.Raise(new FastSceneTransitionEvent
        {
            ScenesToLoad = transition.ScenesToLoad,
            ScenesToUnload = transition.ScenesToUnload
        });

        if (_expectedSceneKeys.Count == 0)
        {
            Debug.Log($"expectedSceneKeys.Count == 0");
            _transitionTCS.TrySetResult();
            Cleanup();
            return UniTask.CompletedTask;
        }

        return _transitionTCS.Task;
    }

    private void Cleanup()
    {
        _loadedSceneKeys.Clear();
        _expectedSceneKeys.Clear();
        _transitionTCS = null;
    }
}
