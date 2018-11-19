using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstCRT : MonoBehaviour {

    [SerializeField]
    CustomRenderTexture _crt;

    [SerializeField]
    Shader _shader;

    Material _material;

	// Use this for initialization
	void Start () {
        _material = new Material(_shader);
        _material.hideFlags = HideFlags.DontSave;

        _crt.material = _material;
	}
	
	// Update is called once per frame
	void Update () {
        _crt.Update(0);
    }
}
