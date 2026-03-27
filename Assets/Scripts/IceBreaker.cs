using UnityEngine;

public class IceBreaker : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        BreakableIce ice = other.GetComponent<BreakableIce>();
        if (ice != null)
        {
            ice.Break();
        }
    }
}
