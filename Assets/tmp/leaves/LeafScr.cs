using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeafScr : MonoBehaviour
{
    [SerializeField][Range(0f, 1f)]
    private float _darkest = 0.7f;

    [SerializeField]
    private Material[] mat;

    // Start is called before the first frame update
    void Start()
    {
        float colInt = Random.Range(_darkest, 1);
        Color colour = new Color(colInt, colInt, colInt, 1);

        
        MeshRenderer[] allChildren = GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer childMat in allChildren)
        {
            childMat.material.color = colour;
        }
        

        //gameObject.GetComponent<Material>().color = colour;
    }
}
