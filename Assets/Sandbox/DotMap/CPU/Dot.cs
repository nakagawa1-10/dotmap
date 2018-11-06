using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sandbox.DotMap
{
    public class Dot : MonoBehaviour
    {
        [SerializeField]
        private Vector2 _offset = new Vector2(0.0f, 0.0f);
        public Vector2 Offset
        {
            get { return _offset; }
            set { _offset = value; }
        }

        private Vector3 _originPos;

        private Coroutine _listCoroutine = null;

        private bool _isWaving = false;

        private float _wavingTime = 0.0f;

        private float _waveSec = 2.0f;

        private float _waveIntensity = 1.0f;

        private float _distanceRatio = 1.0f;

        private float _waveDecay = 1.0f;


        private void Start()
        {
            _originPos = transform.position;
        }

        private void Update()
        {
            // 波うちアニメーション
            Wave();
        }

        public void StartWave(float distanceRatio, float waveIntensity = 1.0f, float waveSec = 5.0f)
        {
            _wavingTime = 0.0f;
            _isWaving = true;
            _distanceRatio = distanceRatio;
            _waveIntensity = waveIntensity;
            _waveSec = waveSec;
            _waveDecay = 1.0f;
        }

        private void Wave()
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


        public void MoveUp(float magnitude)
        {
            transform.position = new Vector3(_originPos.x, _originPos.y + magnitude, _originPos.z);
        }


        public void Lift()
        {
            if (!gameObject.activeSelf) return;
            if (_listCoroutine != null) StopCoroutine(_listCoroutine);

            _listCoroutine = StartCoroutine(ListEnumerator());
        }

        private IEnumerator ListEnumerator()
        {
            transform.DOMoveY(_originPos.y + 2, 1.0f);
            //transform.position = new Vector3(_originPos.x, _originPos.y + 1, _originPos.z);

            yield return new WaitForSeconds(1.0f);

            transform.DOMoveY(_originPos.y, 1.0f);
            //transform.position = new Vector3(_originPos.x, _originPos.y, _originPos.z);

            _listCoroutine = null;
        }
    }
}