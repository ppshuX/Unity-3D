using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerShooting : NetworkBehaviour
{
    [SerializeField]
    private PlayerWeapon weapon;
    [SerializeField]
    private LayerMask mask;

    private Camera cam;

    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponentInChildren<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        RaycastHit hit;
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, weapon.range, mask))
        {
            ShootServerRpc(hit.collider.name);
        }
    }

    [ServerRpc]
    private void ShootServerRpc(string hittedName)
    {
        GameManager.UpdateInfo(transform.name + " hit " + hittedName);
    }
}