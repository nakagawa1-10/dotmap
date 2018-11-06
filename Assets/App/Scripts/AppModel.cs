using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Docomo.Map5g
{
    public class AppModel
    {
        private ReactiveProperty<string> _clickedDotId;
        public IReadOnlyReactiveProperty<string> ClickedDot { get { return _clickedDotId; } }
        public void SetClickedDotId(string clickedDotId) { _clickedDotId.Value = clickedDotId; }


        public AppModel()
        {
            _clickedDotId = new ReactiveProperty<string>(string.Empty);
        }
    }
}