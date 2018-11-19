using Kosu.UnityLibrary;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// モック
namespace Sandbox.DotMap
{
    public class SampleDotMap : MonoBehaviour
    {
        [SerializeField]
        private GameObject _dotObj;

        [SerializeField]
        private GameObject _dotParentObj;

        //[SerializeField]
        //private int _screenWidthPixelNum = 512;
        //[SerializeField]
        //private int _screenHeightPixelNum = 512;
        [SerializeField]
        private float _pixelSize = 0.5f;

        [SerializeField]
        private int _vDotNum = 512;
        [SerializeField]
        private int _hDotNum = 512;

        private int _texWidth = 0;
        private int _texHeight = 0;

        private Dot[ , ] _dots; // [縦, 横]

        private float _vInterval = 0.0f;
        private float _hInterval = 0.0f;

        [SerializeField]
        private Texture2D _maskTex;

        //private Color[] _maskColorArr;

        [SerializeField]
        private Color _maskThreshold;


        // 波アニメーション用
        [SerializeField]
        private Shader kernelShader;

        private RenderTexture _scaleBuffer1;
        private RenderTexture _scaleBuffer2;
        private Material _kernelMaterial;
        private bool needsReset = true;

        [SerializeField]
        private RawImage _rawImage;

        private Material _sharedDotMaterial;

        // 波紋アニメーション用
        private RaycastHit hit; // 画面クリック検知用
        [SerializeField]
        private Vector2 _liftUpRange = new Vector2(3f, 3f); // 奇数である必要がある
        [SerializeField]
        private float _waveMagnitude = 1.0f;
        [SerializeField]
        private float _waveSec = 2.5f;


        private void Start()
        {
            _texWidth = _maskTex.width;
            _texHeight = _maskTex.height;
            if (_vDotNum > _texHeight) _vDotNum = _texHeight;
            if (_hDotNum > _texWidth) _hDotNum = _texWidth;

            //_maskColorArr = _maskTex.GetPixels();
            _sharedDotMaterial = _dotObj.GetComponent<Renderer>().sharedMaterial;

            DrawMap();
        }

        // TODO : 後々UniRxのEveryFrameに置き換える
        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                //var mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10.0f);

                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); //マウスのポジションを取得してRayに代入

                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider.gameObject.tag == "Dot")
                    {
                        //Debug.Log("World pos : " + hit.point);
                        //Debug.Log(hit.collider.gameObject.GetComponent<Dot>().Offset + " : " +  hit.point);
                        OnClickDot(hit.collider.gameObject.GetComponent<Dot>());
                    }
                }
            }

            // ノイズテクスチャ
            //UpdateBuffer();
        }

        private void OnDestroy()
        {
            if (_scaleBuffer1) DestroyImmediate(_scaleBuffer1);
            if (_scaleBuffer2) DestroyImmediate(_scaleBuffer2);
        }

        #region 波紋アニメーション用
        private void OnClickDot(Dot dot)
        {
            //dot.Lift();
            var offset = dot.Offset;

            // 周囲のドットオブジェクトを取得
            var rangeDots = new List<Dot>();
            var vRange = ((int)_liftUpRange.y - 1) / 2;
            var hRange = ((int)_liftUpRange.x - 1) / 2;

            for (int v = 0; v < vRange + 1; v++)
            {
                for (int h = 0; h < hRange + 1; h++)
                {
                    if (v == 0)
                    {
                        if (h == 0)
                        {
                            rangeDots.Add(_dots[(int)offset.x, (int)offset.y + v]);
                        }
                        else
                        {
                            if (_hDotNum > offset.x + h) rangeDots.Add(_dots[(int)offset.x + h, (int)offset.y]);
                            if (0 <= offset.x - h) rangeDots.Add(_dots[(int)offset.x - h, (int)offset.y]);
                        }
                    }
                    else
                    {
                        if (_vDotNum > offset.y + v)
                        { 
                            if (h == 0)
                            {
                                rangeDots.Add(_dots[(int)offset.x + h, (int)offset.y + v]);
                            }
                            else
                            {
                                //if (_hDotNum > offset.x + h) _dots[(int)offset.x + h, (int)offset.y + v].Lift();
                                //if (0 <= offset.x - h) _dots[(int)offset.x - h, (int)offset.y + v].Lift();
                                if (_hDotNum > offset.x + h) rangeDots.Add(_dots[(int)offset.x + h, (int)offset.y + v]);
                                if (0 <= offset.x - h) rangeDots.Add(_dots[(int)offset.x - h, (int)offset.y + v]);
                            }
                        }
                        if (0 <= offset.y - v)
                        {
                            if (h == 0)
                            {
                                rangeDots.Add(_dots[(int)offset.x + h, (int)offset.y - v]);
                            }
                            else
                            {
                                //if (_hDotNum > offset.x + h) _dots[(int)offset.x + h, (int)offset.y - v].Lift();
                                //if (0 <= offset.x - h) _dots[(int)offset.x - h, (int)offset.y - v].Lift();
                                if (_hDotNum > offset.x + h) rangeDots.Add(_dots[(int)offset.x + h, (int)offset.y - v]);
                                if (0 <= offset.x - h) rangeDots.Add(_dots[(int)offset.x - h, (int)offset.y - v]);
                            }
                        }
                    }
                }
            }

            // 最大の距離を取得
            float maxDistance = 0.0f;
            foreach (var liftDot in rangeDots)
            {
                maxDistance = Mathf.Max(maxDistance, Vector2.Distance(dot.Offset, liftDot.Offset));
            }

            // cosを割り振り
            foreach (var liftDot in rangeDots)
            {
                var distance = Vector2.Distance(dot.Offset, liftDot.Offset);
                //var distanceRatio = distance / maxDistance;
                var distanceRatio = distance / maxDistance;
                liftDot.StartWave(distanceRatio, _waveMagnitude, _waveSec);
                //liftDot.MoveUp((Mathf.Cos(Mathf.PI * distanceRatio) + 1.0f) * _waveMagnitude);
                //Debug.Log(distance + " : " + (Mathf.Cos(Mathf.PI * distanceRatio) + 1.0f));
            }
        }
        #endregion

        #region 波アニメーション用スケールバッファー作成
        private void UpdateBuffer()
        {
            if (needsReset) ResetResources();

            SwapBuffersAndInvokeKernels();

            // Set Buffer Texture
            _sharedDotMaterial.SetTexture("_ScaleBuffer", _scaleBuffer2);
        }

        private RenderTexture CreateBuffer()
        {
            //var width = _hDotNum;
            //var height = _vDotNum;
            var width = 320;
            var height = 320;
            var buffer = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat);
            buffer.hideFlags = HideFlags.DontSave;
            buffer.filterMode = FilterMode.Point;
            buffer.wrapMode = TextureWrapMode.Repeat;
            return buffer;
        }

        private Material CreateMaterial(Shader shader)
        {
            var material = new Material(shader);
            material.hideFlags = HideFlags.DontSave;
            return material;
        }

        private void InitializeBuffers()
        {
            Graphics.Blit(null, _scaleBuffer2, _kernelMaterial, 0);
        }

        private void SwapBuffersAndInvokeKernels()
        {
            var tempPosition = _scaleBuffer1;
            _scaleBuffer1 = _scaleBuffer2;
            _scaleBuffer2 = tempPosition;

            _kernelMaterial.SetTexture("_PositionBuffer", _scaleBuffer1);
            Graphics.Blit(null, _scaleBuffer2, _kernelMaterial, 1);
        }

        private void ResetResources()
        {
            if (_scaleBuffer1) DestroyImmediate(_scaleBuffer1);
            if (_scaleBuffer2) DestroyImmediate(_scaleBuffer2);

            _scaleBuffer1 = CreateBuffer();
            _rawImage.texture = _scaleBuffer1;
            _scaleBuffer2 = CreateBuffer();

            if (!_kernelMaterial) _kernelMaterial = CreateMaterial(kernelShader);

            InitializeBuffers();

            needsReset = false;
        }
        #endregion


        private void DrawMap()
        {
            _dots = new Dot[_vDotNum, _hDotNum];
            int dotIndex = 0;

            // Set dots interval size;
            _vInterval = _texHeight * _pixelSize / _vDotNum;
            _hInterval = _texWidth * _pixelSize / _hDotNum;

            var propertyBlock = new MaterialPropertyBlock();

            // Draw dots
            var vPos = 0.0f;
            for (int v = 0; v < _vDotNum; v++)
            {
                var hPos = 0.0f;
                for (int h = 0; h < _hDotNum; h++)
                {
                    // ドット生成
                    var dot = Instantiate(_dotObj).GetComponent<Dot>();
                    dot.transform.SafeSetParent(_dotParentObj.transform);
                    dot.transform.position = new Vector3(hPos, 0.0f, vPos);
                    _dots[v, h] = dot;
                    dot.Offset = new Vector2(v, h);
                    //_dots[dotIndex] = dot;
                    // ScaleBuffer Offset
                    var uv = new Vector2(h / (float)_hDotNum, v / (float)_vDotNum);
                    propertyBlock.SetVector("_Offset", uv);
                    dot.GetComponent<Renderer>().SetPropertyBlock(propertyBlock);

                    // Mask
                    var maskH = h * Mathf.FloorToInt(((float)_texWidth / (float)_hDotNum));
                    maskH = (maskH >= _texWidth) ? _texWidth - 1 : maskH;
                    var maskV = v * Mathf.FloorToInt(((float)_texHeight / (float)_vDotNum));
                    maskV = (maskV >= _texHeight) ? _texHeight - 1 : maskV;

                    var maskColor = _maskTex.GetPixel(maskH, maskV);
                    if (maskColor.r > _maskThreshold.r ||
                        maskColor.g > _maskThreshold.g ||
                        maskColor.b > _maskThreshold.b)
                    {
                        //_dots[v, h].gameObject.SetActive(false);
                    }

                    hPos += _hInterval;

                    dotIndex++;
                }

                vPos += _vInterval;
                hPos = 0.0f;
            }
        }
    }
}