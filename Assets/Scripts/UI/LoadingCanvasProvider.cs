using UnityEngine;

namespace Assets.Scripts.UI
{
    public sealed class LoadingCanvasProvider : MonoBehaviour
    {
        [SerializeField] private Canvas loadingCanvas;
        public Canvas Canvas => loadingCanvas;
    }
}

