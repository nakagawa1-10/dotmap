using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO : スレッドの結果をデバックに表示
public class MyFirstCompute : MonoBehaviour
{
    [SerializeField]
    private ComputeShader _shader;

    private int _kernelId;
    private int _texKernalId;
    private int _texKernalBId;

    private ComputeBuffer _intBuffer;

    private int _totalCount = 10;

    [SerializeField]
    private int _groupCount = 2;

    [SerializeField]
    private GameObject _planeA;
    [SerializeField]
    private GameObject _planeB;

    private RenderTexture _renderTexture;

    private RenderTexture _renderTextureB;

    private struct ThreadSize
    {
        public int x;
        public int y;
        public int z;

        public ThreadSize(uint x, uint y, uint z)
        {
            this.x = (int)x;
            this.y = (int)y;
            this.z = (int)z;
        }
    }

    private ThreadSize _texKernelThreadSize;
    private ThreadSize _texKernelBThreadSize;


    private void Start ()
    {
        InitKernel();

        _planeA.GetComponent<Renderer>().material.mainTexture = _renderTexture;
        _planeB.GetComponent<Renderer>().material.mainTexture = _renderTextureB;
    }

    private void OnDestroy()
    {
        if (_intBuffer != null)
        {
            _intBuffer.Release();
            _intBuffer = null;
        }

        // TODO:S : RenderTextureの解放方法を探る
        //if (_renderTexture != null)
        //{ 
        //}
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Alpha1))
        {
            ExcuteCSMain();
        }

        //ExcuteKernelFunctinoA();
        if (Input.GetKeyUp(KeyCode.Alpha2))
        {
            ExcuteKernelFunctinoA();
        }

        //ExcuteKernelFunctinoB();
        if (Input.GetKeyUp(KeyCode.Alpha3))
        {
            ExcuteKernelFunctinoB();
        }
    }

    // TODO:C : uintの型ってintと何が違うの?
    private void InitKernel()
    {
        // get kernel id
        _kernelId = _shader.FindKernel("CSMain");
        _texKernalId = _shader.FindKernel("KernelFunctinoA");
        _texKernalBId = _shader.FindKernel("KernelFunctinoB");

        // get kernel size
        uint threadSizeX, threadSizeY, threadSizeZ;
        _shader.GetKernelThreadGroupSizes(_texKernalId, out threadSizeX, out threadSizeY, out threadSizeZ);
        _texKernelThreadSize = new ThreadSize(threadSizeX, threadSizeY, threadSizeZ);
        _shader.GetKernelThreadGroupSizes(_texKernalBId, out threadSizeX, out threadSizeY, out threadSizeZ);
        _texKernelBThreadSize = new ThreadSize(threadSizeX, threadSizeY, threadSizeZ);

        // init buffer
        _intBuffer = new ComputeBuffer(_totalCount, sizeof(int));
        _renderTexture = new RenderTexture(512, 512, 0, RenderTextureFormat.ARGB32);
        _renderTexture.hideFlags = HideFlags.DontSave;
        _renderTexture.enableRandomWrite = true;
        _renderTexture.Create();
        _renderTextureB = new RenderTexture(512, 512, 0, RenderTextureFormat.ARGB32);
        _renderTextureB.hideFlags = HideFlags.DontSave;
        _renderTextureB.enableRandomWrite = true;
        _renderTextureB.Create();

        // set buffer
        _shader.SetBuffer(_kernelId, "Result", _intBuffer);
        _shader.SetTexture(_texKernalId, "TextureBuffer", _renderTexture);
        _shader.SetTexture(_texKernalBId, "TextureBufferB", _renderTextureB);
    }

    private void ExcuteCSMain()
    {
        Debug.Log("[CSMain:結果]");
        _shader.Dispatch(_kernelId, _groupCount, 1, 1);

        int[] result = new int[_totalCount];
        _intBuffer.GetData(result);

        for (int i = 0; i < result.Length; i++)
        {
            Debug.Log("id : " + result[i]);
        }
    }

    private void ExcuteKernelFunctinoA()
    {
        Debug.Log("ExcuteKernelFunction:結果");
        _shader.Dispatch(
            _texKernalId, 
            _renderTexture.width / _texKernelThreadSize.x,
            _renderTexture.height / _texKernelThreadSize.y, 
            _texKernelThreadSize.z);

        //_planeA.GetComponent<Renderer>().material.mainTexture = _renderTexture;
    }

    private void ExcuteKernelFunctinoB()
    {
        Debug.Log("ExcuteKernelFunctionB:結果");
        _shader.Dispatch(
            _texKernalBId,
            _renderTextureB.width / _texKernelBThreadSize.x,
            _renderTextureB.height / _texKernelBThreadSize.y,
            _texKernelBThreadSize.z);

        //_planeB.GetComponent<Renderer>().material.mainTexture = _renderTextureB;
    }
}
