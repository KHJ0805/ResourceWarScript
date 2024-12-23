using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;
using Protocol;
using System;
using System.Threading.Tasks;
using Google.Protobuf;
using Unity.VisualScripting.Antlr3.Runtime;
using System.Text;

public enum PacketType : byte
{
    // ���� ���� ���� ������
    GAME_STATE = 1,
    ITEM_DATA = 2,
    PLAYER_ROOM_INFO = 3,
    PLAYER_INITIAL_DATA = 4,
    PLAYER_STATE = 5,
    FIELD_UNIT = 6,
    POSITION = 7,

    // ȸ������
    SIGN_UP_REQUEST = 8,
    SIGN_UP_RESPONSE = 9,

    // �α���
    SIGN_IN_REQUEST = 10,
    SIGN_IN_RESPONSE = 11,

    // �α׾ƿ�
    SIGN_OUT_REQUEST = 56,
    SIGN_OUT_RESPONSE = 57,

    // ��ū ��߱�
    REFRESH_TOKEN_REQUEST = 12,
    REFRESH_TOKEN_RESPONSE = 13,

    // ���� ���� ��û �� ����
    CREATE_ROOM_REQUEST = 14,
    CREATE_ROOM_RESPONSE = 15,
    MATCH_START_REQUEST = 58, // ��Ī ��û
    MATCH_START_RESPONSE = 59, // ��Ī ���� ���� �˸�
    MATCH_CANCEL_REQUEST = 16,
    MATCH_CANCEL_RESPONSE = 60,
    MATCH_PROGRESS_NOTIFICATION = 17, // ��Ī ���� ���� �˸�

    // ���� ���� �� ������
    JOIN_ROOM_REQUEST = 18,
    JOIN_ROOM_RESPONSE = 19,
}

//�̱������� ����ϼ���
public class SessionNetworkManager : MonoBehaviour
{
    [Header("NetWork")]
    //��Ʈ��ũ �κ� : packet�� ���� ����, data ���
    private TcpClient tcpClient;
    private NetworkStream stream;

    private byte[] receiveBuffer = new byte[4096];
    private List<byte> incompleteData = new List<byte>();

    public event Action OnConnectionSuccess;
    public event Action OnConnectionFailure;

    public const int PACKET_TYPE_LENGTH = 2;    // 2 bytes for packet type
    public const int PACKET_TOKEN_LENGTH = 1;   // 1 byte for token length
    public const int PACKET_PAYLOAD_LENGTH = 4; // 4 bytes for payload length

    private string token => DataManager.Instance.Token;

    public static SessionNetworkManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    //���� ���� ����
    //IP : �ּ� : 
    //Port :
    public void StartConnect(string ip, string port)
    {
        int portNumber = int.Parse(port);

        if (ConnectToServer(ip, portNumber))
        {
            OnConnectionSuccess?.Invoke();
            //���� ����
            InitGame();
        }
        else
        {
            //���� ���� : ���ͳ� ���� �˾� ��
            OnConnectionFailure?.Invoke();
        }
    }

    //1. ���� ���� �õ�
    bool ConnectToServer(string ip, int port)
    {
        try
        {
            tcpClient = new TcpClient(ip, port);
            stream = tcpClient.GetStream();
            Debug.Log($"Connected to {ip}:{port}");
            return true;
        }
        catch (SocketException e)
        {
            Debug.LogError($"SocketException: {e}");
            return false;
        }
    }


    //2. ���� ���� �õ� �� ������ ���� ������ �ϸ� ���� ���� ������ ���� ��Ŷ �ޱ� �غ� Open
    void InitGame()
    {
        StartReceiving();
    }

    //2-1 :  ������ ���� ��Ŷ �ޱ� Open
    void StartReceiving() => _ = ReceivePacketsAsync();

    //2-1-1 : ������ ���� ��Ŷ �ޱ� �Լ�
    async Task ReceivePacketsAsync()
    {
        while (tcpClient.Connected)
        {
            try
            {
                int bytesRead = await stream.ReadAsync(receiveBuffer, 0, receiveBuffer.Length);
                if (bytesRead > 0)
                {
                    Debug.Log($"Received {bytesRead} bytes from server.");
                    ProcessReceivedData(receiveBuffer, bytesRead);
                }
                else
                {
                    Debug.LogWarning("Disconnected from server.");
                    break;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Receive error: {e.Message}");
                await Task.Delay(1000);
            }
        }
    }

    //2-1-2 : ������ ���� ��Ŷ �ޱ� open ���ķ� ������ Ŭ���̾�Ʈ���� ��Ŷ�� �������ָ�
    //�̰����� �޾Ƽ� ��Ŷ�� �ؼ��� �� ���� ó�� 
    void ProcessReceivedData(byte[] data, int length)
    {
        Debug.Log($"Received {length} bytes. Current incompleteData size: {incompleteData.Count}");
        incompleteData.AddRange(data.AsSpan(0, length).ToArray());

        Debug.Log($"Added {length} bytes to incompleteData. New size: {incompleteData.Count}");

        while (true)
        {
            // 1. �ּ� ��Ŷ ���� Ȯ�� (��Ŷ Ÿ�� 2 + ��ū ���� 1 + ���̷ε� ���� 4 = �ּ� 7����Ʈ)
            if (incompleteData.Count < 7)
            {
                Debug.Log("��Ŷ ���̰� ª�ƿ�");
                break;
            }

            // 2. ��Ŷ Ÿ�� �б� (2����Ʈ)
            byte[] typeBytes = incompleteData.GetRange(0, 2).ToArray();
            if (BitConverter.IsLittleEndian) Array.Reverse(typeBytes);
            ushort packetType = BitConverter.ToUInt16(typeBytes, 0);

            // 3. ��ū ���� �б� (1����Ʈ)
            byte tokenLength = incompleteData[2];

            // 4. ��ü ��Ŷ ���� ���
            int headerSize = 2 + 1 + 4; // ��Ŷ Ÿ��(2) + ��ū ����(1) + ���̷ε� ����(4)
            int requiredSize = headerSize + tokenLength;

            if (incompleteData.Count < requiredSize)
            {
                Debug.Log($"Incomplete token data. Need {requiredSize}, Received {incompleteData.Count}");
                break;
            }

            // 5. ��ū �б�
            byte[] tokenBytes = incompleteData.GetRange(3, tokenLength).ToArray();
            string token = Encoding.UTF8.GetString(tokenBytes);

            // 6. ���̷ε� ���� �б� (4����Ʈ)
            byte[] payloadLengthBytes = incompleteData.GetRange(3 + tokenLength, 4).ToArray();
            if (BitConverter.IsLittleEndian) Array.Reverse(payloadLengthBytes);
            int payloadLength = BitConverter.ToInt32(payloadLengthBytes, 0);

            // 7. ��ü ��Ŷ ���� Ȯ��
            requiredSize += payloadLength;

            if (incompleteData.Count < requiredSize)
            {
                Debug.Log($"Incomplete payload data. Need {requiredSize}, Received {incompleteData.Count}");
                break;
            }

            // 8. ���̷ε� �б�
            byte[] payloadBytes = incompleteData.GetRange(3 + tokenLength + 4, payloadLength).ToArray();

            // 9. ��Ŷ ó��
            Debug.Log($"Received complete packet: Type={packetType}, Token={token}, PayloadLength={payloadLength}");
            try
            {
                switch ((PacketType)packetType)
                {
                    case PacketType.SIGN_UP_RESPONSE:
                        ReceiveSignUpResponse(payloadBytes);
                        break;
                    case PacketType.SIGN_IN_RESPONSE:
                        ReceiveSignInResponse(payloadBytes);
                        break;
                    case PacketType.REFRESH_TOKEN_RESPONSE:
                        ReceiveRefreshTokenResponse(payloadBytes);
                        break;

                    //�� ���� ���
                    case PacketType.CREATE_ROOM_RESPONSE:
                        ReceiveCreateRoomResponse(payloadBytes);
                        break;
                    //�� ��Ī �����Ȳ
                    case PacketType.MATCH_PROGRESS_NOTIFICATION:
                        RecieveMatchProgressNotification(payloadBytes);
                        break;
                    //�� ����
                    case PacketType.JOIN_ROOM_RESPONSE:
                        RecieveJoinRoomResponse(payloadBytes);
                        break;
                    case PacketType.MATCH_START_RESPONSE:
                        RecieveMatchStartResponse(payloadBytes);
                        break;
                    case PacketType.MATCH_CANCEL_RESPONSE:
                        RecieveMatchCancelResponse(payloadBytes);
                        break;
                    default:
                        Debug.LogWarning($"Unhandled packet type: {packetType}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error handling packet of type {packetType}: {ex.Message}");
            }

            // 10. ó���� ������ ����
            incompleteData.RemoveRange(0, requiredSize);
        }
    }

    

    //3 : ������ ��û�� ���� ������ ������ �Լ�
    public async Task SendPacketAsync<T>(T payload, PacketType packetType) where T : IMessage<T>
    {
        try
        {
            ushort packetTypeValue = (ushort)packetType;
            byte[] packetTypeBytes = BitConverter.GetBytes(packetTypeValue);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(packetTypeBytes);

            byte[] tokenBytes = Encoding.UTF8.GetBytes(token);
            byte tokenLength = (byte)tokenBytes.Length;
            Debug.Log($"{token}");

            if (tokenBytes.Length > 255)
            {
                Debug.LogError("Token length exceeds 255 bytes.");
                return;
            }

            byte[] payloadBytes = payload.ToByteArray();
            int payloadLength = payloadBytes.Length;

            byte[] payloadLengthBytes = BitConverter.GetBytes(payloadLength);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(payloadLengthBytes);

            // 5. ���� ��Ŷ ũ�� ���
            int totalPacketSize = PACKET_TYPE_LENGTH + PACKET_TOKEN_LENGTH + tokenBytes.Length + PACKET_PAYLOAD_LENGTH + payloadBytes.Length;

            // 6. ��Ŷ �迭 ����
            byte[] packet = new byte[totalPacketSize];
            int offset = 0;

            // 7. ��Ŷ Ÿ�� ����
            Array.Copy(packetTypeBytes, 0, packet, offset, PACKET_TYPE_LENGTH);
            offset += PACKET_TYPE_LENGTH;

            // 8. ��ū ���� ����
            packet[offset] = tokenLength;
            offset += PACKET_TOKEN_LENGTH;

            // 9. ��ū ����
            Array.Copy(tokenBytes, 0, packet, offset, tokenBytes.Length);
            offset += tokenBytes.Length;

            // 10. ���̷ε� ���� ����
            Array.Copy(payloadLengthBytes, 0, packet, offset, PACKET_PAYLOAD_LENGTH);
            offset += PACKET_PAYLOAD_LENGTH;

            // 11. ���̷ε� ����
            Array.Copy(payloadBytes, 0, packet, offset, payloadBytes.Length);
            offset += payloadBytes.Length;

            Debug.Log($"Sending packet: Type={packetType}, TokenLength={tokenLength}, PayloadLength={payloadLength}, TotalSize={totalPacketSize}");

            // 13. ��Ŷ ����
            await stream.WriteAsync(packet, 0, packet.Length);
            await stream.FlushAsync();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to send packet: {ex.Message}");
            // �߰����� ���� �ڵ鸵 ������ ���⿡ ������ �� �ֽ��ϴ�.
        }
    }
    // ��Ŷ �ۼ��� ���!
    // ��Ŷ ������ �� SeesionNetworkManager.Instance.�޼�������ϼ��� �� ȣ��
    //public async void �޼�������ϼ���(�ڷ��� ����)
    //{
    //    // ��Ŷ �����
    //    var ��Ŷ�̸� = new ��Ŷ���� �ִ� �޼�����
    //    {
    //        ��Ŷ���� �ִ� ���� = ���� ������
    //    };

    //    PacketType packetType = PacketType.���� enum���� EEEE_EEEE_REQUEST;

    //    await SendPacketAsync(��Ŷ�̸�, packetType);
    //}

    //// ��Ŷ �����Ҷ�! 282������ ��Ŷ �޴� �޼���� ��ŶŸ�Ա����ؼ� �������� ����)
    //private void �޼�������ϼ���(byte[] payload)
    //{
    //    try
    //    {
    //        ��Ŷ�����޴¸޼��� ���ڵ��Ѻ����� = ��Ŷ�����޴¸޼���.Parser.ParseFrom(payload);
    //        ���ڵ��Ѻ�����.��Ŷ�����ڵ尡 == 0�̸� ���� �� �̷�������
    //        // ��� �ڵ� Ȯ��
    //        if (refreshTokenResponse.RefreshTokenResultCode == 0)
    //        {
    //            // ���� ó��
    //            string newToken = refreshTokenResponse.Token;
    //            ulong newExpirationTime = refreshTokenResponse.ExpirationTime;
    //            DataManager.Instance.RefreshToken(newToken, newExpirationTime);
    //        }
    //        else
    //        {
    //            // ���� - �ٽ� ��߱�?
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        Debug.LogError($"Fail {ex.Message}");
    //    }
    //}

    //ȸ������ ��û
    public async void SendSignUpRequest(string nickname, string password, string id)
    {
        var signUpReq = new C2SSignUpReq
        {
            Nickname = nickname,
            Id = id,
            Password = password
        };

        PacketType packetType = PacketType.SIGN_UP_REQUEST;

        await SendPacketAsync(signUpReq, packetType);
    }

    //ȸ������ ����
    private void ReceiveSignUpResponse(byte[] payload)
    {
        try
        {
            // ���̷ε带 Protocol Buffers �޽����� ���ڵ�
            S2CSignUpRes signUpResponse = S2CSignUpRes.Parser.ParseFrom(payload);

            // ��� �ڵ� Ȯ��
            if (signUpResponse.SignUpResultCode == 0)
            {
                // ����
                SessionUiManager.Instance.SignUpSuccess();

            }
            else
            {
                // ���� - ���� �ڵ忡 ���� ó��
                Debug.LogError($"Sign up failed with code: {signUpResponse.SignUpResultCode}");
                // ��: ����ڿ��� ���� �޽��� ǥ��
                SessionUiManager.Instance.SignUpFail();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to decode S2CSignUpRes: {ex.Message}");
        }
    }

    //�α��� ��û
    public async void SendSignInRequest(string id, string password)
    {
        var signInReq = new C2SSignInReq
        {
            Id = id,
            Password = password
        };

        PacketType packetType = PacketType.SIGN_IN_REQUEST;

        await SendPacketAsync(signInReq, packetType);
        Debug.Log("������ ��Ŷ ����, �α��� ��û");
    }

    //�α��� ����
    private void ReceiveSignInResponse(byte[] payload)
    {
        try
        {
            // ���̷ε带 Protocol Buffers �޽����� ���ڵ�
            S2CSignInRes signInResponse = S2CSignInRes.Parser.ParseFrom(payload);
            // ��� �ڵ� Ȯ��
            if (signInResponse.SignInResultCode == 0)
            {
                // ����
                SessionUiManager.Instance.SignInSuccess();

                Debug.Log("�α��� ����");
                //todo : �α��� ���� �Ŀ� (S2CSignInRes) �г��� �����޶�� �ϱ�
                string token = signInResponse.Token;
                ulong expirationTime = signInResponse.ExpirationTime;
                string userNickname = signInResponse.Nickname;

                DataManager.Instance.SetUserData(token, expirationTime, userNickname);
                //DataManager.Instance.SetUserData(token, expirationTime, userNickname);
            }
            else
            {
                // ���� - ���� �ڵ忡 ���� ó��
                Debug.LogError($"Sign In failed with code: {signInResponse.SignInResultCode}");
                // ��: ����ڿ��� ���� �޽��� ǥ��
                SessionUiManager.Instance.SignInFail();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to decode S2CSignInRes: {ex.Message}");
        }
    }

    // ��ū ��߱� ��û
    public async void SendRefreshTokenRequest(string token)
    {
        var refreshTokenReq = new C2SRefreshTokenReq
        {

        };

        PacketType packetType = PacketType.REFRESH_TOKEN_REQUEST;

        await SendPacketAsync(refreshTokenReq, packetType);
    }

    // ��ū ��߱� ����
    private void ReceiveRefreshTokenResponse(byte[] payload)
    {
        try
        {
            // ���̷ε带 Protocol Buffers �޽����� ���ڵ�
            S2CRefreshTokenRes refreshTokenResponse = S2CRefreshTokenRes.Parser.ParseFrom(payload);

            // ��� �ڵ� Ȯ��
            if (refreshTokenResponse.RefreshTokenResultCode == 0)
            {
                // ���� ó��
                string newToken = refreshTokenResponse.Token;
                ulong newExpirationTime = refreshTokenResponse.ExpirationTime;
                DataManager.Instance.RefreshToken(newToken, newExpirationTime);
            }
            else
            {
                // ���� - �ٽ� ��߱�?
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Fail {ex.Message}");
        }
    }

    //����� ��û
    public async void SendCreateRoomReq(bool isPrivate)
    {
        var createRoomReq = new C2SCreateRoomReq
        {
            IsPrivate = isPrivate
        };

        PacketType packetType = PacketType.CREATE_ROOM_REQUEST;

        await SendPacketAsync(createRoomReq, packetType);
    }
    //����� ����
    private void ReceiveCreateRoomResponse(byte[] payload)
    {
        try
        {
            S2CCreateRoomRes createRoomResponse = S2CCreateRoomRes.Parser.ParseFrom(payload);

            string gameCode = createRoomResponse.GameCode;
            string gameUrl = createRoomResponse.GameUrl;
            SessionUiManager.Instance.GameCodeEnterRoom(gameCode);
            //���Ǽ��� ���� ����
            try
            {
                if (stream != null)
                {
                    stream.Close();
                    stream = null;
                }

                if (tcpClient != null)
                {
                    tcpClient.Close();
                    tcpClient = null;
                }

                Debug.Log("Disconnected from server.");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error while disconnecting: {e}");
            }

            //���Ӽ��� ���� ����
            string[] urlParts = gameUrl.Split(':'); // "ip�ּ�,��Ʈ��ȣ" �и�
            if (urlParts.Length == 2)
            {
                string ip = urlParts[0].Trim();
                string port = urlParts[1].Trim();

                Debug.Log($"{ip}�� �Ծ��");
                Debug.Log($"{port}�� �Ծ��");

                GameServerManager.Instance.isCreateRoomRequest = true;
                GameServerManager.Instance.StartConnect(ip, port);
            }
            else
            {
                Debug.LogError($"Invalid gameUrl format: {gameUrl}");
            }
            //���Ӽ����Ŵ������� ��ū ��ȿ���˻� �ǽ��Ұ�
            //���� ���� Ui�Ŵ������� code�޾Ƽ� �ڵ常 data�Ŵ����� ���� -> ����� Ui off, �� Ui on -> �� ui�� start���ڸ��� code�� �����͸Ŵ������� �޾ƿͼ� ����
            
        }
        catch (Exception ex)
        {
            Debug.LogError($"Fail {ex.Message}");
        }
    }
    //�׽�Ʈ�� ����
    public void TestSceneGameServerConnect()
    {
        //���� ���� ���� ����
        try
        {
            if (stream != null)
            {
                stream.Close();
                stream = null;
            }

            if (tcpClient != null)
            {
                tcpClient.Close();
                tcpClient = null;
            }

            Debug.Log("Disconnected from server.");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error while disconnecting: {e}");
        }
        //���Ӽ��� �����ϱ�
        GameServerManager.Instance.TestSceneConnect("211.51.160.19", "5556");
    }

    //�� ���� ��û
    public async void SendJoinRoomReq(string gameCode)
    {
        var joinRoomReq = new C2SJoinRoomReq
        {
            GameCode = gameCode,
        };

        PacketType packetType = PacketType.JOIN_ROOM_REQUEST;

        await SendPacketAsync(joinRoomReq, packetType);
    }

    //�� ���� ���� (������� ������ �������)
    private void RecieveJoinRoomResponse(byte[] payload)
    {
        try
        {
            S2CJoinRoomRes joinRoomRes = S2CJoinRoomRes.Parser.ParseFrom(payload);
            uint joinRoomResultCode = joinRoomRes.JoinRoomResultCode;
            string gameUrl = joinRoomRes.GameUrl;
            if(joinRoomResultCode == 0)
            {
                SessionUiManager.Instance.GameCodeRegistRoom(DataManager.Instance.GameCode);
                //���Ǽ��� ���� ����
                try
                {
                    if (stream != null)
                    {
                        stream.Close();
                        stream = null;
                    }

                    if (tcpClient != null)
                    {
                        tcpClient.Close();
                        tcpClient = null;
                    }

                    Debug.Log("Disconnected from server.");
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error while disconnecting: {e}");
                }

                //���Ӽ��� ���� ����
                string[] urlParts = gameUrl.Split(':'); // "ip�ּ�,��Ʈ��ȣ" �и�
                if (urlParts.Length == 2)
                {
                    string ip = urlParts[0].Trim();
                    string port = urlParts[1].Trim();

                    GameServerManager.Instance.isCreateRoomRequest = false;
                    GameServerManager.Instance.StartConnect(ip, port);
                }
                else
                {
                    Debug.LogError($"Invalid gameUrl format: {gameUrl}");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Fail {ex.Message}");
        }
    }

    //�ڵ� ��Ī ��� ��û
    public async void SendMatchCacelReq()
    {
        var matchCancelReq = new C2SMatchCancelReq
        {

        };

        PacketType packetType = PacketType.MATCH_CANCEL_REQUEST;

        await SendPacketAsync(matchCancelReq, packetType);
    }

    //�ڵ� ��Ī ��� ����
    private void RecieveMatchCancelResponse(byte[] payload)
    {
        try
        {
            S2CMatchCancelRes matchProgressNoti = S2CMatchCancelRes.Parser.ParseFrom(payload);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Fail {ex.Message}");
        }
    }
    //�ڵ� ��Ī ��û �߽�
    public async void SendMatchStartReq()
    {
        var matchReq = new C2SMatchReq
        {

        };

        PacketType packetType = PacketType.MATCH_START_REQUEST;

        await SendPacketAsync(matchReq, packetType);
    }
    //�ڵ� ��Ī ��û ����
    private void RecieveMatchStartResponse(byte[] payload)
    {
        try
        {
            S2CMatchRes matchProgressNoti = S2CMatchRes.Parser.ParseFrom(payload);
            if(matchProgressNoti.RoomMatchResultCode ==0 )
            {

            }

        }
        catch (Exception ex)
        {
            Debug.LogError($"Fail {ex.Message}");
        }
    }

    //�ڵ� ��Ī ���൵ ����
    private void RecieveMatchProgressNotification(byte[] payload)
    {
        try
        {
            S2CMatchProgressNoti matchProgressNoti = S2CMatchProgressNoti.Parser.ParseFrom(payload);
            uint matchProgressCode = matchProgressNoti.MatchProgressCode;
            ulong elapsedTime = matchProgressNoti.ElapsedTime;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Fail {ex.Message}");
        }
    }

}
