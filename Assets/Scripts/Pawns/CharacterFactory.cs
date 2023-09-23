using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeroesLike.Characters
{
    public class CharacterFactory
    {
        private Dictionary<string, Character> _characterDict;

        public CharacterFactory(List<Character> prefabs) 
        {
            _characterDict = new();
            prefabs.ForEach(i => _characterDict.Add(i.name, i));
        }

        public bool Create(string characterType, Character.CharacterAlignment alignment, int packCount, out Character character)
        {
            if (_characterDict.TryGetValue(characterType, out var charObject))
            {
                character = GameObject.Instantiate(charObject).Init(alignment, packCount);
                return true;
            }
            else { throw new System.Exception("Incorrect Character Type"); }
        }
    }
}
