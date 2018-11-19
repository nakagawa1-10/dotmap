using DG.Tweening;
using Kosu.UnityLibrary;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Docomo.Map5g
{
    // TODO:M : ここはどれだけ単純に実装できるか。あまりコードの綺麗さとかは気にしなくて良さそう
    public class DebugView : MonoBehaviour
    {
        [SerializeField]
        private CanvasGroup _group;


        #region MonoBehaviour functions
        private void Awake()
        {
            UniRxUtility.ObserveInputKeyDown(KeyCode.F1, () =>
            {
                ToggleVisible();
            }, gameObject);
            //UniRxUtility.ObserveInputKeyDown(KeyCode.F3, () =>
            //{
                
            //}, gameObject);
        }
        #endregion

        private void ToggleVisible()
        {
            if (IsShown())
            {
                Hide();
            }
            else
            {
                Show();
            }
        }

        private void Show()
        {
            _group.DOKill();
            _group.DOFade(1.0f, 0.2f);
        }

        private void Hide()
        {
            _group.DOKill();
            _group.DOFade(0.0f, 0.2f);
        }

        private bool IsShown()
        {
            return _group.alpha > 0.0f;
        }
    }
}