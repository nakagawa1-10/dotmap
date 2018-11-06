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

        // TODO:C : 保管場所としてもっと良い場所ありそうなんで検討
        [SerializeField]
        private Texture2D _maskTexture;

        // TODO:C : 保管場所としてもっと良い場所ありそうなんで検討
        [SerializeField]
        private Color _maskThresholdColor;


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
            _view.MaskMap(_maskTexture, _maskThresholdColor);
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
            _streams.Add(_model.ClickedDot.Subscribe(OnChangeClickedDot));
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

        private void OnChangeClickedDot(GameObject dot)
        {
            if (_view.HasInitMap())
            {
                var offset = new Vector2(dot.transform.position.x, dot.transform.position.z);
                _view.Ripple(offset, 10);
            }
        }

        private void OnClickDot(GameObject dot)
        {
            _model.SetClickedDot(dot);
        }
    }
}