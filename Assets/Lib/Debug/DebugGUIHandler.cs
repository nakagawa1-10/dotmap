using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kosu.UnityLibrary
{

    public class DebugGUIHandler : MonoBehaviour
    {
        [SerializeField]
        protected GUISkin _skin;

        private bool _isEnable;

        private int _frameCount;
        private float _prevTime;
        private float _fps;

        private List<System.IDisposable> _stream = new List<System.IDisposable>();

        private void OnEnable()
        {
            _stream.Add(UniRxUtility.ObserveInputKeyDown(KeyCode.F1, () =>
            {
                _isEnable = !_isEnable;
            }, gameObject));
        }

        private void OnDisable()
        {
            foreach (var stream in _stream)
            {
                stream.Dispose();
            }

            _stream.Clear();
            _stream = null;
            OnAfterDisable();
        }

        protected virtual void OnAfterDisable()
        {
        }

        private void Update()
        {
            ++_frameCount;
            float time = Time.realtimeSinceStartup - _prevTime;

            if (time > 0.5f)
            {
                _fps = _frameCount / time;
                _frameCount = 0;
                _prevTime = Time.realtimeSinceStartup;
            }
        }

        private void OnGUI()
        {
            if (!_isEnable)
            {
                return;
            }

            GUI.skin = _skin;
            GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height), "debug", _skin.box);
            {
                CreateDebugGUI();
            }
            GUILayout.EndArea();
        }

        protected virtual void CreateDebugGUI()
        {
            GUILayout.BeginArea(new Rect(20, 50, Screen.width, 50));
            {
                GUILayout.Label(string.Format("FPS : {0}", _fps));
            }
            GUILayout.EndArea();
        }

    }
}
