using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class BallTrail : MonoBehaviour {
    public Shader curShader;
    private List<Vector3> offsets = new List<Vector3>(); // 存储前几帧的坐标
    private List<Material> mats = new List<Material>(); // 存储人物的材质，用于给shader传参数
    // Use this for initialization
    void Start () 
    {
        offsets.Add(transform.position);
        offsets.Add(transform.position);
        offsets.Add(transform.position);
        offsets.Add(transform.position);

        var skinMeshRenderer = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (var mr in skinMeshRenderer) 
            mats.Add(mr.material);

        var meshRenderer = gameObject.GetComponentsInChildren<MeshRenderer>();
        foreach (var mr in meshRenderer) 
            mats.Add(mr.material);

        foreach (var mat in mats)
            mat.shader = curShader;
    }
    
    // Update is called once per frame
    void Update () {
        foreach (var mat in mats) // 每帧将之前的位置传入shader中
        {
            mat.SetVector("_Offset0", offsets[3] - transform.position);
            mat.SetVector("_Offset1", offsets[2] - transform.position);
            mat.SetVector("_Offset2", offsets[1] - transform.position);
            mat.SetVector("_Offset3", offsets[0] - transform.position);
        }

        offsets.Add(transform.position);
        offsets.RemoveAt(0);
    }
}