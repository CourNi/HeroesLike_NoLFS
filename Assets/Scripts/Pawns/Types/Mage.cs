using HeroesLike.Grid;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HeroesLike.Characters
{
    public class Mage : Character
    {
        private const int SPELL_DAMAGE = 3;

        public override Character Init(CharacterAlignment alignment, int packSize)
        {
            _maxHealth = 4;
            _maxMovement = 6;
            _initiative = 10;
            _minDamage = 1;
            _maxDamage = 2;
            _minRangeDamage = 2;
            _maxRangeDamage = 4;
            _distanceAttack = true;
            _hasAbility = true;
            base.Init(alignment, packSize);
            return this;
        }

        private Vector2Int _targetFrom;
        private Vector2Int _targetTo;
        private int _targetsCount;
        private int _step;

        public override void Attack(CGrid grid, Character target)
        {
            _movement = 0;
            int damage;
            if (_behaviorPhase != AbilityPhase.Cast)
            {
                if (_distanceAttack && !_inCloseCombat)
                    damage = Random.Range(_minRangeDamage, _maxRangeDamage + 1) * _packCount;
                else
                    damage = Random.Range(_minDamage, _maxDamage + 1) * _packCount;
                target.TakeDamage(grid, damage);
                ActionEndInvoker(CharAction.Attack);
            }
            else
            {
                damage = SPELL_DAMAGE * _packCount * _targetsCount;
                target.TakeDamage(grid, damage);
            }
        }

        public override void UseAbility(CGrid grid)
        {
            if (_behaviorPhase == AbilityPhase.Prepare)
            {
                _step = 0;
                base.UseAbility(grid);
                grid.OnTargetChange += HighlightTarget;
                _behaviorPhase = AbilityPhase.Cast;
            }
            else if (_behaviorPhase == AbilityPhase.Cast)
            {
                _movement = 0;
                if (_step == 0)
                    _step++;
                else if (_step == 1)
                {
                    grid.OnTargetChange -= HighlightTarget;

                    var cells = grid.GetWayVectors(_targetFrom, _targetTo);
                    var cellsTargeted = cells.Where(v => grid[v].Occupant != null).ToList();
                    _targetsCount = cellsTargeted.Count;

                    if (_targetsCount != 0)
                        cellsTargeted.ForEach(cell =>
                        {
                            Attack(grid, grid[cell].Occupant);
                        });

                    _behaviorPhase = AbilityPhase.Prepare;
                    ActionEndInvoker(CharAction.Ability);
                }
            }
        }

        private void HighlightTarget(CGrid grid, GridCell cell)
        {
            if (_step == 0)
            {
                _targetFrom = cell.GridPosition;
                grid.HiglightDisable();
                cell.Activate(true);
            }
            if (_step == 1)
            {
                grid.HiglightDisable();
                _targetTo = cell.GridPosition;
                var cells = grid.GetWayVectors(_targetFrom, cell.GridPosition);
                cells.ForEach(v => grid[v].Activate(true));
            }
        }
    }
}