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
    //public NetworkVariable<int> colourIndex = new NetworkVariable<int>();
    public int colourIndex;
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
        //SetColour();
        //StartCoroutine(TestLoop());


    }






    private void OnTriggerEnter(Collider collision)
    {
        if (IsServer)
        {
            Debug.Log("Hit Something");
            if (collision.gameObject.tag == "Drawing")
            {
                Debug.Log("Hit Drawing");
                if (currentColour == Color.white || collision.gameObject.GetComponent<DataStorage>().selectedColour == colourIndex)
                {
                    Debug.Log("Trigger switch");

                    if (!awaitChangeLocal)
                    {
                        awaitChangeLocal = true;
                        StartCoroutine(AwaitedChange());

                    }
                    collision.GetComponent<NetworkObject>().Despawn(true);
                    Destroy(collision.gameObject);
                    //awaitChange.Value = true;
                    //collision.GetComponent<NetworkObject>().Despawn();
                }

            }
            else if (collision.gameObject.tag == "Bounds")
            {
                // Elimination and end game
            }
        }

    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    void SetColour_Rpc(int colourIndex)
    {
        currentColour = GameManager.instance.drawingColours[colourIndex];
        spriteRenderer.color = currentColour;
    }

    void SetColour()
    {

        //Color tmpColour = currentColour;
        int tmpColour = colourIndex;
        while (true)
        {
            if (IsServer) colourIndex = GameManager.instance.coloursList[Random.Range(0, GameManager.instance.coloursList.Count)];
            if (tmpColour != colourIndex || GameManager.instance.coloursList.Count <= 1) break;
        }
        //currentColour = GameManager.instance.drawingColours[colourIndex];
        //spriteRenderer.color = currentColour;
        return;
    }

    void NewDirection()
    {
        moveDirection *= -1f;
        return;
    }

    IEnumerator TestLoop()
    {
        while (true)
        {
            yield return null;
            Debug.Log(Random.Range(0, GameManager.instance.coloursList.Count));
        }
    }

    IEnumerator AwaitedChange() 
    {
        yield return new WaitForSeconds(0.1f);
        if (IsServer) speed.Value += speedDefault.Value /3 + speed.Value / 5;
        SetColour();
        NewDirection();
        //awaitChange.Value = false;
        SetColour_Rpc(colourIndex);
        awaitChangeLocal = false;
    }
    

    private void Update()
    {
        //if (awaitChange.Value == true && !awaitChangeLocal)
        //{
        //    awaitChangeLocal = true;
        //    StartCoroutine(AwaitedChange());
        //}
        if (IsServer) transform.position = Vector3.MoveTowards(transform.position, transform.position+moveDirection, Time.deltaTime * speed.Value);
    }

}
