using System.Collections;
using System.Collections.Generic;
using ED.SC;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

public class AvatarController : NetworkBehaviour {
    [SerializeField] private float movementSpeed = 3.0f;
    [SerializeField] private GameObject doorPrefab;
    private readonly List<Color> m_Colors = new List<Color>() { Color.blue , Color.green, Color.red};
    private SpriteRenderer m_Renderer;
    private static NetworkVariable<Vector3> m_Position = new NetworkVariable<Vector3>(Vector3.zero, 
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<Color> m_Color = new NetworkVariable<Color>(Color.black, 
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkObject doorNetworkObj = null;

    // Start is called before the first frame update
    void Start()
    {
        SmartConsole.Log($"ClientId {OwnerClientId}");
        if (OwnerClientId > 2)
        {
            NetworkManager.Singleton.DisconnectClient(OwnerClientId);
            Destroy(gameObject);
            return;
        }
        // player id defines player position
        transform.position += Vector3.right * 2 * OwnerClientId;
        // get component for color change
        m_Renderer = GetComponent<SpriteRenderer>();
        // assign initial color to network variable
        if(IsOwner) m_Color.Value = m_Colors[(int) OwnerClientId];
        //ChangeColorServerRpc(m_Colors[(int) OwnerClientId]);
    }

    // Update is called once per frame
    void Update() {
        // sets color to network variable value
        if (m_Renderer.color != m_Color.Value) m_Renderer.color = m_Color.Value;

        // movement
        if (Input.GetKey(KeyCode.LeftArrow)) LerpPosition(Vector3.left, movementSpeed, OwnerClientId);
        if (Input.GetKey(KeyCode.RightArrow)) LerpPosition(Vector3.right, movementSpeed, OwnerClientId);
        if (Input.GetKey(KeyCode.UpArrow)) LerpPosition(Vector3.up, movementSpeed, OwnerClientId);
        if (Input.GetKey(KeyCode.DownArrow)) LerpPosition(Vector3.down, movementSpeed, OwnerClientId);

        if (Input.GetKey(KeyCode.A) && !(doorNetworkObj == null)) VisibilityServerRpc();

        if (Input.GetKey(KeyCode.S) && !(doorNetworkObj == null)) VisibilityHostServerRpc();

        // color change
        // if you need to change something not in transform, it's needed RPC calling
        // RPC calling executes the method in several clients (specified by RpcTarget)
        // if (Input.GetKeyDown(KeyCode.RightControl)) m_Color.Value = m_Colors[2];
        if (Input.GetKeyDown(KeyCode.RightControl)) SpawnDoorServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    void VisibilityHostServerRpc()
    {
        Debug.Log("Spawnea door: " + OwnerClientId);
        doorNetworkObj.NetworkShow(1);
        doorNetworkObj.NetworkShow(2);
    }

    [ServerRpc(RequireOwnership = false)]
    void VisibilityServerRpc()
    {
        Debug.Log("Spawnea door: " + OwnerClientId);
        doorNetworkObj.NetworkShow(OwnerClientId);
    }

    void LerpPosition(Vector3 offset, float speed, ulong OwnerClientId) {
        Vector3 positionNow = transform.position;
        transform.position = Vector3.Lerp(positionNow, positionNow +  offset, Time.deltaTime * speed);
        Debug.Log("Se mueve el cliente: "+OwnerClientId);
    }

    [ServerRpc(RequireOwnership = true)]
    void SpawnDoorServerRpc()
    {
        SmartConsole.Log("spawn door");
        GameObject door = Instantiate(doorPrefab, m_Position.Value, Quaternion.identity);
        m_Position.Value += Vector3.right + Vector3.up;
        doorNetworkObj = door.GetComponent<NetworkObject>();
        doorNetworkObj.SpawnWithObservers = false;
        doorNetworkObj.Spawn();
        Debug.Log("Door reference: "+this.doorNetworkObj);
    }
    
    // [ServerRpc(RequireOwnership = false)]
    // void ChangeColorServerRpc(Color newColor)
    // {
    //     ChangeColorClientRpc(newColor);
    // }
    //
    // [ClientRpc]
    // void ChangeColorClientRpc(Color newColor)
    // {
    //     SmartConsole.Log($"New color {newColor}");
    //     if(m_Renderer!=null) m_Renderer.color = newColor;
    // }
}
