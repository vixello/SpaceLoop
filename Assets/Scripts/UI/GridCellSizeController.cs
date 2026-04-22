using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    internal class GridCellSizeController : MonoBehaviour
    {
        [SerializeField] private GridLayoutGroup _gridLayout;
        [SerializeField] private int _columns = 4;

        private RectTransform _rectTransform;

        private void Awake()
        {
            _rectTransform = transform as RectTransform;
        }

        private void OnEnable()
        {
            RecalculateCellSize();
        }

        private void OnRectTransformDimensionsChange()
        {
            RecalculateCellSize();
        }

        [SerializeField] private float _maxCellSize = 280f;

        private void RecalculateCellSize()
        {
            float totalWidth = _rectTransform.rect.width;

            float padding = _gridLayout.padding.left + _gridLayout.padding.right;
            float spacing = _gridLayout.spacing.x * (_columns - 1);

            float availableWidth = totalWidth - padding - spacing;

            float cellSize = availableWidth / _columns;

            cellSize = Mathf.Min(cellSize, _maxCellSize);

            _gridLayout.cellSize = new Vector2(cellSize, cellSize);
        }
    }
}
