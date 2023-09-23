using HeroesLike.Grid;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HeroesLike.Characters
{
    public class Urka : Character
    {
        public override Character Init(CharacterAlignment alignment, int packSize)
        {
            _maxHealth = 20;
            _maxMovement = 8;
            _initiative = 2;
            _minDamage = 1;
            _maxDamage = 20;
            _hasAbility = true;
            base.Init(alignment, packSize);
            return this;
        }

        public override void UseAbility(CGrid grid)
        {
            _movement = 0;
            var cells = grid.GetNeighborRadial(GridPosition);
            if (cells.Count > 0 )
                cells.ForEach(cell =>
                {
                    var occupant = grid[cell].Occupant;
                    if (occupant != null && occupant.Alignment != Alignment)
                    {
                        occupant.Stunned = true;
                    }
                        
                });
            ActionEndInvoker(CharAction.Ability);
        }
    }
}
