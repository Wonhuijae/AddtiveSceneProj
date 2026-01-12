using UnityEngine;

public class GlobalBoxClipController : MonoBehaviour
{
    public Vector3 boxCenter;
    public Vector3 boxSize = new Vector3(10, 5, 10);
    public float sectionThickness = 0.02f;

    void Update()
    {
        Vector3 half = boxSize * 0.5f;
        Vector3 boxMin = boxCenter - half;
        Vector3 boxMax = boxCenter + half;

        Shader.SetGlobalVector("_BoxMin", boxMin);
        Shader.SetGlobalVector("_BoxMax", boxMax);
        Shader.SetGlobalFloat("_SectionThickness", sectionThickness);
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(boxCenter, boxSize);
    }
#endif
}
