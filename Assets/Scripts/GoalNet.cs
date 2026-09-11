using UnityEngine;
using System.Collections;

// 평평한 Plane 메쉬(그물)를 받아, 공이 닿은 지점에서 퍼져나가는 찰랑임을 흉내낸다.
public class GoalNet : MonoBehaviour
{
    public float rippleStrength = 0.3f;
    public float rippleSpeed = 8f;
    public float damping = 3f;
    public float duration = 1.2f;

    private Mesh mesh;
    private Vector3[] originalVertices;
    private Vector3[] workingVertices;
    private Coroutine rippleRoutine;

    void Awake()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        mesh = Instantiate(mf.mesh);
        mf.mesh = mesh;
        originalVertices = mesh.vertices;
        workingVertices = new Vector3[originalVertices.Length];
    }

    public void Ripple(Vector3 worldImpactPoint)
    {
        if (rippleRoutine != null) StopCoroutine(rippleRoutine);
        rippleRoutine = StartCoroutine(RippleRoutine(transform.InverseTransformPoint(worldImpactPoint)));
    }

    IEnumerator RippleRoutine(Vector3 localImpact)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            for (int i = 0; i < originalVertices.Length; i++)
            {
                float dist = Vector3.Distance(originalVertices[i], localImpact);
                float wave = Mathf.Sin(dist * rippleSpeed - t * rippleSpeed * 3f) * rippleStrength;
                float falloff = Mathf.Exp(-t * damping) * Mathf.Exp(-dist * 0.5f);
                workingVertices[i] = originalVertices[i] + Vector3.up * wave * falloff;
            }
            mesh.vertices = workingVertices;
            mesh.RecalculateNormals();
            yield return null;
        }
        mesh.vertices = originalVertices;
        mesh.RecalculateNormals();
        rippleRoutine = null;
    }
}
