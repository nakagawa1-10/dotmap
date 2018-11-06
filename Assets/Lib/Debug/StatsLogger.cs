using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Kosu.UnityLibrary
{
    public class StatsLogger : MonoBehaviour
    {

        [SerializeField]
        private Text _logStringPrefab;

        [SerializeField]
        private RectTransform _logStringParent;

        private void OnEnable()
        {
            Application.logMessageReceivedThreaded += LogCallbackHandler;
        }

        private void OnDisable()
        {
            Application.logMessageReceivedThreaded -= LogCallbackHandler;
        }

        private void LogCallbackHandler(string logString, string stackTrace, LogType type)
        {
            string log = "";

            switch (type)
            {
                case LogType.Log:
                    log = "<color=\"#00FF11\">";
                    break;

                case LogType.Warning:
                    log = "<color=\"#FFF000\">";
                    break;

                case LogType.Error:
                case LogType.Exception:
                    log = "<color=\"#FF0500\">";
                    break;
            }

            log += string.Format("[{0}][{1}] {2}</color>",
                                 type.ToString(),
                                 System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),
                                 logString);
            var text = Instantiate(_logStringPrefab);
            text.text = log;
            text.transform.SetParent(_logStringParent, false);

            while (_logStringParent.childCount > 8)
            {
                var child = _logStringParent.GetChild(0);
                DestroyImmediate(child.gameObject);
            }
        }

    }
}
