using HeroesLike.Grid;

namespace HeroesLike.Characters
{
    public class Zombie : Character
    {
        public override Character Init(CharacterAlignment alignment, int packSize)
        {
            _maxHealth = 3000;
            _maxMovement = 2;
            _initiative = 4;
            _minDamage = 0;
            _maxDamage = 0;
            _hasAbility = false;
            base.Init(alignment, packSize);
            return this;
        }

        public override void Attack(CGrid grid, Character target)
        {
            int currentTargetAlignment = target.Alignment;
            target.Alignment = Alignment;
            target.OccupyCell(CharAction.Pass);
            Runtime.Log($"{target.name} charmed!");
            target.QueueAction(delegate
            {
                target.Alignment = currentTargetAlignment;
                target.OccupyCell(CharAction.Pass);
                Runtime.Log($"{target.name} with us again!");
            });
            base.Attack(grid, target);
        }
    }
}
