using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Docomo.Map5g
{
    public class AppModel
    {
        //　クリックされたDot
        private ReactiveProperty<GameObject> _clickedDot;
        public IReadOnlyReactiveProperty<GameObject> ClickedDot { get { return _clickedDot; } }
        public void SetClickedDot(GameObject clickedDot) { _clickedDot.Value = clickedDot; }


        public AppModel()
        {
            _clickedDot = new ReactiveProperty<GameObject>(new GameObject());
        }
    }
}