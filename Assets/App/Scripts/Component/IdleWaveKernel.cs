using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Docomo.Map5g
{
    public class IdleWaveKernel : MonoBehaviour
    {
        [SerializeField]
        private Shader _kernelShader;

        private Material _kernelMaterial;

        private RenderTexture _idleWaveScaleBuffer1 = null;

        private RenderTexture _idleWaveScaleBuffer2 = null;

        public RenderTexture Buffer
        {
            get { return _idleWaveScaleBuffer2; }
        }

        private bool _needsReset = true;


        public void UpdateBuffer()
        {
            if (_needsReset) ResetResources();

            SwapBuffersAndInvokeKernels();
        }

        public void ResetBuffer()
        {
            //_kernelMaterial.SetTexture("_PositionBuffer", _idleWaveScaleBuffer2);
            Graphics.Blit(null, _idleWaveScaleBuffer2, _kernelMaterial, 2);
        }

        private RenderTexture CreateBuffer()
        {
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
            Graphics.Blit(null, _idleWaveScaleBuffer2, _kernelMaterial, 0);
        }

        private void SwapBuffersAndInvokeKernels()
        {
            var tempPosition = _idleWaveScaleBuffer1;
            _idleWaveScaleBuffer1 = _idleWaveScaleBuffer2;
            _idleWaveScaleBuffer2 = tempPosition;

            _kernelMaterial.SetTexture("_PositionBuffer", _idleWaveScaleBuffer2);
            Graphics.Blit(null, _idleWaveScaleBuffer2, _kernelMaterial, 1);
        }

        private void ResetResources()
        {
            ClearResources();

            _idleWaveScaleBuffer1 = CreateBuffer();
            _idleWaveScaleBuffer2 = CreateBuffer();
            if (!_kernelMaterial) _kernelMaterial = CreateMaterial(_kernelShader);

            InitializeBuffers();

            _needsReset = false;
        }

        private void ClearResources()
        {
            if (_idleWaveScaleBuffer1) DestroyImmediate(_idleWaveScaleBuffer1);
            if (_idleWaveScaleBuffer2) DestroyImmediate(_idleWaveScaleBuffer2);
            if (_kernelMaterial) DestroyImmediate(_kernelMaterial);
        }
    }
}