using UnityEngine;

public static class Soundproofing
{
    private const string LayerName = "Soundproof";
    private static int mask = -1;
    public static int Mask => mask >= 0 ? mask : mask = LayerMask.GetMask(LayerName);

    public static bool IsBlocked(Vector3 from, Vector3 to)
    {
        bool blocked = Physics.Linecast(from, to, Mask, QueryTriggerInteraction.Ignore);

#if UNITY_EDITOR
        Debug.DrawLine(from, to, blocked ? Color.red : Color.green, 1f);
#endif

        return blocked;
    }
}