using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndState : ManagerState {

    // Cover
    private GameObject cover;
    private Material coverMaterial;

    // Constructor with context of central manager
    public EndState(GameManager.Context context) : base(context) { }

    // Initializer for the state
    protected override void Init()
    {
        base.Init();
        Debug.Log("Entering state 4 - End ...");
        // Create cover
        cover = CreateCover();
        coverMaterial = cover.GetComponent<Renderer>().material;
    }

    // Internal execution called once per frame
    protected override void Execute()
    {
        base.Execute();
        // Showing cover
        if (coverMaterial.color.a < 1f)
        {
            coverMaterial.color = new Color(0, 0, 0, coverMaterial.color.a + 60f / 10000f);
        }
    }

    // Deinitializer for the state
    protected override void Deinit()
    {
        base.Deinit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // Condition to stop the state
    protected override bool ShouldBeEnded()
    {
        return coverMaterial.color.a >= 1f;
    }

    // Create a spherical cover
    protected GameObject CreateCover()
    {
        GameObject cover = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        cover.transform.parent = Camera.main.transform;
        cover.transform.localPosition = new Vector3(0, 0, 0);
        float r = 0.3f;
        cover.transform.localScale = new Vector3(r, r, r);
        cover.GetComponent<Renderer>().material.shader = Shader.Find("Transparent/Diffuse");
        cover.GetComponent<Renderer>().material.color = new Color(0, 0, 0, 0);
        cover.name = "Cover";
        cover.transform.localRotation = Quaternion.Euler(new Vector3());
        // Flip normal
        MeshFilter meshFilter = cover.GetComponent<MeshFilter>();
        Mesh mesh = meshFilter.mesh;
        int[] triangles = mesh.triangles;
        Array.Reverse(triangles);
        mesh.triangles = triangles;
        return cover;
    }
}
