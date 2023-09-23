using HeroesLike.Grid;
using HeroesLike.UI;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityTools.Animation.Easing;

namespace HeroesLike.Characters
{
    [RequireComponent(typeof(SpriteRenderer))]
    public abstract class Character : MonoBehaviourExt
    {
        public delegate void ScheduledAction();
        private const float MOVEMENT_TIME = 0.25f;
        private const int MOVEMENT_RANGE_CLOSE = 1;

        public enum CharacterAlignment { Ally, Enemy }
        public enum AbilityPhase { Prepare, Cast }

        private Queue<ScheduledAction> _actionQueue = new();
        private protected int _health;
        private protected int _maxHealth;
        private protected int _maxMovement;
        private protected int _movement;
        private protected int _minDamage;
        private protected int _maxDamage;
        private protected int _range;
        private protected int _minRangeDamage;
        private protected int _maxRangeDamage;
        private protected int _initiative;
        private protected int _packCount;
        private protected bool _distanceAttack;
        private protected bool _inCloseCombat;
        private protected bool _hasAbility;
        private protected bool _abilityActive;
        private protected bool _blockMovementRestore;
        private protected CharacterAlignment _alignment;
        private protected AbilityPhase _behaviorPhase = AbilityPhase.Prepare;

        public virtual Character Init(CharacterAlignment alignment, int packCount)
        {
            OnActionEnd += OccupyCell;
            OnTurnEnd += OnEndTurn;
            _packCount = packCount;
            _alignment = alignment;
            _health = _maxHealth;
            _movement = _maxMovement;
            _range = _maxMovement;
            var align = alignment == CharacterAlignment.Ally ? "ally" : "enemy";
            Runtime.Log($"{name} was spawned in {packCount} counts for {align}");
            return this;
        }

        public virtual Character OnTurnBegin() 
        {
            _movement = _maxMovement;
            return this;
        }

        public virtual void OnRoundBegin() { }

        public virtual void OnEndTurn()
        {
            Stunned = false;
            if (_actionQueue.Count > 0)
                lock (new object())
                    _actionQueue.Dequeue()();
        }

        public virtual Character Activate(CGrid grid)
        {
            CheckForCloseCombat(grid);

            if (Stunned || Movement <= 0)
            {
                if (Stunned) BattleLogger.Instance.Info($"{name.Split('(')[0]} пропускает ход!");
                OnTurnEnd?.Invoke();
                return this;
            }

            if (_inCloseCombat)
            {
                Movement = MOVEMENT_RANGE_CLOSE;
                grid.HiglightCells(GridPosition, Movement, Alignment);
            }
            else
            {
                grid.HiglightCells(GridPosition, Movement, Alignment);
                if (IsDistant) grid.HiglightCells(GridPosition, _range, Alignment, false, CGrid.SpreadType.Finder);
            }

            return this;
        }

        public enum CharAction { Attack, Move, Ability, Pass }

        public virtual ScheduledAction TakeAction(CharAction action, CGrid grid, Vector2Int selected)
        {
            CheckForCloseCombat(grid);
            switch (action)
            {
                case CharAction.Attack:
                    if (_distanceAttack) 
                        Attack(grid, grid[selected].Occupant);
                    else if (!_inCloseCombat)
                    {
                        Move(grid, grid.GetNearestCell(selected, GridPosition));
                        return new ScheduledAction(delegate { TakeAction(CharAction.Attack, grid, selected); });
                    }
                    else Attack(grid, grid[selected].Occupant);
                    break;
                case CharAction.Move:
                    Move(grid, selected);
                    break;
                case CharAction.Ability:
                    UseAbility(grid);
                    break;
                case CharAction.Pass:
                    OnTurnEnd?.Invoke();
                    break;
            }
            return null;
        }

        public virtual void QueueAction(ScheduledAction action)
        {
            lock (new object())
                _actionQueue.Enqueue(action);
        }
            

        public virtual void TakeDamage(CGrid grid, int damage)
        {
            BattleLogger.Instance.DamageInfo($"{name.Split('(')[0]} получил {damage} урона");
            Runtime.Log($"{name} get {damage} damage");
            if (damage >= _maxHealth * (_packCount - 1) + _health)
            {
                _packCount = 0;
                Dead();
            }
            else
            {
                var killed = Mathf.FloorToInt(damage / _maxHealth);
                if (_health > damage)
                {
                    _health -= damage;
                }
                else
                {
                    damage = Mathf.Clamp(damage - _health, 0, damage);
                    _health = 0;
                    _packCount -= Mathf.FloorToInt(damage / _maxHealth);
                    _health = damage % _maxHealth;
                }
            }
        }

        public virtual void UseAbility(CGrid grid)
        {
            Runtime.Log($"{name} use ability");
            grid.HiglightDisable();
        }

        public virtual void Attack(CGrid grid, Character target)
        {
            Runtime.Log($"{target.name} was attacked by {name}");
            _movement = 0;
            int damage;
            if (_distanceAttack && !_inCloseCombat)
                damage = Random.Range(_minRangeDamage, _maxRangeDamage+1) * _packCount;
            else
                damage = Random.Range(_minDamage, _maxDamage+1) * _packCount;
            target.TakeDamage(grid, damage);
            OnActionEnd?.Invoke(CharAction.Attack);
        }

        public virtual void Move(CGrid grid, Vector2Int target)
        {
            var cell = GetCurrentCell();
            cell.Status = GridCell.CellStatus.Empty;
            cell.Occupant = null;
            Anim = new();
            grid.HiglightDisable();
            var way = grid.GetWay(GridPosition, target);
            _movement -= way.Count;
            way.ForEach(w =>
            {
                var pos = transform.localPosition;
                Anim.Raise(Ease.Linear, MOVEMENT_TIME, value => transform.localPosition = Vector3.Lerp(transform.localPosition, w, value));
            });
            Anim.Action(() => OnActionEnd?.Invoke(CharAction.Move)).Start();
        }

        private protected void ActionEndInvoker(CharAction action) =>
            OnActionEnd?.Invoke(action);

        public void OccupyCell(CharAction action)
        {
            var cell = GetCurrentCell();
            switch (_alignment)
            {
                case CharacterAlignment.Ally:
                    cell.Status = GridCell.CellStatus.Player;
                    break;
                case CharacterAlignment.Enemy:
                    cell.Status = GridCell.CellStatus.Enemy;
                    break;
            }
            cell.Occupant = this;
        }

        public virtual void Dead()
        {
            GetCurrentCell().Status = GridCell.CellStatus.Empty;
            OnDead?.Invoke(this);
        }

        public GridCell GetCurrentCell()
        {
            if (Physics.Raycast(transform.position, transform.forward * 10, out RaycastHit hit))
                return hit.collider.gameObject.GetComponent<GridCell>();                
            else throw new System.Exception("Персонаж непонятно где! Оо");
        }

        private void CheckForCloseCombat(CGrid grid)
        {
            switch (_alignment)
            {
                case CharacterAlignment.Ally:
                    _inCloseCombat = grid.GetNeighborStatuses(GridPosition).Contains(GridCell.CellStatus.Enemy);
                    break;
                case CharacterAlignment.Enemy:
                    _inCloseCombat = grid.GetNeighborStatuses(GridPosition).Contains(GridCell.CellStatus.Player);
                    break;
            }
        }

        public Vector2Int GridPosition { get => GetCurrentCell().GridPosition; }
        public int Initiative { get => _initiative; }
        public int Pack { get => _packCount; }
        public int Movement { get => _movement; set => _movement = value; }
        public int Alignment { get => (int)_alignment; set => _alignment = (CharacterAlignment)value; }
        public bool IsDistant { get => _distanceAttack; }
        public bool HasAbility { get => _hasAbility; }
        public bool Stunned { get => _blockMovementRestore; set => _blockMovementRestore = value; }
        public AbilityPhase Phase { get => _behaviorPhase; }

        public delegate void TurnEnd();
        public event TurnEnd OnTurnEnd;
        public delegate void ActionEnd(CharAction action);
        public event ActionEnd OnActionEnd;
        public delegate void DeadEvent(Character ch);
        public event DeadEvent OnDead;
    }
}