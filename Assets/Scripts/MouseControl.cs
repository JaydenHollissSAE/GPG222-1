using Unity.Netcode;
using UnityEngine;

public class MouseControl : NetworkBehaviour
{
    Camera m_camera;
    SpriteRenderer sr;


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Cursor.visible = false;
        m_camera = Camera.main;
        sr = GetComponent<SpriteRenderer>();
    }


    public void SetMouseColour(Color colour)
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        sr.color = colour;
    }

    private void Update()
    {
        if ((IsLocalPlayer))
        {
            if (m_camera == null) m_camera = Camera.main;
            Vector2 mousePos = m_camera.ScreenToWorldPoint(Input.mousePosition);
            transform.position = mousePos;
        }

    }


}
