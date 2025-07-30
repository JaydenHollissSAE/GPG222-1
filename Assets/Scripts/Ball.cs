using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
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
    [SerializeField] GameObject ending;

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
                if (!awaitChangeLocal)
                {
                    awaitChangeLocal = true;
                    ResetOnOut();
                }
            }
        }

    }


    void CheckBounds()
    {
        if (transform.position.x > 30f || transform.position.x < -30f || transform.position.y > 30f || transform.position.y < -30f)
        {
            if (!awaitChangeLocal)
            {
                awaitChangeLocal = true;
                ResetOnOut();
            }
        }
        return;
    }



    private void ResetOnOut()
    {
        List<Draw> players = GameManager.instance.drawList;
        if (!GameManager.instance.endlessActive)
        {
            int index = GameManager.instance.coloursList.IndexOf(colourIndex);
            players[index].GetComponent<NetworkObject>().Despawn();
            GameManager.instance.drawList.RemoveAt(index);
            GameManager.instance.coloursList.RemoveAt(index);
            GameManager.instance.NewList();
            transform.position = Vector2.zero;
            players = GameManager.instance.drawList;
            if (players.Count > 1)
            {
                StartCoroutine(AwaitedChange(true));
            }
            else
            {
                StartCoroutine(EndingSequence());
                //GetComponent<NetworkObject>().Despawn();
            }
        }
        else
        {
            StartCoroutine(AwaitedChange(true));
        }


    }


    IEnumerator EndingSequence()
    {
        while (ending.transform.localScale.y < 1)
        {
            ending.transform.localScale = Vector2.MoveTowards(transform.position, Vector2.one, Time.deltaTime);
            yield return null;
        }
    }




    [Rpc(SendTo.Everyone, RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
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
        //moveDirection *= -1f;

        float tweak = 1f;
        if (transform.position.y < (transform.position.y + moveDirection.y))
        {
            tweak = -1f;
        }
        moveDirection = new Vector3(Random.Range(-1f,1f), Random.Range(-0f, 1f)*tweak, 0);
        return;
    }


    IEnumerator AwaitedChange(bool reset = false) 
    {
        yield return new WaitForSeconds(0.1f);
        if (!reset) speed.Value += speedDefault.Value /3 + speed.Value / 5;
        else speed.Value = speedDefault.Value;
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
        if (IsServer) CheckBounds();
        if (IsServer) transform.position = Vector3.MoveTowards(transform.position, transform.position+moveDirection, Time.deltaTime * speed.Value);
    }

}
