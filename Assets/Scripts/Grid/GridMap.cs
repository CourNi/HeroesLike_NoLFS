using System.Collections.Generic;
using UnityEngine;

namespace HeroesLike.Grid
{
    public class GridMap : MonoBehaviourExt
    {
        private const float SHIFT = 0.875f;

        [SerializeField]
        private GridCell _gridPrefab;
        [SerializeField]
        private Vector2Int _gridSize;
        [SerializeField]
        private Vector3 _initPosition;

        private CGrid _grid;
        private List<GridCell> _cells = new();

        private void Awake()
        {
            _grid = new(_gridSize);
            
            for (int y = 0; y < _gridSize.y; y++)
            {
                for (int x = 0; x < _gridSize.x; x++)
                {
                    var cell = Instantiate(_gridPrefab, transform);
                    _cells.Add(cell);
                    _grid.SetCell(cell, x, y);
                }
            }
            var shiftY = SHIFT / 2 * Mathf.Tan(Mathf.PI / 3);
            _grid.Init(_initPosition - new Vector3(_gridSize.y / 2 * SHIFT - SHIFT / 4 + (_gridSize.y % 2 == 0 ? 0 : SHIFT / 2), _gridSize.x / 2 * shiftY - (_gridSize.x % 2 == 0 ? shiftY / 2 : 0), 0));
        }

        public void DestroyThis() => Destroy(this);

        public CGrid Grid { get => _grid; }
        public List<GridCell> CellList { get => _cells; }
    }
}