using HeroesLike.Characters;
using UnityEngine;

namespace HeroesLike.Grid
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class GridCell : MonoBehaviourExt, IInteractable
    {
        public enum CellStatus { Empty, Prop, Player, Enemy }

        [ReadOnly, SerializeField]
        private Vector2Int _gridPosition;
        private bool _isActivated = false;
        private SpriteRenderer _renderer;

        [ReadOnly, SerializeField]
        private CellStatus _status = CellStatus.Empty;
        private Character _occupant = null;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
        }

        public void SetPosition(Vector3 targetPosition) =>
            transform.localPosition = targetPosition;         

        public void Activate(bool state)
        {
            _isActivated = state;
            if (state && _status == CellStatus.Empty)
                _renderer.color = Color.cyan;
            else if (state && (_status == CellStatus.Enemy || _status == CellStatus.Player))
                _renderer.color = Color.red;
            else
                _renderer.color = Color.gray;
        }

        #region Interact
        public void OnClick()
        {
            if (_isActivated)
                OnClicked?.Invoke(this);
        }

        public void OnEnter()
        {
            if (_isActivated)
                _renderer.color = Color.white;
            OnTargeted?.Invoke(this);
        }

        public void OnExit()
        {
            if (_isActivated)
                Activate(_isActivated);
        }

        public void OnOver() { }
        #endregion

        public bool Activated { get => _isActivated; }
        public Vector3 MovementPosition { get => transform.localPosition + new Vector3(0,0,-0.75f); }
        public Vector2Int GridPosition { get => _gridPosition; set => _gridPosition = value; }
        public CellStatus Status { get => _status; set => _status = value; }
        public Character Occupant { get => _occupant; set => _occupant = value; }

        public delegate void Clicked(GridCell cell);
        public event Clicked OnClicked;
        public delegate void Targeted(GridCell cell);
        public event Targeted OnTargeted;
    }
}