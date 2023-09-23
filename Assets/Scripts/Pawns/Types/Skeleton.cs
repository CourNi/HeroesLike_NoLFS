using HeroesLike.Grid;
using HeroesLike.UI;
using System.Linq;
using UnityEngine;

namespace HeroesLike.Characters
{
    public class Skeleton : Character
    {
        private const int BASE_DAMAGE = 6;
        private const int BASE_INITIATIVE = 6;
        private const int BASE_HEALTH = 6;
        private const int BASE_SPEED = 3;

        private const float FIRST_GENERATION_MODIFIER = 0.7f;
        private const float SECOND_GENERATION_MODIFIER = 0.4f;
        private const float THIRD_GENERATION_MODIFIER = 0.1f;

        private int _generation;
        private bool _borned;

        public override Character Init(CharacterAlignment alignment, int packSize)
        {
            _maxHealth = 6;
            _maxMovement = 3;
            _initiative = 6;
            _minDamage = 6;
            _maxDamage = 6;
            _hasAbility = false;
            base.Init(alignment, packSize);
            return this;
        }

        public Character NewStatsAssign(int generation, bool borned = true)
        {
            _borned = borned;
            var modifier = generation switch
            {
                0 => 1,
                1 => FIRST_GENERATION_MODIFIER,
                2 => SECOND_GENERATION_MODIFIER,
                3 => THIRD_GENERATION_MODIFIER,
                _ => 1
            };

            _borned = true;
            _maxHealth = Mathf.CeilToInt(BASE_HEALTH * modifier);
            _maxMovement = Mathf.CeilToInt(BASE_SPEED * modifier);
            _minDamage = Mathf.CeilToInt(BASE_DAMAGE * modifier);
            _maxDamage = Mathf.CeilToInt(BASE_DAMAGE * modifier);
            _initiative = Mathf.CeilToInt(BASE_INITIATIVE * modifier);
            _movement = _maxMovement;
            _health = _maxHealth;
            return this;
        }

        public override void TakeDamage(CGrid grid, int damage)
        {
            int initialCount = _packCount;
            base.TakeDamage(grid, damage);
            int respawnCount = initialCount - _packCount;
            if (!_borned)
            {
                var targets = grid.GetNeighborRadial(GridPosition);
                if (targets.Where(t => grid[t].Status == GridCell.CellStatus.Empty) == null) return;

                var character = SpawnerAbstraction.Instance.Spawn(targets.Where(t => grid[t].Status == GridCell.CellStatus.Empty).First(), "Skeleton", _alignment, respawnCount);
                (character as Skeleton).NewStatsAssign(++_generation);
                BattleLogger.Instance.Info($"{respawnCount} of {name.Split('(')[0]} восстали!");
                Runtime.Log($"{respawnCount} of {name} respawned");
            }
        }
    }
}

