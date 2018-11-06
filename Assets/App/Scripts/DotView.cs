using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Docomo.Map5g
{
    public class DotView : MonoBehaviour
    {
        private Vector3 _originPos;

        private bool _isWaving = false;

        private float _wavingTime = 0.0f;

        private float _waveSec = 2.0f;

        private float _waveIntensity = 1.0f;

        private float _distanceRatio = 1.0f;

        private float _waveDecay = 1.0f;

        private System.IDisposable _rippleStream;


        private void OnEnable()
        {
            _rippleStream = Observable.EveryUpdate().Subscribe(_ => {
                Ripple();
            });
        }

        private void OnDisable()
        {
            _rippleStream.Dispose();
        }

        private void Start()
        {
            _originPos = transform.position;
        }

        public void StartRipple(float distanceRatio, float waveIntensity = 6.0f, float waveSec = 1.3f)
        {
            _wavingTime = 0.0f;
            _isWaving = true;
            _distanceRatio = distanceRatio;
            _waveIntensity = waveIntensity;
            _waveSec = waveSec;
            _waveDecay = 1.0f;
        }

        private void Ripple()
        {
            // 波紋アニメーション終了
            if (_wavingTime > _waveSec)
            {
                _isWaving = false;
            }

            if (!_isWaving)
            {
                _waveDecay = Mathf.Lerp(_waveDecay, 0.0f, 0.05f);
            }

            // 波紋の高さ決定
            var magnitude = Mathf.Cos(Mathf.PI * 3.0f * (_distanceRatio - (_wavingTime * 0.5f)));// + 1.0f;
            var changeRatio = (Mathf.Cos(Mathf.PI * (_distanceRatio - (_wavingTime * 0.5f))) + 1.0f) * 0.5f;
            var plusYPos = magnitude * _waveIntensity * changeRatio;
            // クリックしたドットからの距離に応じて変化量を減衰させる
            plusYPos *= (1.0f - _distanceRatio);
            // アニメーションの減衰
            plusYPos *= _waveDecay;
            var targetYPos = _originPos.y + plusYPos;

            // 位置を更新
            transform.position = new Vector3(_originPos.x, Mathf.Lerp(transform.position.y, targetYPos, 0.3f), _originPos.z);

            _wavingTime += Time.deltaTime;
        }
    }
}