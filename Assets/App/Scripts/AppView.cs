using Kosu.UnityLibrary;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Docomo.Map5g
{
    public class AppView : MonoBehaviour
    {
        public System.Action<GameObject> OnClickDot;

        [SerializeField]
        private GameObject _dotPrefab;

        [SerializeField]
        private Transform _dotParent;

        private DotView[,] _dotArr = null;

        private RaycastHit _hit; // 画面クリック検知用

        private System.IDisposable _mouseEventStream = null;

        private System.IDisposable _idleWaveStream = null;

        [SerializeField]
        private Texture2D _maskTexture;

        [SerializeField]
        private Color _maskThresholdColor;

        private Material _dotSharedMaterial = null;

        [SerializeField]
        private IdleWaveKernel _idleWaveKernel;


        #region MonoBehavior functions
        private void Start()
        {
            _dotSharedMaterial = _dotPrefab.GetComponent<Renderer>().sharedMaterial;
        }

        private void OnEnable()
        {
            _mouseEventStream = UniRxUtility.ObserveGetMouseButtonDown(0, () =>
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out _hit))
                {
                    if (_hit.collider.gameObject.tag == "Dot")
                    {
                        OnClickDot.SafeInvoke(_hit.collider.gameObject);
                    }
                }
            }, gameObject);
        }

        private void OnDisable()
        {
            _mouseEventStream.Dispose();
            if (IsIdleWaving()) StopIdleWave();
        }
        #endregion

        // TODO:C : カメラの位置をちょうどマップの上に自動でくるように設定
        // TODO:W : IEnumratorで1フレーム当たりの付加を下げるでも良さそう
        public void InitMap(int width, int height, float wGapSize, float hGapSize)
        {
            _dotArr = new DotView[width, height];
            //_dotArr = new GameObject[width, height];
            var dotPos = new Vector2(0.0f, 0.0f);
            var propertyBlock = new MaterialPropertyBlock();

            for (int w = 0; w < width; w++)
            {
                for (int h = 0; h < height; h++)
                {
                    var dot = Instantiate(_dotPrefab);
                    dot.transform.SetParent(_dotParent.transform);
                    dot.transform.SetLocalPositionX(wGapSize * w);
                    dot.transform.SetLocalPositionZ(hGapSize * h);
                    // ドット全体の中での自身の位置をUV座標形式でマテリアルに保存
                    propertyBlock.SetVector("_Offset", new Vector2(w / (float)width,　h / (float)height));
                    dot.GetComponent<Renderer>().SetPropertyBlock(propertyBlock);

                    _dotArr[w, h] = dot.GetComponent<DotView>();
                    _dotArr[w, h].SetId(w, h);
                }
            }

            // マスク処理
            MaskMap();
        }

        // TODO:C : デバック用にマップの削除処理 + 削除完了を伝えるイベント実装
        //public void DeleteMap() {}

        // TODO:C : デバック用にマップの再構成処理を実装
        //public void UpdateMap() {}

        public bool HasInitMap()
        {
            return (_dotArr != null && _dotArr.Length > 0);
        }

        // TODO:C : GPUで実装
        // TODO:W : IEnumratorで1フレーム当たりの付加を下げるのもあり
        //public void MaskMap(Texture2D maskTex, Color thresholdColor)
        private void MaskMap()
        {
            if (!HasInitMap())
            {
                Debug.LogError("[AppView.MaskMap] Fail to mask due to unexcution of map init.");
                return;
            }

            var wDotNum = _dotArr.GetLength(0);
            var hDotNum = _dotArr.GetLength(1);
            for (int w = 0; w < wDotNum; w++)
            {
                for (int h = 0; h < hDotNum; h++)
                {
                    // ドットの位置に対応するテクスチャのピクセル情報を取得
                    float u = w / (float)wDotNum;
                    float v = h / (float)hDotNum;
                    var maskColor = _maskTexture.GetPixel(
                        Mathf.FloorToInt(_maskTexture.width * u), 
                        Mathf.FloorToInt(_maskTexture.height * v));

                    // マスク処理
                    if (!maskColor.IsDarkerThan(_maskThresholdColor))
                        _dotArr[w, h].gameObject.SetActive(true);
                }
            }
        }

        #region 波紋アニメーション
        public void Ripple(Vector2 startOffset, int rippleRange)
        {
            var rippleDots = GetElementsInRange<DotView>(startOffset, _dotArr, rippleRange);

            float maxDistance = 0.0f;
            foreach (var dot in rippleDots)
            {
                var distanceFromStartOffset = Vector2.Distance(startOffset, new Vector2(dot.transform.position.x, dot.transform.position.z));
                maxDistance = Mathf.Max(maxDistance, distanceFromStartOffset);
            }

            foreach (var dot in rippleDots)
            {
                var distance = Vector2.Distance(startOffset, new Vector2(dot.transform.position.x, dot.transform.position.z));
                var distanceRatio = distance / maxDistance;
                dot.StartRipple(distanceRatio);
            }
        }
        #endregion // 波紋アニメーション


        // TODO:C : コンピュートシェーダーで実装
        #region 待機中波アニメーション
        public void StartIdleWave()
        {
            if (IsIdleWaving()) return;
            _idleWaveStream = Observable.EveryUpdate().Subscribe(_ =>
            {
                _idleWaveKernel.UpdateBuffer();
                _dotSharedMaterial.SetTexture("_ScaleBuffer", _idleWaveKernel.Buffer);
            });
        }

        public void StopIdleWave()
        {
            if (!IsIdleWaving()) return;

            _idleWaveStream.Dispose();
            _idleWaveStream = null;

            _idleWaveKernel.ResetBuffer();
            _dotSharedMaterial.SetTexture("_ScaleBuffer", _idleWaveKernel.Buffer);
        }

        public bool IsIdleWaving()
        {
            return _idleWaveStream != null;
        }
        #endregion

        // Utility
        // ここをもっとシンプルな形に変更 : 試行回数減らす
        // 二次元配列の特定の要素から指定した範囲内の要素をまとめたListを返す
        /*
         * 1. 要素数が[4, 4]で範囲が1以内の場合
         * +xxx
         * +x-x
         * +xxx
         * ++++
         * 
         * "-","x"をまとめたリストを返す
         * "-" : 指定した要素
         * "x" : 範囲内の要素
         * "+" : 範囲外の要素
         * 
         * Note : 試行回数 (range + 1)2乗
         */
        private List<T> GetElementsInRange<T>(Vector2 offset, T[,] arr2D, int range)
        {
            var list = new List<T>();
            var xLength = arr2D.GetLength(0);
            var yLength = arr2D.GetLength(1);

            for (int x = 0; x < range + 1; x++)
            {
                for (int y = 0; y < range + 1; y++)
                {
                    if (x == 0)
                    {
                        if (y == 0)
                        {
                            list.Add(arr2D[(int)offset.x, (int)offset.y]);
                        }
                        else
                        {
                            if (yLength > offset.y + y) list.Add(arr2D[(int)offset.x, (int)offset.y + y]);
                            if (0 <= offset.y - y) list.Add(arr2D[(int)offset.x, (int)offset.y - y]);
                        }
                    }
                    else
                    {
                        if (xLength > offset.x + x)
                        {
                            if (y == 0)
                            {
                                list.Add(arr2D[(int)offset.x + x, (int)offset.y]);
                            }
                            else
                            {
                                if (yLength > offset.y + y) list.Add(arr2D[(int)offset.x + x, (int)offset.y + y]);
                                if (0 <= offset.y - y) list.Add(arr2D[(int)offset.x + x, (int)offset.y - y]);
                            }
                        }
                        if (0 <= offset.x - x)
                        {
                            if (y == 0)
                            {
                                list.Add(arr2D[(int)offset.x - x, (int)offset.y]);
                            }
                            else
                            {
                                if (yLength > offset.y + y) list.Add(arr2D[(int)offset.x - x, (int)offset.y + y]);
                                if (0 <= offset.y - y) list.Add(arr2D[(int)offset.x - x, (int)offset.y - y]);
                            }
                        }
                    }
                }
            }

            return list;
        }
    }
}