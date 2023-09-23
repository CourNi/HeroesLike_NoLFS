using HeroesLike.Grid;
using HeroesLike.UI;
using UnityEngine;

namespace HeroesLike.Characters
{
    public class Knight : Character
    {
        private const int SECOND_TURN_CHANCE_PERCENT = 45;
        private const float FIRST_REDUCE_CHANCE = 0.99f;
        private const float SECOND_REDUCE_CHANCE = 0.66f;
        private const float THIRD_REDUCE_CHANCE = 0.33f;

        private int _reduceCounter = 0;

        public override Character Init(CharacterAlignment alignment, int packSize)
        {
            _maxHealth = 20;
            _maxMovement = 5;
            _initiative = 4;
            _minDamage = 6;
            _maxDamage = 10;
            _hasAbility = false;
            base.Init(alignment, packSize);
            return this;
        }

        public override void TakeDamage(CGrid grid, int damage)
        {
            damage = _reduceCounter++ switch
            {
                0 => Mathf.CeilToInt(damage * (1 - FIRST_REDUCE_CHANCE)),
                1 => Mathf.CeilToInt(damage * (1 - SECOND_REDUCE_CHANCE)),
                2 => Mathf.CeilToInt(damage * (1 - THIRD_REDUCE_CHANCE)),
                _ => damage
            };
            base.TakeDamage(grid, damage);
        }

        public override Character Activate(CGrid grid)
        {
            if (_movement <= 0 && Random.Range(0, 100) < SECOND_TURN_CHANCE_PERCENT - 1)
            {
                _movement = _maxMovement;
                BattleLogger.Instance.Info($"{name.Split('(')[0]} получает дополнительный ход!");
            }
            base.Activate(grid);
            return this;
        }
    }
}