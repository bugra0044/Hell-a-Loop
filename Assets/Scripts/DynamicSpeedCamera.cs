using UnityEngine;
using Unity.Cinemachine;

public class DynamicSpeedCamera : MonoBehaviour
{
    [Header("Referanslar")]
    public CharacterMovement playerMovement;

    private CinemachineCamera vcam;
    private CinemachinePositionComposer composer;
    private CinemachineBasicMultiChannelPerlin noise; // YENÝ: Titreme motoru

    [Header("Zoom Ayarlarý")]
    public float minZoom = 5f;
    public float maxZoom = 9f;
    public float zoomSpeed = 2f;

    [Header("Ýleri Bakýþ Ayarlarý")]
    public float minLookAhead = 0f;
    public float maxLookAhead = 3f;

    [Header("Sarsýntý (Shake) Ayarlarý")]
    public float phase4Shake = 1.5f; // Mach 4'te orta þiddet sarsýntý
    public float phase5Shake = 3.5f; // Mach 5'te (Jordan Air) þiddetli sarsýntý

    void Start()
    {
        vcam = GetComponent<CinemachineCamera>();
        composer = GetComponent<CinemachinePositionComposer>();
        // Titreme motorunu bul
        noise = GetComponent<CinemachineBasicMultiChannelPerlin>();
    }

    void Update()
    {
        if (playerMovement == null || vcam == null || composer == null) return;

        int phase = playerMovement.currentPhase;

        // 1. DÝNAMÝK ZOOM
        float targetZoom = Mathf.Lerp(minZoom, maxZoom, (float)(phase - 1) / 4f);
        var lens = vcam.Lens;
        lens.OrthographicSize = Mathf.Lerp(lens.OrthographicSize, targetZoom, Time.deltaTime * zoomSpeed);
        vcam.Lens = lens;

        // 2. ÝLERÝ BAKIÞ
        float lookDir = Mathf.Sign(playerMovement.transform.localScale.x);
        float targetLookAhead = Mathf.Lerp(minLookAhead, maxLookAhead, (float)(phase - 1) / 4f);
        Vector3 targetOffset = new Vector3(targetLookAhead * lookDir, composer.TargetOffset.y, 0);
        composer.TargetOffset = Vector3.Lerp(composer.TargetOffset, targetOffset, Time.deltaTime * zoomSpeed);

        // 3. YENÝ: HIZA GÖRE SARSINTI (SHAKE) YÖNETÝMÝ
        if (noise != null)
        {
            float targetShake = 0f; // Varsayýlan: Titreme yok

            if (phase == 4) targetShake = phase4Shake;
            else if (phase == 5) targetShake = phase5Shake;

            // Sarsýntýyý aniden deðil, hafif yumuþak bir geçiþle baþlat/bitir
            noise.AmplitudeGain = Mathf.Lerp(noise.AmplitudeGain, targetShake, Time.deltaTime * 5f);
        }
    }
}