using TMPro;
using UnityTools.Timer;

namespace HeroesLike.UI
{
    public class BattleLogger : Singleton<BattleLogger>
    {
        private CTimer timer;
        private TextMeshProUGUI _tmp;
        private TextMeshProUGUI _tmpDamage;

        private void Awake() =>
            timer = new();

        public void DamageInfo(string text) =>
            _tmpDamage.text = text;

        public void Info(string text)
        {
            _tmp.enabled = true;
            _tmp.text = text;
            timer.Dispose();
            timer.Delay(2.0f, () => _tmp.enabled = false);
        }

        public void Persistent(string text)
        {
            timer.Dispose();
            _tmp.enabled = true;
            _tmp.text = text;
        }

        public TextMeshProUGUI TMP { set => _tmp = value; }
        public TextMeshProUGUI TMPDamage { set => _tmpDamage = value; }
    }
}