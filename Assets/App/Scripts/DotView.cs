using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Docomo.Map5g
{
    public class DotView : MonoBehaviour
    {
        // ドット全体の中での座標が入る : x_y
        private string _id = "";

        public string Id
        {
            get { return _id; }
        }

        private Vector3 _originPos;

        private bool _isRippling = false;

        private float _ripplingTime = 0.0f;

        private float _rippleDuration = 2.0f;

        private float _rippleIntensity = 1.0f;

        private float _distanceRatio = 1.0f;

        private float _rippleDecay = 1.0f;

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

        public void StartRipple(float distanceRatio, float intensity = 6.0f, float duration = 1.3f)
        {
            _ripplingTime = 0.0f;
            _isRippling = true;
            _distanceRatio = distanceRatio;
            _rippleIntensity = intensity;
            _rippleDuration = duration;
            _rippleDecay = 1.0f;
        }

        private void Ripple()
        {
            // 波紋アニメーション終了
            if (_ripplingTime > _rippleDuration)
            {
                _isRippling = false;
            }

            if (!_isRippling)
            {
                _rippleDecay = Mathf.Lerp(_rippleDecay, 0.0f, 0.05f);
            }

            // TODO:S : マジックナンバーを減らしたい
            // 波紋の高さ決定
            var magnitude = Mathf.Cos(Mathf.PI * 3.0f * (_distanceRatio - (_ripplingTime * 0.5f)));// + 1.0f;
            var changeRatio = (Mathf.Cos(Mathf.PI * (_distanceRatio - (_ripplingTime * 0.5f))) + 1.0f) * 0.5f;
            var plusYPos = magnitude * changeRatio * _rippleIntensity;
            // クリックしたドットからの距離に応じて変化量を減衰させる
            plusYPos *= (1.0f - _distanceRatio);
            // アニメーションの減衰
            plusYPos *= _rippleDecay;
            var targetYPos = _originPos.y + plusYPos;

            // 位置を更新
            transform.position = new Vector3(_originPos.x, Mathf.Lerp(transform.position.y, targetYPos, 0.3f), _originPos.z);

            _ripplingTime += Time.deltaTime;
        }

        public void SetId(int x, int y)
        {
            _id = x + "_" + y;
        }
    }
}