using HeroesLike.Characters;
using HeroesLike.Grid;
using HeroesLike.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityTools.Extensions;
using Character = HeroesLike.Characters.Character;

namespace HeroesLike
{
    public class BattleController : MonoBehaviourExt
    {
        private CGrid _grid;
        [SerializeField] private List<Character> _chars = new();
        private Queue<Character> _activationQueue = new();
        private Queue<Character.ScheduledAction> _actionQueue = new();
        private CharacterFactory _factory;
        private Character _activeChar;
        private int _round = 0;

        #region Temp?
        //íå óñïåâàþ îòäåëèòü View
        [SerializeField] private GameObject _buttonPass;
        [SerializeField] private GameObject _buttonAbility;
        #endregion

        private void Start()
        {
            SpawnerAbstraction.Instance.RegisterConroller(this);
            if (TryGetComponent(out GridMap gridMap))
            {
                _grid = gridMap.Grid;
                gridMap.CellList.ForEach(cell => cell.OnClicked += ClickHandler);
                gridMap.DestroyThis();
            }
            _factory = new(_chars);
            _chars.Clear();
            Spawn(new Vector2Int(2, 2), "Knight", Character.CharacterAlignment.Ally, 20);
            Spawn(new Vector2Int(3, 3), "Knight", Character.CharacterAlignment.Ally, 20);
            Spawn(new Vector2Int(4, 2), "Knight", Character.CharacterAlignment.Ally, 20);
            Spawn(new Vector2Int(3, 2), "Archer", Character.CharacterAlignment.Ally, 40);
            Spawn(new Vector2Int(7, 3), "Mage", Character.CharacterAlignment.Ally, 20);
            Spawn(new Vector2Int(6, 4), "Mage", Character.CharacterAlignment.Ally, 20);
            Spawn(new Vector2Int(5, 12), "Zombie", Character.CharacterAlignment.Enemy, 1);
            Spawn(new Vector2Int(8, 8), "Skeleton", Character.CharacterAlignment.Enemy, 50);
            Spawn(new Vector2Int(7, 8), "Skeleton", Character.CharacterAlignment.Enemy, 50);
            Spawn(new Vector2Int(8, 7), "Skeleton", Character.CharacterAlignment.Enemy, 50);
            Spawn(new Vector2Int(7, 7), "Skeleton", Character.CharacterAlignment.Enemy, 50);
            Spawn(new Vector2Int(3, 15), "Urka", Character.CharacterAlignment.Enemy, 10);
            Spawn(new Vector2Int(8, 15), "Urka", Character.CharacterAlignment.Enemy, 10);
            Timer = new();
            Timer.Delay(1.0f, RoundStart);
        }

        public void ClickHandler(GridCell cell)
        {
            if (_activeChar != null)
            {
                if (_activeChar.Phase == Character.AbilityPhase.Cast) _activeChar.UseAbility(_grid);
                else
                {
                    _buttonPass.SetActive(false);
                    _buttonAbility.SetActive(false);
                    Character.ScheduledAction action = null;
                    if (cell.Status == GridCell.CellStatus.Empty)
                        action = _activeChar.TakeAction(Character.CharAction.Move, _grid, cell.GridPosition);
                    else if ((cell.Status == GridCell.CellStatus.Player && _activeChar.Alignment == 1) ||
                        (cell.Status == GridCell.CellStatus.Enemy && _activeChar.Alignment == 0))
                        action = _activeChar.TakeAction(Character.CharAction.Attack, _grid, cell.GridPosition);

                    if (action != null)
                        lock (new object())
                            _actionQueue.Enqueue(action);
                }
            }
        }

        public void UseAbility() =>
            _activeChar.UseAbility(_grid);

        private void RoundStart()
        {
            _grid.HiglightDisable();
            BattleLogger.Instance.Info($"Íà÷àëî ðàóíäà {++_round}!");
            Runtime.Log($"!!! Round {_round} begins !!!");
            _chars.Shuffle();
            _chars.OrderByDescending(x => x.Initiative).ToList().ForEach(ch => _activationQueue.Enqueue(ch));
            Runtime.Log($"Queue count: {_activationQueue.Count}");
            Timer.Delay(1.0f, ActivateNext);
        }

        private void ActionEnd(Character.CharAction action)
        {
            if (_actionQueue.Count > 0)
                lock (new object())
                    _actionQueue.Dequeue()();
            else
                try 
                { 
                    _activeChar.Activate(_grid);
                    _buttonPass.SetActive(true);
                }
                catch { }
        }

        private void TurnEnd()
        {
            _grid.HiglightDisable();
            Timer.Dispose();
            Timer.Delay(1.0f, ActivateNext);
        }

        private void CharacterDeath(Character character)
        {
            character.OnDead -= CharacterDeath;
            _activationQueue = new(_activationQueue.Where(x => x != character));
            _chars.Remove(character);
            Destroy(character.gameObject);
            CheckVictory();
        }

        private void CheckVictory()
        {
            if (_chars.FirstOrDefault(ch => ch.Alignment != 0) == null)
            {
                _grid.HiglightDisable();
                _activeChar = null;
                BattleLogger.Instance.Persistent("ÑÂÅÒËÀß ÑÒÎÐÎÍÀ ÎÄÅÐÆÀËÀ ÏÎÁÅÄÓ!");
            }
            else if (_chars.FirstOrDefault(ch => ch.Alignment != 1) == null)
            {
                _grid.HiglightDisable();
                _activeChar = null;
                BattleLogger.Instance.Persistent("ÒÅÌÍÀß ÑÒÎÐÎÍÀ ÎÄÅÐÆÀËÀ ÏÎÁÅÄÓ!");
            }
        }

        public void ActivateNext()
        {
            if (_activationQueue.Count > 0)
            {
                try
                {
                    if (_activeChar != null)
                    {
                        _activeChar.OnEndTurn();
                        Runtime.Log($"{_activeChar.name} ends turn");
                    }
                    _activeChar = _activationQueue.Dequeue().OnTurnBegin();
                    Runtime.Log($"Turn of {_activeChar.name}");
                    BattleLogger.Instance.Info($"Õîäèò {_activeChar.name.Split('(')[0]}!");
                    _activeChar.Activate(_grid);
                    _buttonPass.SetActive(true);
                    _buttonAbility.SetActive(_activeChar.HasAbility);
                }
                catch (System.Exception e)
                {
                    Runtime.Log(e.Message);
                    ActivateNext();
                }
            }
            else
            {
                RoundStart();
            }
        }

        [Command("spawn")] //íå îáðàùàéòå âíèìàíèÿ ýòî äëÿ îòëàäêè â êîíñîëè
        public void Spawn(int xPos, int yPos, string characterType, int alignment, int packCount) =>
            Spawn(new Vector2Int(xPos, yPos), characterType, (Character.CharacterAlignment)alignment, packCount);

        public Character Spawn(Vector2Int position, string characterType, Character.CharacterAlignment alignment, int packCount)
        {
            if (_factory.Create(characterType, alignment, packCount, out Character ch))
            {
                if (_grid[position].Status == GridCell.CellStatus.Empty)
                {
                    ch.transform.localPosition = _grid[position].MovementPosition;
                    _grid[position].Status = alignment switch
                    {
                        Character.CharacterAlignment.Ally => GridCell.CellStatus.Player,
                        Character.CharacterAlignment.Enemy => GridCell.CellStatus.Enemy,
                        _ => GridCell.CellStatus.Empty
                    };
                    ch.OnActionEnd += ActionEnd;
                    ch.OnTurnEnd += TurnEnd;
                    ch.OnDead += CharacterDeath;
                    _grid[position].Occupant = ch;
                    _chars.Add(ch);
                    return ch;
                }
                else throw new System.Exception("Character not spawned!");
            }
            else throw new System.Exception("Character not spawned!");
        }
    }
}