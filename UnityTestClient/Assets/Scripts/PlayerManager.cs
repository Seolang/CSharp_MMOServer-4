using UnityEngine;
using System.Collections.Generic;
using static S_PlayerList;

public class PlayerManager
{
    MyPlayer _myPlayer;
    Dictionary<int, Player> _players = new Dictionary<int, Player>();

    public static PlayerManager Instance { get; } = new PlayerManager();

    public void InitiatePlayerList(S_PlayerList packet)
    {
        // Prefab 로드
        Object obj = Resources.Load("Player");

        foreach (S_PlayerList.Player p in packet.players)
        {
            // Player 오브젝트 생성
            GameObject go = Object.Instantiate(obj) as GameObject;

            // 내 플레이어인지 아닌지에 따라 스크립트 부착
            if (p.isSelf)
            {
                MyPlayer myPlayer = go.AddComponent<MyPlayer>();
                myPlayer.PlayerId = p.playerId;
                myPlayer.transform.position = new Vector3(p.posX, p.posY, p.posZ);
                _myPlayer = myPlayer;
            }
            else
            {
                Player player = go.AddComponent<Player>();
                player.PlayerId = p.playerId;
                player.transform.position = new Vector3(p.posX, p.posY, p.posZ);
                _players.Add(p.playerId, player);
            }
        }
    }

    public void MovePlayer(S_BroadcastMove packet)
    {
        // 이동을 동기화 하는 방법에는 여러가지가 있다
        // 1. 서버에서 허락 패킷이 왔을 때 실제로 이동
        // 2. 클라에서 먼저 이동시키고 서버에 이동 정보 전달
        // 지금은 2번 방법을 사용
        if (_myPlayer.PlayerId == packet.playerId)
        {
            _myPlayer.transform.position = new Vector3(packet.posX, packet.posY, packet.posZ);
        }
        else
        {
            Player player = null;
            if (_players.TryGetValue(packet.playerId, out player))
            {
                player.transform.position = new Vector3(packet.posX, packet.posY, packet.posZ);
            }
        }
    }

    public void EnterGame(S_BroadcastEnterGame packet)
    {
        if (packet.playerId == _myPlayer.PlayerId)
            return;

        // 새로 들어온 플레이어 오브젝트 생성
        Object obj = Resources.Load("Player");
        GameObject go = Object.Instantiate(obj) as GameObject;

        Player player = go.AddComponent<Player>();
        player.transform.position = new Vector3(packet.posX, packet.posY, packet.posZ);
        _players.Add(packet.playerId, player);
    }

    public void LeaveGame(S_BroadcastLeaveGame packet)
    {
        if (_myPlayer.PlayerId == packet.playerId)
        {
            GameObject.Destroy(_myPlayer.gameObject);
            _myPlayer = null;
        }
        else
        {
            Player player = null;
            if (_players.TryGetValue(packet.playerId, out player))
            {
                GameObject.Destroy(player.gameObject);
                _players.Remove(packet.playerId);
            }
        }
    }
}
