using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineDrawer : MonoBehaviour
{
    private LineRenderer lineRenderer;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();

        StartCoroutine("DestroyAfterDisplay");
    }

    public void SetLine(Vector3 startPos, Vector3 endPos)
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.SetPosition(0, startPos); // Start position
        lineRenderer.SetPosition(1, endPos);   // End position
    }

    IEnumerator DestroyAfterDisplay()
    {
        yield return new WaitForSeconds(0.3f);
        Destroy(gameObject);
    }
}
