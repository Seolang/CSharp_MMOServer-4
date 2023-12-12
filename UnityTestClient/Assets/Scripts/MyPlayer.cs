using System.Collections;
using UnityEngine;

public class MyPlayer : Player
{
    NetworkManager _network;

    void Start()
    {
        _network = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
        StartCoroutine("CoSendPacket");
    }

    void Update()
    {
        
    }

    IEnumerator CoSendPacket()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.25f);

            C_Move movePacket = new C_Move()
            {
                posX = UnityEngine.Random.Range(-10, 10),
                posY = 0,
                posZ = UnityEngine.Random.Range(-10, 10),
            };
            _network.Send(movePacket.Write());
        }
    }
}
