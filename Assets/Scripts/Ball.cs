using System.Collections;
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
    [SerializeField] float speedDefaultOG = 0.6f;
    public NetworkVariable<float> speedDefault = new NetworkVariable<float>();
    public NetworkVariable<int> colourIndex = new NetworkVariable<int>();
    public NetworkVariable<bool> awaitChange = new NetworkVariable<bool>();
    public Color currentColour;
    private SpriteRenderer spriteRenderer;
    [SerializeField] private bool awaitChangeLocal = false;

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
        if (IsServer)
        {
            Debug.Log("Hit Something");
            if (collision.gameObject.tag == "Drawing")
            {
                Debug.Log("Hit Drawing");
                if (currentColour == Color.white || collision.gameObject.GetComponent<LineRenderer>().startColor == currentColour)
                {
                    Debug.Log("Trigger switch");

                    if (awaitChange.Value == false) awaitChange.Value = true;

                }

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
        while (true)
        {
            if (IsServer) colourIndex.Value = Random.Range(0, GameManager.instance.coloursList.Count - 1);
            currentColour = GameManager.instance.drawingColours[GameManager.instance.coloursList[colourIndex.Value]];
            if (tmpColour != currentColour || GameManager.instance.coloursList.Count > 1) break;
        }
        spriteRenderer.color = currentColour;
        return;
    }

    void NewDirection()
    {
        moveDirection *= -1f;
        return;
    }
    IEnumerator AwaitedChange() 
    {
        yield return new WaitForSeconds(0.1f);
        if (IsServer) speed.Value += speedDefault.Value + speed.Value / 2;
        SetColour();
        NewDirection();
        awaitChange.Value = false;
        awaitChangeLocal = false;
    }
    

    private void Update()
    {
        if (awaitChange.Value == true && !awaitChangeLocal)
        {
            awaitChangeLocal = true;
            StartCoroutine(AwaitedChange());
        }
        if (IsServer) transform.position = Vector3.MoveTowards(transform.position, transform.position+moveDirection, Time.deltaTime * speed.Value);
    }

}
