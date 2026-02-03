using UnityEngine;

[RequireComponent(typeof(LineRenderer), typeof(CircleCollider2D))]
public class GrapplePoint : MonoBehaviour
{
    [Header("Ayarlar")]
    public float range = 5f; // Salınma menzili
    public int segments = 50; // Çember kalitesi

    private LineRenderer line;
    private CircleCollider2D col;

    void Start()
    {
        line = GetComponent<LineRenderer>();
        col = GetComponent<CircleCollider2D>();
        
        line.loop = true;
        line.positionCount = segments;
        line.useWorldSpace = false; // Obje ile hareket etsin
        
        // Collider'ı Trigger yap ve boyutunu ayarla
        col.isTrigger = true;
        col.radius = range;
        
        DrawRangeCircle();
    }

    void OnValidate()
    {
        if (line == null) line = GetComponent<LineRenderer>();
        if (col == null) col = GetComponent<CircleCollider2D>();
        
        if (col != null)
        {
            col.radius = range;
            col.isTrigger = true;
        }
        
        DrawRangeCircle();
    }

    void DrawRangeCircle()
    {
        if (line == null) return;
        
        line.positionCount = segments; // Ensure rigid match
        line.loop = true;

        float angle = 0f;
        for (int i = 0; i < segments; i++)
        {
            float x = Mathf.Sin(Mathf.Deg2Rad * angle) * range;
            float y = Mathf.Cos(Mathf.Deg2Rad * angle) * range;

            line.SetPosition(i, new Vector3(x, y, 0));

            angle += (360f / segments);
        }
    }
    
    // Gizmos ile editörde de görelim
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
