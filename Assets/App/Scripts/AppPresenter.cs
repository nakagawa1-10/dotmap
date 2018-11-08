using Kosu.UnityLibrary;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Docomo.Map5g
{
    public class AppPresenter : MonoBehaviour
    {
        private AppModel _model;

        [SerializeField]
        private AppView _view;

        private List<System.IDisposable> _streams = new List<System.IDisposable>();

        private MapSetting _mapSetting;


        #region MonoBehaviour functions
        private void Awake()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 30;

            _model = new AppModel();
            _mapSetting = DataUtility.LoadDataFromJson<MapSetting>(MapSetting.PATH);
            // TODO:S : デバック機構作成
        }

        private void OnEnable()
        {
            Bind();
        }

        private void OnDisable()
        {
            Unbind();
        }

        private void Start()
        {
            _view.InitMap(_mapSetting.Width, _mapSetting.Height, _mapSetting.WidthGapSize, _mapSetting.HeightGapSize);
            _view.StartIdleWave();
        }

        private void OnApplicationQuit()
        {
            DataUtility.SaveDataToJson(_mapSetting, MapSetting.PATH);
        }
        #endregion // MonoBehaviour functions

        private void Bind()
        {
            _view.OnClickDot = OnClickDot;
            _streams.Add(_model.ClickedDot.Subscribe(OnChangeClickedDotId));
            // Idle Waviing Toggle for debug
            _streams.Add(UniRxUtility.ObserveInputKeyDown(KeyCode.I, () =>
            {
                if (_view.IsIdleWaving())
                    _view.StopIdleWave();
                else
                    _view.StartIdleWave();
            }, gameObject));
        }

        private void Unbind()
        {
            foreach (var stream in _streams)
            {
                stream.Dispose();
            }

            _streams.Clear();
        }

        private void OnChangeClickedDotId(string dotId)
        {
            if (dotId.IsNullOrEmpty())
            {
                return;
            }

            var dotOffsetStr = dotId.Split('_');
            var dotOffset = new Vector2(int.Parse(dotOffsetStr[0]), int.Parse(dotOffsetStr[1]));
            if (_view.HasInitMap())
            {
                var offset = new Vector2(dotOffset.x, dotOffset.y);
                // TODO:M : ここの変数外部化
                _view.Ripple(offset, 13);
            }

            dotId = string.Empty;
        }

        private void OnClickDot(GameObject dot)
        {
            string dotId = dot.GetComponent<DotView>().Id;
            _model.SetClickedDotId(dotId);
        }
    }
}