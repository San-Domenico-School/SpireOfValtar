using UnityEngine;

public class PlayerSpawnPoint : MonoBehaviour
{
    [SerializeField] private bool useTransformRotation = true;

    public Vector3 Position => transform.position;
    public Quaternion Rotation => useTransformRotation ? transform.rotation : Quaternion.identity;
}
