using UnityEngine;

namespace HeroesLike.Core
{
    [RequireComponent(typeof(Canvas))]
    public class CanvasInit : MonoBehaviour
    {
        private void Awake()
        {
            ControlManager.Instance.MainCanvas = gameObject.GetComponent<Canvas>();
        }
    }
}
