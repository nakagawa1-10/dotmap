using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Sandbox.GpuMap
{
    public class MapInstancing : MonoBehaviour
    {
        static readonly int ThreadBlockSize = 256;

        private struct DotData
        {
            public bool Enable;
            public Vector3 OriginPos;
            public Vector3 Position;
            public Vector3 Albedo;
        }

        // cubeの数
        [SerializeField]
        int _instanceCountX = 100;
        [SerializeField]
        int _instanceCountY = 100;

        int _instanceCount;

        [SerializeField]
        Vector3 _basePosition = new Vector3(0, 0, 0);

        [SerializeField]
        Vector3 _dotScale = new Vector3(1f, 1f, 1f);

        [SerializeField]
        Texture _maskTexture;

        [SerializeField]
        Color _maskColor;

        [SerializeField]
        Mesh _dotMesh;

        [SerializeField]
        ComputeShader _computeShader;

        int _updateKernelId = -1;

        ComputeBuffer _dotDataBuffer;

        [SerializeField]
        Shader _gpuInstanceShader;

        Material _dotMaterial;

        // GPU Instancingのための引数
        // [0] : index count per instance
        // [1] : instance count
        // [2] : start index location
        // [3] : base vertex location
        // [4] : start instance location
        uint[] _gpuInstancingArgs = new uint[5] { 0, 0, 0, 0, 0 };

        ComputeBuffer _gpuInstancingArgsBuffer;


        void Start()
        {
            _instanceCount = _instanceCountX * _instanceCountY;

            // バッファー初期化
            _dotDataBuffer = new ComputeBuffer(_instanceCount, Marshal.SizeOf(typeof(DotData)));
            _gpuInstancingArgsBuffer = new ComputeBuffer(1, _gpuInstancingArgs.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
            _gpuInstancingArgs[0] = (_dotMesh != null) ? _dotMesh.GetIndexCount(0) : 0;
            _gpuInstancingArgs[1] = (uint)_instanceCount;
            _gpuInstancingArgsBuffer.SetData(_gpuInstancingArgs);

            // Compute Shader初期化
            int kernelId = _computeShader.FindKernel("Init");
            _updateKernelId = _computeShader.FindKernel("Update");
            _computeShader.SetInt("_Width", _instanceCountX);
            _computeShader.SetInt("_Height", _instanceCountY);
            _computeShader.SetTexture(kernelId, "_MaskTexture", _maskTexture);
            _computeShader.SetInt("_MaskWidth", _maskTexture.width);
            _computeShader.SetInt("_MaskHeight", _maskTexture.height);
            _computeShader.SetVector("_MaskColor", new Vector4(_maskColor.r, _maskColor.g, _maskColor.b, _maskColor.a));
            _computeShader.SetBuffer(kernelId, "_DotDataBuffer", _dotDataBuffer);
            _computeShader.SetBuffer(_updateKernelId, "_DotDataBuffer", _dotDataBuffer);
            _computeShader.SetTexture(_updateKernelId, "_MaskTexture", _maskTexture);
            _computeShader.Dispatch(kernelId, (Mathf.CeilToInt(_instanceCount / ThreadBlockSize) + 1), 1, 1);

            // 描画用Shader初期化
            _dotMaterial = new Material(_gpuInstanceShader);
            _dotMaterial.hideFlags = HideFlags.DontSave;
        }

        void Update()
        {
            // コンピュートシェーダー更新
            _computeShader.SetVector("_BasePos", _basePosition);
            _computeShader.SetVector("_MaskColor", new Vector4(_maskColor.r, _maskColor.g, _maskColor.b, _maskColor.a));
            _computeShader.Dispatch(_updateKernelId, (Mathf.CeilToInt(_instanceCount / ThreadBlockSize) + 1), 1, 1);

            // 描画
            _dotMaterial.SetVector("_DotMeshScale", _dotScale);
            _dotMaterial.SetBuffer("_DotDataBuffer", _dotDataBuffer);
            Graphics.DrawMeshInstancedIndirect(_dotMesh, 0, _dotMaterial, new Bounds(new Vector3(0, 0, 0), new Vector3(100, 100, 100)), _gpuInstancingArgsBuffer);
        }

        void OnDestroy()
        {
            if (_dotDataBuffer != null)
            {
                _dotDataBuffer.Release();
                _dotDataBuffer = null;
            }

            if (_gpuInstancingArgsBuffer != null)
            {
                _gpuInstancingArgsBuffer.Release();
                _gpuInstancingArgsBuffer = null;
            }
        }
    }
}