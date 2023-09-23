using UnityEngine;

namespace HeroesLike
{
    public class ControlManager : Singleton<ControlManager>
    {
        private MainControl _control;

        private Camera _gameCamera;
        private Canvas _canvas;

        private GameObject _currentObject;
        private GameObject _previousObject;

        private void Awake()
        {
            _control = new();
            _gameCamera = Camera.main;

            _control.Game.Click.performed += ctx => OnClick();
            _control.Game.Enable();

            _control.Debug.Console.performed += ctx => Runtime.OnConsole();
            _control.Debug.Return.performed += ctx => Runtime.OnReturn();
            _control.Debug.PageUp.performed += ctx => Runtime.OnPageUp();
            _control.Debug.PageDown.performed += ctx => Runtime.OnPageDown();
            _control.Debug.Enable();
        }

        private void Update()
        {
            if (RaycastUpdater(out RaycastHit hit))
            {
                _currentObject = hit.collider.gameObject;
                if (_currentObject != _previousObject)
                {
                    hit.collider.GetComponent<IInteractable>()?.OnEnter();
                    _previousObject?.GetComponent<IInteractable>()?.OnExit();
                }
                else hit.collider.GetComponent<IInteractable>()?.OnOver();
                _previousObject = hit.collider.gameObject;
            }
            else
            {
                _currentObject?.GetComponent<IInteractable>()?.OnExit();
                _currentObject = null;
                _previousObject = null;
            }
        }

        private void OnClick()
        {
            if (RaycastUpdater(out RaycastHit hit)) hit.collider.GetComponent<IInteractable>()?.OnClick();
        }

        private bool RaycastUpdater(out RaycastHit hit)
        {
            Vector3 coor = GetCurrentInputPosition();
            if (Physics.Raycast(_gameCamera.ScreenPointToRay(coor), out hit))
                return true;
            else return false;
        }

        public Vector2 GetCurrentInputPosition() =>
                _control.Game.Position.ReadValue<Vector2>();

        public Vector3 GetCurrentInputWorldPosition() =>
                _gameCamera.ScreenToWorldPoint((Vector3)GetCurrentInputPosition() + Vector3.forward * _canvas.planeDistance);

        public MainControl Control { get => _control; }
        public Canvas MainCanvas { get => _canvas; set => _canvas = value; }
    }
}
