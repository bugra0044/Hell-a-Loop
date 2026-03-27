using UnityEngine;

[RequireComponent(typeof(DistanceJoint2D))]
public class GrapplingHook : MonoBehaviour
{
    [Header("Ayarlar")]
    public float searchRadius = 10f; // Ne kadar uzaktaki noktayı görebilir?
    public LayerMask grappleLayer;   // "GrapplePoint" olan objelerin layerı (veya hepsi)
    public float swingForce = 15f;   // (Opsiyonel) Sallanırken hızlanmak için

    [Header("Görsel")]
    public LineRenderer ropeRenderer; // İpi çizen Renderer

    private DistanceJoint2D joint;
    private CharacterMovement movement;
    private Vector2 currentAnchor;
    private bool isAttached;

    void Start()
    {
        joint = GetComponent<DistanceJoint2D>();
        movement = GetComponent<CharacterMovement>();
        
        joint.enabled = false;
        if (ropeRenderer != null) 
        {
            ropeRenderer.positionCount = 2; // Garanti olsun
            ropeRenderer.enabled = false;
        }
    }

    void Update()
    {
        // Sol Tık Basıldı: Tutun (User requested Left Click)
        if (Input.GetMouseButtonDown(0))
        {
            TryAttach();
        }

        // Sol Tık Bırakıldı: Bırak
        if (Input.GetMouseButtonUp(0))
        {
            Detach();
        }

        if (isAttached)
        {
            // İpi Çiz
            if (ropeRenderer != null)
            {
                if (!ropeRenderer.enabled) ropeRenderer.enabled = true; // Force enable if swinging
                ropeRenderer.SetPosition(0, transform.position);
                ropeRenderer.SetPosition(1, currentAnchor);
            }
        }
        else
        {
             // Eğer bağlı değilsek ama renderer açıksa kapat (Safety)
             if (ropeRenderer != null && ropeRenderer.enabled)
             {
                 ropeRenderer.enabled = false;
             }
        }
    }

    void FixedUpdate()
    {
        if (isAttached)
        {
            // Opsiyonel: Sallanırken A/D ile hız verme (Salıncak mantığı)
            float h = Input.GetAxisRaw("Horizontal");
            if (Mathf.Abs(h) > 0.1f)
            {
                // Force applied perpendicular to the rope? Or just horizontal?
                // Simple horizontal force works for swinging usually.
                movement.GetRigidbody().AddForce(Vector2.right * h * swingForce, ForceMode2D.Force);
            }
        }
    }

    void TryAttach()
    {
        // En yakın noktayı bul
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, searchRadius);
        GrapplePoint nearest = null;
        float minDst = float.MaxValue;

        foreach (var hit in hits)
        {
            GrapplePoint gp = hit.GetComponent<GrapplePoint>();
            if (gp != null)
            {
                float dst = Vector2.Distance(transform.position, hit.transform.position);
                // "Menzil" kontrolü: Noktanın kendi range'i içinde miyiz?
                if (dst <= gp.range && dst < minDst)
                {
                    minDst = dst;
                    nearest = gp;
                }
            }
        }

        if (nearest != null)
        {
            Attach(nearest.transform.position);
        }
    }

    void Attach(Vector2 targetPos)
    {
        isAttached = true;
        currentAnchor = targetPos;

        // Joint Ayarları
        joint.connectedAnchor = targetPos;
        joint.distance = Vector2.Distance(transform.position, targetPos);
        joint.enabled = true;

        // Karakter Hareketini Devre Dışı Bırak (Fizikodurumunu koru)
        movement.SetSwinging(true);

        if (ropeRenderer != null) ropeRenderer.enabled = true;
    }

    void Detach()
    {
        if (!isAttached) return;

        isAttached = false;
        joint.enabled = false;
        movement.SetSwinging(false);
        if (ropeRenderer != null) ropeRenderer.enabled = false;
        
        // Fırlatma/Çıkış anında ekstra zıplama şansı verilebilir
        // movement.disableGroundSnapping = true;
    }
}
