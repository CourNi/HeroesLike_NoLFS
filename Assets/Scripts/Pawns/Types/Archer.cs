using HeroesLike.Characters;
using HeroesLike.Grid;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Archer : Character
{
    private const float ABILITY_DAMAGE_MODIFIER = 0.5f;

    private Vector2Int _cell_upper;
    private Vector2Int _cell_lower;

    public override Character Init(CharacterAlignment alignment, int packSize)
    {
        _maxHealth = 10;
        _maxMovement = 6;
        _initiative = 6;
        _minDamage = 1;
        _maxDamage = 6;
        _minRangeDamage = 6;
        _maxRangeDamage = 15;
        _distanceAttack = true;
        _hasAbility = true;
        base.Init(alignment, packSize);
        return this;
    }

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
            damage = Mathf.RoundToInt(Random.Range(_minRangeDamage * ABILITY_DAMAGE_MODIFIER, _maxRangeDamage * ABILITY_DAMAGE_MODIFIER) * _packCount);
            target.TakeDamage(grid, damage);
        }
    }

    public override void UseAbility(CGrid grid)
    {
        if (_behaviorPhase == AbilityPhase.Prepare)
        {
            base.UseAbility(grid);
            grid.OnTargetChange += HighlightTarget;
            _behaviorPhase = AbilityPhase.Cast;
        }
        else if (_behaviorPhase == AbilityPhase.Cast)
        {
            base.UseAbility(grid);
            _movement = 0;
            grid.OnTargetChange -= HighlightTarget;
            List<Vector2Int> cells = new List<Vector2Int>() { _cell_upper, _cell_lower };
            cells.AddRange(grid.GetNeighborRadial(_cell_upper));
            cells.AddRange(grid.GetNeighborRadial(_cell_lower));
            cells.Where(v => grid[v].Occupant != null).ToList().ForEach(cell =>
            {
                if (grid[cell].Occupant.Alignment != Alignment)
                    Attack(grid, grid[cell].Occupant);
            });
            _behaviorPhase = AbilityPhase.Prepare;
            ActionEndInvoker(CharAction.Ability);
        }
    }

    private void HighlightTarget(CGrid grid, GridCell cell)
    {
        _cell_upper = grid.GetNeighborAtDirection(cell.GridPosition, 1, cell.GridPosition.x % 2 == 0);
        _cell_lower = grid.GetNeighborAtDirection(cell.GridPosition, 3, cell.GridPosition.x % 2 == 0);
        grid.HiglightCells(_cell_upper, 1, Alignment);
        grid.HiglightCells(_cell_lower, 1, Alignment, false);
    }
}
