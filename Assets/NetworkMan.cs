using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Net.Sockets;
using System.Net;

public class NetworkMan : MonoBehaviour
{
    public PlayerUnit playerPrefab;
    Dictionary<string, PlayerUnit> playerUnits = new Dictionary<string, PlayerUnit>();
    List<Player> newPlayers = new List<Player>();
    List<Player> disconnectedPlayers = new List<Player>();

    public UdpClient udp;
    public string serverIp = "18.222.93.164";
    public int serverPort = 12345;
    string clientId;
    // Start is called before the first frame update
    void Start()
    {
        udp = new UdpClient();

        udp.Connect( serverIp, serverPort );

        Byte[] sendBytes = Encoding.ASCII.GetBytes("{\'message\':\'connect\'}");
      
        udp.Send(sendBytes, sendBytes.Length);

        udp.BeginReceive(new AsyncCallback(OnReceived), udp);

        InvokeRepeating("HeartBeat", 1, 1);
    }

    void OnDestroy(){
        udp.Dispose();
    }


    public enum commands{
        SERVER_CONNECTED,
        NEW_CLIENT,
        UPDATE,
        CLIENT_DROPPED,
    };
    
    [Serializable]
    public class Message{
        public commands cmd;
    }

    [Serializable]
    public struct receivedColor
    {
        public float R;
        public float G;
        public float B;
    }

    [Serializable]
    public struct receivedPos
    {
        public float x;
        public float y;
        public float z;
    }

    [Serializable]
    public class Player{
        public string id;
        public receivedColor color;
        public receivedPos pos;
    }

    [Serializable]
    public class PlayerPacketData
    {
        public string id;
        public string message;
        public Vector3 pos;
    }

    [Serializable]
    public class NewPlayer{
        public commands cmd;
        public Player player;
    }

    [Serializable]
    public class GameState{
        public commands cmd;
        public Player[] players;
    }

    public Message latestMessage;
    public GameState lastestGameState;
    void OnReceived(IAsyncResult result){
        // this is what had been passed into BeginReceive as the second parameter:
        UdpClient socket = result.AsyncState as UdpClient;
        
        // points towards whoever had sent the message:
        IPEndPoint source = new IPEndPoint(0, 0);

        // get the actual message and fill out the source:
        byte[] message = socket.EndReceive(result, ref source);
        
        // do what you'd like with `message` here:
        string returnData = Encoding.ASCII.GetString(message);
        Debug.Log("Got this: " + returnData);
        
        latestMessage = JsonUtility.FromJson<Message>(returnData);
        try{
            switch(latestMessage.cmd){
                case commands.SERVER_CONNECTED:
                    {
                        NewPlayer newPlayer = JsonUtility.FromJson<NewPlayer>( returnData );
                        clientId = newPlayer.player.id;
                        newPlayers.Add( newPlayer.player );
                        break;
                    }
                case commands.NEW_CLIENT:
                    {
                        NewPlayer newPlayer = JsonUtility.FromJson<NewPlayer>( returnData );
                        newPlayers.Add( newPlayer.player );
                        break;
                    }
                case commands.UPDATE:
                    lastestGameState = JsonUtility.FromJson<GameState>( returnData );
                    break;
                case commands.CLIENT_DROPPED:
                    NewPlayer droppedPlayer = JsonUtility.FromJson<NewPlayer>( returnData );
                    disconnectedPlayers.Add( droppedPlayer.player );
                    break;
                default:
                    Debug.Log("Error");
                    break;
            }
        }
        catch (Exception e){
            Debug.Log(e.ToString());
        }
        
        // schedule the next receive operation once reading is done:
        socket.BeginReceive(new AsyncCallback(OnReceived), socket);
    }

    void SpawnPlayers(  ){
        if( newPlayers.Count > 0 )
        {
            foreach( var newPlayer in newPlayers )
            {
                PlayerUnit player = Instantiate( playerPrefab );
                player.transform.position = new Vector3( newPlayer.pos.x, newPlayer.pos.y, newPlayer.pos.z );
                player.id = newPlayer.id;
                playerUnits.Add( newPlayer.id, player );
            }
            newPlayers.Clear();
        }
    }

    void UpdatePlayers(){
        if( lastestGameState != null & lastestGameState.players.Length > 0 )
        {
            foreach( var player in lastestGameState.players )
            {
                if( playerUnits.ContainsKey( player.id ) )
                {
                    Color newColor = new Color( player.color.R, player.color.G, player.color.B );
                    playerUnits[player.id].transform.position = new Vector3(player.pos.x, player.pos.y, player.pos.z);
                    playerUnits[player.id].SetColor( newColor );
                }
            }
        }
    }

    void DestroyPlayers(){
        if( disconnectedPlayers.Count > 0 )
        {
            foreach( var droppedPlayer in disconnectedPlayers )
            {
                if( playerUnits.ContainsKey( droppedPlayer.id ) )
                {
                    Destroy( playerUnits[droppedPlayer.id].gameObject );
                    playerUnits.Remove( droppedPlayer.id );
                }
            }
            disconnectedPlayers.Clear();
        }
    }
    
    void HeartBeat(){

        if( clientId != null )
        {
            PlayerPacketData data = new PlayerPacketData();
            data.id = clientId;
            data.message = "heartbeat";
            data.pos = playerUnits[clientId].transform.position;
            string message = JsonUtility.ToJson( data );
            Byte[] sendBytes = Encoding.ASCII.GetBytes( message );
            udp.Send( sendBytes, sendBytes.Length );
        }
    }

    void Update(){
        SpawnPlayers();
        UpdatePlayers();
        DestroyPlayers();
    }
}