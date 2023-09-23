using HeroesLike.Characters;
using System;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;

namespace HeroesLike.UI
{
    [RequireComponent(typeof(TextMeshPro))]
    public class Counter : MonoBehaviour
    {
        private TextMeshPro _text;
        private List<IDisposable> _disposables = new();

        private void Start()
        {
            _text = GetComponent<TextMeshPro>();
            var ch = transform.parent?.GetComponent<Character>();
            ch.ObserveEveryValueChanged(ch => ch.Pack).Subscribe(x => _text.text = x.ToString()).AddTo(_disposables);
            ch.ObserveEveryValueChanged(ch => ch.Alignment).Subscribe(x => _text.color = x switch { 0 => Color.green, 1 => Color.blue, _ => Color.green }).AddTo(_disposables);
        }

        private void OnDestroy() =>
            _disposables.ForEach(d => d.Dispose());
    }
}
