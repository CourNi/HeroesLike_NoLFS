using HeroesLike;
using UnityEngine;
using Character = HeroesLike.Characters.Character;

public class SpawnerAbstraction : Singleton<SpawnerAbstraction>
{
    private BattleController _battleController;

    public Character Spawn(Vector2Int position, string characterType, Character.CharacterAlignment alignment, int packCount) =>
        _battleController.Spawn(position, characterType, alignment, packCount);

    public void RegisterConroller(BattleController controller) =>
        _battleController = controller;
}
