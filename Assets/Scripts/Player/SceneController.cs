using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]

public class SceneController: MonoBehaviour{
    public Transform teleportLocationA;
    public Transform teleportLocationB;

    [Tooltip("Tag used by colliders that should teleport to location A")]
    public string teleportTagA = "TeleportA";

    [Tooltip("Tag used by colliders that should teleport to location B")]
    public string teleportTagB = "TeleportB";

    CharacterController _characterController;

    void Awake(){
        _characterController = GetComponent<CharacterController>();
    }

    void OnTriggerEnter(Collider other){
        HandleTeleportCollision(other.gameObject);
    }

    void OnControllerColliderHit(ControllerColliderHit hit){
        HandleTeleportCollision(hit.collider.gameObject);
    }

    void HandleTeleportCollision(GameObject hitObject){
        if (hitObject.CompareTag(teleportTagA) && teleportLocationA != null){
            TeleportTo(teleportLocationA);
            return;
        }
        if (hitObject.CompareTag(teleportTagB) && teleportLocationB != null){
            TeleportTo(teleportLocationB);
            return;
        }
    }

    void TeleportTo(Transform target){
        if (target == null){
            return;
        }
        bool wasEnabled = _characterController.enabled;
        if (wasEnabled){
            _characterController.enabled = false;
        }
        transform.position = target.position;
        transform.rotation = target.rotation;
        if (wasEnabled){
            _characterController.enabled = true;
        }
    }
}


