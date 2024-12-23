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
    // 게임 상태 관련 데이터
    GAME_STATE = 1,
    ITEM_DATA = 2,
    PLAYER_ROOM_INFO = 3,
    PLAYER_INITIAL_DATA = 4,
    PLAYER_STATE = 5,
    FIELD_UNIT = 6,
    POSITION = 7,

    // 회원가입
    SIGN_UP_REQUEST = 8,
    SIGN_UP_RESPONSE = 9,

    // 로그인
    SIGN_IN_REQUEST = 10,
    SIGN_IN_RESPONSE = 11,

    // 로그아웃
    SIGN_OUT_REQUEST = 56,
    SIGN_OUT_RESPONSE = 57,

    // 토큰 재발급
    REFRESH_TOKEN_REQUEST = 12,
    REFRESH_TOKEN_RESPONSE = 13,

    // 대기실 관련 요청 및 응답
    CREATE_ROOM_REQUEST = 14,
    CREATE_ROOM_RESPONSE = 15,
    MATCH_START_REQUEST = 58, // 매칭 신청
    MATCH_START_RESPONSE = 59, // 매칭 시작 여부 알림
    MATCH_CANCEL_REQUEST = 16,
    MATCH_CANCEL_RESPONSE = 60,
    MATCH_PROGRESS_NOTIFICATION = 17, // 매칭 진행 여부 알림

    // 대기실 입장 및 나가기
    JOIN_ROOM_REQUEST = 18,
    JOIN_ROOM_RESPONSE = 19,
}

//싱글톤으로 사용하세요
public class SessionNetworkManager : MonoBehaviour
{
    [Header("NetWork")]
    //네트워크 부분 : packet을 읽을 버퍼, data 등등
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

    //세션 서버 정보
    //IP : 주소 : 
    //Port :
    public void StartConnect(string ip, string port)
    {
        int portNumber = int.Parse(port);

        if (ConnectToServer(ip, portNumber))
        {
            OnConnectionSuccess?.Invoke();
            //접속 성공
            InitGame();
        }
        else
        {
            //접속 실패 : 인터넷 에러 팝업 등
            OnConnectionFailure?.Invoke();
        }
    }

    //1. 서버 접속 시도
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


    //2. 서버 접속 시도 후 서버와 연결 성공을 하면 상태 유지 서버로 부터 패킷 받기 준비 Open
    void InitGame()
    {
        StartReceiving();
    }

    //2-1 :  서버로 부터 패킷 받기 Open
    void StartReceiving() => _ = ReceivePacketsAsync();

    //2-1-1 : 서버로 부터 패킷 받기 함수
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

    //2-1-2 : 서버로 부터 패킷 받기 open 이후로 서버가 클라이언트에게 패킷을 전달해주면
    //이곳에서 받아서 패킷을 해석한 후 동작 처리 
    void ProcessReceivedData(byte[] data, int length)
    {
        Debug.Log($"Received {length} bytes. Current incompleteData size: {incompleteData.Count}");
        incompleteData.AddRange(data.AsSpan(0, length).ToArray());

        Debug.Log($"Added {length} bytes to incompleteData. New size: {incompleteData.Count}");

        while (true)
        {
            // 1. 최소 패킷 길이 확인 (패킷 타입 2 + 토큰 길이 1 + 페이로드 길이 4 = 최소 7바이트)
            if (incompleteData.Count < 7)
            {
                Debug.Log("패킷 길이가 짧아요");
                break;
            }

            // 2. 패킷 타입 읽기 (2바이트)
            byte[] typeBytes = incompleteData.GetRange(0, 2).ToArray();
            if (BitConverter.IsLittleEndian) Array.Reverse(typeBytes);
            ushort packetType = BitConverter.ToUInt16(typeBytes, 0);

            // 3. 토큰 길이 읽기 (1바이트)
            byte tokenLength = incompleteData[2];

            // 4. 전체 패킷 길이 계산
            int headerSize = 2 + 1 + 4; // 패킷 타입(2) + 토큰 길이(1) + 페이로드 길이(4)
            int requiredSize = headerSize + tokenLength;

            if (incompleteData.Count < requiredSize)
            {
                Debug.Log($"Incomplete token data. Need {requiredSize}, Received {incompleteData.Count}");
                break;
            }

            // 5. 토큰 읽기
            byte[] tokenBytes = incompleteData.GetRange(3, tokenLength).ToArray();
            string token = Encoding.UTF8.GetString(tokenBytes);

            // 6. 페이로드 길이 읽기 (4바이트)
            byte[] payloadLengthBytes = incompleteData.GetRange(3 + tokenLength, 4).ToArray();
            if (BitConverter.IsLittleEndian) Array.Reverse(payloadLengthBytes);
            int payloadLength = BitConverter.ToInt32(payloadLengthBytes, 0);

            // 7. 전체 패킷 길이 확인
            requiredSize += payloadLength;

            if (incompleteData.Count < requiredSize)
            {
                Debug.Log($"Incomplete payload data. Need {requiredSize}, Received {incompleteData.Count}");
                break;
            }

            // 8. 페이로드 읽기
            byte[] payloadBytes = incompleteData.GetRange(3 + tokenLength + 4, payloadLength).ToArray();

            // 9. 패킷 처리
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

                    //방 생성 결과
                    case PacketType.CREATE_ROOM_RESPONSE:
                        ReceiveCreateRoomResponse(payloadBytes);
                        break;
                    //방 매칭 진행상황
                    case PacketType.MATCH_PROGRESS_NOTIFICATION:
                        RecieveMatchProgressNotification(payloadBytes);
                        break;
                    //방 입장
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

            // 10. 처리된 데이터 제거
            incompleteData.RemoveRange(0, requiredSize);
        }
    }

    

    //3 : 나만의 요청을 만들어서 서버에 보내는 함수
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

            // 5. 최종 패킷 크기 계산
            int totalPacketSize = PACKET_TYPE_LENGTH + PACKET_TOKEN_LENGTH + tokenBytes.Length + PACKET_PAYLOAD_LENGTH + payloadBytes.Length;

            // 6. 패킷 배열 생성
            byte[] packet = new byte[totalPacketSize];
            int offset = 0;

            // 7. 패킷 타입 복사
            Array.Copy(packetTypeBytes, 0, packet, offset, PACKET_TYPE_LENGTH);
            offset += PACKET_TYPE_LENGTH;

            // 8. 토큰 길이 복사
            packet[offset] = tokenLength;
            offset += PACKET_TOKEN_LENGTH;

            // 9. 토큰 복사
            Array.Copy(tokenBytes, 0, packet, offset, tokenBytes.Length);
            offset += tokenBytes.Length;

            // 10. 페이로드 길이 복사
            Array.Copy(payloadLengthBytes, 0, packet, offset, PACKET_PAYLOAD_LENGTH);
            offset += PACKET_PAYLOAD_LENGTH;

            // 11. 페이로드 복사
            Array.Copy(payloadBytes, 0, packet, offset, payloadBytes.Length);
            offset += payloadBytes.Length;

            Debug.Log($"Sending packet: Type={packetType}, TokenLength={tokenLength}, PayloadLength={payloadLength}, TotalSize={totalPacketSize}");

            // 13. 패킷 전송
            await stream.WriteAsync(packet, 0, packet.Length);
            await stream.FlushAsync();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to send packet: {ex.Message}");
            // 추가적인 에러 핸들링 로직을 여기에 구현할 수 있습니다.
        }
    }
    // 패킷 송수신 방법!
    // 패킷 전송할 때 SeesionNetworkManager.Instance.메서드명정하세요 로 호출
    //public async void 메서드명정하세요(자료형 변수)
    //{
    //    // 패킷 만들기
    //    var 패킷이름 = new 패킷명세에 있는 메세지명
    //    {
    //        패킷명세에 있는 변수 = 받은 변수명
    //    };

    //    PacketType packetType = PacketType.맨위 enum에서 EEEE_EEEE_REQUEST;

    //    await SendPacketAsync(패킷이름, packetType);
    //}

    //// 패킷 수신할때! 282번줄이 패킷 받는 메서드고 패킷타입구분해서 이쪽으로 연결)
    //private void 메서드명정하세요(byte[] payload)
    //{
    //    try
    //    {
    //        패킷명세에받는메세지 디코딩한변수명 = 패킷명세에받는메세지.Parser.ParseFrom(payload);
    //        디코딩한변수명.패킷명세에코드가 == 0이면 성공 뭐 이런식으로
    //        // 결과 코드 확인
    //        if (refreshTokenResponse.RefreshTokenResultCode == 0)
    //        {
    //            // 성공 처리
    //            string newToken = refreshTokenResponse.Token;
    //            ulong newExpirationTime = refreshTokenResponse.ExpirationTime;
    //            DataManager.Instance.RefreshToken(newToken, newExpirationTime);
    //        }
    //        else
    //        {
    //            // 실패 - 다시 재발급?
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        Debug.LogError($"Fail {ex.Message}");
    //    }
    //}

    //회원가입 요청
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

    //회원가입 수신
    private void ReceiveSignUpResponse(byte[] payload)
    {
        try
        {
            // 페이로드를 Protocol Buffers 메시지로 디코드
            S2CSignUpRes signUpResponse = S2CSignUpRes.Parser.ParseFrom(payload);

            // 결과 코드 확인
            if (signUpResponse.SignUpResultCode == 0)
            {
                // 성공
                SessionUiManager.Instance.SignUpSuccess();

            }
            else
            {
                // 실패 - 오류 코드에 따라 처리
                Debug.LogError($"Sign up failed with code: {signUpResponse.SignUpResultCode}");
                // 예: 사용자에게 오류 메시지 표시
                SessionUiManager.Instance.SignUpFail();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to decode S2CSignUpRes: {ex.Message}");
        }
    }

    //로그인 요청
    public async void SendSignInRequest(string id, string password)
    {
        var signInReq = new C2SSignInReq
        {
            Id = id,
            Password = password
        };

        PacketType packetType = PacketType.SIGN_IN_REQUEST;

        await SendPacketAsync(signInReq, packetType);
        Debug.Log("서버로 패킷 전송, 로그인 요청");
    }

    //로그인 수신
    private void ReceiveSignInResponse(byte[] payload)
    {
        try
        {
            // 페이로드를 Protocol Buffers 메시지로 디코드
            S2CSignInRes signInResponse = S2CSignInRes.Parser.ParseFrom(payload);
            // 결과 코드 확인
            if (signInResponse.SignInResultCode == 0)
            {
                // 성공
                SessionUiManager.Instance.SignInSuccess();

                Debug.Log("로그인 성공");
                //todo : 로그인 성공 후에 (S2CSignInRes) 닉네임 보내달라고 하기
                string token = signInResponse.Token;
                ulong expirationTime = signInResponse.ExpirationTime;
                string userNickname = signInResponse.Nickname;

                DataManager.Instance.SetUserData(token, expirationTime, userNickname);
                //DataManager.Instance.SetUserData(token, expirationTime, userNickname);
            }
            else
            {
                // 실패 - 오류 코드에 따라 처리
                Debug.LogError($"Sign In failed with code: {signInResponse.SignInResultCode}");
                // 예: 사용자에게 오류 메시지 표시
                SessionUiManager.Instance.SignInFail();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to decode S2CSignInRes: {ex.Message}");
        }
    }

    // 토큰 재발급 요청
    public async void SendRefreshTokenRequest(string token)
    {
        var refreshTokenReq = new C2SRefreshTokenReq
        {

        };

        PacketType packetType = PacketType.REFRESH_TOKEN_REQUEST;

        await SendPacketAsync(refreshTokenReq, packetType);
    }

    // 토큰 재발급 수신
    private void ReceiveRefreshTokenResponse(byte[] payload)
    {
        try
        {
            // 페이로드를 Protocol Buffers 메시지로 디코드
            S2CRefreshTokenRes refreshTokenResponse = S2CRefreshTokenRes.Parser.ParseFrom(payload);

            // 결과 코드 확인
            if (refreshTokenResponse.RefreshTokenResultCode == 0)
            {
                // 성공 처리
                string newToken = refreshTokenResponse.Token;
                ulong newExpirationTime = refreshTokenResponse.ExpirationTime;
                DataManager.Instance.RefreshToken(newToken, newExpirationTime);
            }
            else
            {
                // 실패 - 다시 재발급?
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Fail {ex.Message}");
        }
    }

    //방생성 요청
    public async void SendCreateRoomReq(bool isPrivate)
    {
        var createRoomReq = new C2SCreateRoomReq
        {
            IsPrivate = isPrivate
        };

        PacketType packetType = PacketType.CREATE_ROOM_REQUEST;

        await SendPacketAsync(createRoomReq, packetType);
    }
    //방생성 수신
    private void ReceiveCreateRoomResponse(byte[] payload)
    {
        try
        {
            S2CCreateRoomRes createRoomResponse = S2CCreateRoomRes.Parser.ParseFrom(payload);

            string gameCode = createRoomResponse.GameCode;
            string gameUrl = createRoomResponse.GameUrl;
            SessionUiManager.Instance.GameCodeEnterRoom(gameCode);
            //세션서버 접속 해제
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

            //게임서버 접속 시작
            string[] urlParts = gameUrl.Split(':'); // "ip주소,포트번호" 분리
            if (urlParts.Length == 2)
            {
                string ip = urlParts[0].Trim();
                string port = urlParts[1].Trim();

                Debug.Log($"{ip}가 왔어요");
                Debug.Log($"{port}가 왔어요");

                GameServerManager.Instance.isCreateRoomRequest = true;
                GameServerManager.Instance.StartConnect(ip, port);
            }
            else
            {
                Debug.LogError($"Invalid gameUrl format: {gameUrl}");
            }
            //게임서버매니저에서 토큰 유효성검사 실시할것
            //현재 로직 Ui매니저에서 code받아서 코드만 data매니저에 저장 -> 방생성 Ui off, 방 Ui on -> 방 ui는 start하자마자 code을 데이터매니저에서 받아와서 저장
            
        }
        catch (Exception ex)
        {
            Debug.LogError($"Fail {ex.Message}");
        }
    }
    //테스트씬 입장
    public void TestSceneGameServerConnect()
    {
        //세션 서버 접속 끊기
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
        //게임서버 접속하기
        GameServerManager.Instance.TestSceneConnect("211.51.160.19", "5556");
    }

    //방 입장 요청
    public async void SendJoinRoomReq(string gameCode)
    {
        var joinRoomReq = new C2SJoinRoomReq
        {
            GameCode = gameCode,
        };

        PacketType packetType = PacketType.JOIN_ROOM_REQUEST;

        await SendPacketAsync(joinRoomReq, packetType);
    }

    //방 입장 수신 (방생성을 제외한 방법으로)
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
                //세션서버 접속 해제
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

                //게임서버 접속 시작
                string[] urlParts = gameUrl.Split(':'); // "ip주소,포트번호" 분리
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

    //자동 매칭 취소 요청
    public async void SendMatchCacelReq()
    {
        var matchCancelReq = new C2SMatchCancelReq
        {

        };

        PacketType packetType = PacketType.MATCH_CANCEL_REQUEST;

        await SendPacketAsync(matchCancelReq, packetType);
    }

    //자동 매칭 취소 수신
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
    //자동 매칭 신청 발신
    public async void SendMatchStartReq()
    {
        var matchReq = new C2SMatchReq
        {

        };

        PacketType packetType = PacketType.MATCH_START_REQUEST;

        await SendPacketAsync(matchReq, packetType);
    }
    //자동 매칭 신청 수신
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

    //자동 매칭 진행도 수신
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
