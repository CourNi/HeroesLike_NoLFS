using TMPro;
using UnityEngine;

namespace HeroesLike.UI
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class LogView : MonoBehaviour
    {
        [SerializeField] private bool DamageLog = false;

        private void Start()
        {
            var tmp = GetComponent<TextMeshProUGUI>();
            if (!DamageLog)
            {
                tmp.enabled = false;
                BattleLogger.Instance.TMP = tmp;
            }
            else
            {
                tmp.text = "";
                BattleLogger.Instance.TMPDamage = tmp;
            }
            Destroy(this);
        }
    }
}
