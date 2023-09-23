using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HeroesLike.Grid
{
    public class CGrid
    {
        public enum SpreadType { Finder, Radial, Flower }
        private const float SHIFT = 0.875f;

        private GridCell[,] _grid;
        private Vector2Int _gridSize;
        private GridCell _currentTarget;

        public GridCell this[Vector2Int address] { get => _grid[address.x, address.y]; }
        public GridCell this[int x, int y] { get => _grid[x, y]; }

        public CGrid(Vector2Int gridSize)
        {
            _gridSize = gridSize;
            _grid = new GridCell[gridSize.x, gridSize.y];
        }

        public void SetCell(GridCell cell, int row, int column)
        {
            _grid[row, column] = cell;
            cell.GridPosition = new Vector2Int(row, column);
            OnHighlightClear += cell.Activate;
            cell.OnTargeted += Targeted;
        }

        public void Targeted(GridCell cell)
        {
            if (cell != null)
            {
                _currentTarget = cell;
                OnTargetChange?.Invoke(this, cell);
            }
        }
            

        public void Init(Vector3 startPosition)
        {
            var shiftY = SHIFT / 2 * Mathf.Tan(Mathf.PI / 3);
            for (int row = 0; row < _gridSize.y; row++)
                for (int column = 0; column < _gridSize.x; column++)
                    _grid[column, row].SetPosition(startPosition + new Vector3(SHIFT * row + (column % 2 == 0 ? SHIFT / 2 : 0), shiftY * column, 0));
        }

        public void HiglightCells(Vector2Int position, int radius, int charAlign, bool highlightClear = true, SpreadType type = SpreadType.Radial, bool ignoreObstacles = false)
        {
            if (highlightClear) HiglightDisable();
            switch (type)
            {
                case SpreadType.Finder:
                    LinearSpread(position, radius, ignoreObstacles, charAlign);
                    break;
                case SpreadType.Radial:
                    RadialSpread(position, radius, ignoreObstacles, charAlign);
                    break;
                case SpreadType.Flower:
                    FlowerSpread(position, radius, ignoreObstacles, charAlign);
                    break;
            }
        }

        public void HiglightDisable() => OnHighlightClear?.Invoke();

        private List<Vector2Int> GetRadialSelection(Vector2Int position, int radius)
        {
            List<Vector2Int> list = new() { position };
            for (int i = 0; i < radius; i++)
            {
                List<Vector2Int> temp = new();
                list.ForEach(p =>
                {
                    for (int j = 0; j < 6; j++)
                        try { temp.Add(GetNeighborAtDirection(p, j, p.x % 2 == 0)); }
                        catch { }
                });
                list.AddRange(temp.Except(list));
            }
            return list;
        }

        public List<Vector2Int> GetNeighborRadial(Vector2Int position)
        {
            List<Vector2Int> list = new();
            for (int j = 0; j < 6; j++)
                try { list.Add(GetNeighborAtDirection(position, j, position.x % 2 == 0)); }
                catch { }
            return list;
        }

        private void RadialSpread(Vector2Int position, int radius, bool ignoreObstacles, int charAlign)
        {
            var list = GetRadialSelection(position, radius);
            list.ForEach(p =>
            {
                if (CheckBounds(p, _gridSize))
                    if (ignoreObstacles || !HasObstacle(position, p, charAlign))
                        this[p].Activate(true);
            });
        }

        private void LinearSpread(Vector2Int position, int radius, bool ignoreObstacles, int charAlign)
        {
            var list = GetRadialSelection(position, radius);
            list.ForEach(p =>
            {
                var status = charAlign switch
                {
                     0 => GridCell.CellStatus.Enemy,
                     1 => GridCell.CellStatus.Player,
                     _ => GridCell.CellStatus.Empty
                };

                if (CheckBounds(p, _gridSize))
                    if (this[p].Status == status)
                        this[p].Activate(true);
            });
        }

        private void FlowerSpread(Vector2Int position, int radius, bool ignoreObstacles, int charAlign)
        {
            throw new System.Exception("Not Implemented");
        }

        private bool HasObstacle(Vector2Int fromPosition, Vector2Int toPosition, int align)
        {
            switch (align)
            {
                case 0:
                    if (this[toPosition].Status == GridCell.CellStatus.Player) return true;
                    break;
                case 1:
                    if (this[toPosition].Status == GridCell.CellStatus.Enemy) return true;
                    break;
            }

            Vector2Int currentTo = toPosition;
            while (currentTo != fromPosition)
            {
                currentTo = GetNearestCellNoStatus(currentTo, fromPosition);
                if (currentTo == fromPosition) return false;
                if (this[currentTo].Status != GridCell.CellStatus.Empty) return true;
            }
            return false;
        }

        public Vector2Int GetNeighborAtDirection(Vector2Int cell, int direction, bool even)
        {
            return direction switch
            {
                0 => cell + (even ? new Vector2Int(1, 0) : new Vector2Int(1, -1)),
                1 => cell + (even ? new Vector2Int(1, 1) : new Vector2Int(1, 0)),
                2 => cell + new Vector2Int(0, 1),
                3 => cell + (even ? new Vector2Int(-1, 1) : new Vector2Int(-1, 0)),
                4 => cell + (even ? new Vector2Int(-1, 0) : new Vector2Int(-1, -1)),
                5 => cell + new Vector2Int(0, -1),
                _ => throw new System.Exception("Direction out of bound")
            };
        }

        public Vector2Int GetNearestCell(Vector2Int currentPosition, Vector2Int targetPosition)
        {
            var oldPos = currentPosition;
            for (int i = 0; i < 6; i++)
            {
                var newPos = GetNeighborAtDirection(currentPosition, i, currentPosition.x % 2 == 0);
                if ((targetPosition - newPos).sqrMagnitude < (targetPosition - oldPos).sqrMagnitude && this[newPos].Status == GridCell.CellStatus.Empty)
                    oldPos = newPos;
            }
            return oldPos;
        }

        public Vector2Int GetNearestCellNoStatus(Vector2Int currentPosition, Vector2Int targetPosition)
        {
            var oldPos = currentPosition;
            for (int i = 0; i < 6; i++)
            {
                var newPos = GetNeighborAtDirection(currentPosition, i, currentPosition.x % 2 == 0);
                if ((targetPosition - newPos).sqrMagnitude < (targetPosition - oldPos).sqrMagnitude)
                    oldPos = newPos;
            }
            return oldPos;
        }

        public List<Vector3> GetWay(Vector2Int from, Vector2Int to)
        {
            List<Vector3> ways = new() { this[to].MovementPosition };
            Vector2Int currentTo = to;
            while (currentTo != from)
            {
                currentTo = GetNearestCell(currentTo, from);
                ways.Add(this[currentTo].MovementPosition);
            }
            ways.Reverse();
            return ways;
        }

        public List<Vector2Int> GetWayVectors(Vector2Int from, Vector2Int to)
        {
            List<Vector2Int> ways = new() { to };
            Vector2Int currentTo = to;
            while (currentTo != from)
            {
                currentTo = GetNearestCellNoStatus(currentTo, from);
                ways.Add(currentTo);
            }
            return ways;
        }

        public GridCell.CellStatus[] GetNeighborStatuses(Vector2Int target)
        {
            GridCell.CellStatus[] statuses = new GridCell.CellStatus[6];
            for (int i = 0; i < 6; i++)
            {
                try { statuses[i] = this[GetNeighborAtDirection(target, i, target.x % 2 == 0)].Status; }
                catch { }                
            }
            return statuses;
        }

        private bool CheckBounds(Vector2Int vector, Vector2Int grid)
        {
            return ((vector.x >= 0 && vector.x < grid.x) && (vector.y >= 0 && vector.y < grid.y)) ? true : false;
        }

        private void UnsubscribeAll()
        {
            System.Delegate[] clientList = OnHighlightClear.GetInvocationList();
            foreach (var client in clientList)
                OnHighlightClear -= (client as HighlightClear);
        }

        public GridCell Target { get => _currentTarget; }

        public delegate void HighlightClear(bool state = false);
        public event HighlightClear OnHighlightClear;
        public delegate void Retarget(CGrid grid, GridCell cell);
        public event Retarget OnTargetChange;
    }
}