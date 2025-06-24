using Unity.Netcode;
using UnityEditor.Build.Content;
using UnityEngine;
using UnityEngine.EventSystems;

public class Ball : NetworkBehaviour
{
    private Vector3 moveDirection = new Vector3(1, 0.5f, 0);
    //public float speed = 1f;
    public NetworkVariable<float> speed = new NetworkVariable<float>();
    //[SerializeField] float speedDefault = 1f;
    [SerializeField] float speedDefaultOG = 1f;
    public NetworkVariable<float> speedDefault = new NetworkVariable<float>();
    public Color currentColour;
    private SpriteRenderer spriteRenderer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        spriteRenderer = GetComponent<SpriteRenderer>();

        speedDefault.Value = speedDefaultOG;
        speed.Value = speedDefault.Value;
        currentColour = spriteRenderer.color;
        SetColour();


    }






    private void OnTriggerEnter(Collider collision)
    {
        Debug.Log("Hit Something");
        if (collision.gameObject.tag == "Drawing")
        {
            Debug.Log("Hit Drawing");
            if (currentColour == Color.white || collision.gameObject.GetComponent<LineRenderer>().startColor == currentColour)
            {
                Debug.Log("Trigger switch");
                if (IsServer) speed.Value += speedDefault.Value + speed.Value / 2;
                SetColour();
                NewDirection();
            }

        }
    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    void DestroyLine_Rpc(NetworkObjectReference line)
    {

    }

    void SetColour()
    {

        Color tmpColour = currentColour;
        while(tmpColour == currentColour) currentColour = GameManager.instance.drawingColours[GameManager.instance.coloursList[Random.Range(0, GameManager.instance.coloursList.Count)]];
        spriteRenderer.color = currentColour;
    }

    void NewDirection()
    {
        moveDirection *= -1f;
    }

    private void Update()
    {
        if (IsServer) transform.position = Vector3.MoveTowards(transform.position, transform.position+moveDirection, Time.deltaTime * speed.Value);
    }

}
