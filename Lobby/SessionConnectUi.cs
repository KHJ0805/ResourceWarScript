using UnityEngine;
using TMPro;
using DG.Tweening;

public class SessionConnectUi : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI connectingMassage;
    public GameObject connectFailUi;
    public GameObject loginUi;

    private void Start()
    {
        var networkManager = SessionNetworkManager.Instance;
        networkManager.OnConnectionSuccess += OnConnectionSuccess;
        networkManager.OnConnectionFailure += OnConnectionFailure;

        // 연결 시도
        networkManager.StartConnect("SessionIp", "SessionPort");
        Debug.Log("connect try");

        // 연결 UI 애니메이션
        StartConnectingAnimation();
    }

    private void StartConnectingAnimation()
    {
        DOTween.Kill(this);
        int dotCount = 1;
        DOTween.To(() => dotCount, x => dotCount = x, 3, 1f).OnUpdate(() =>
        {
            connectingMassage.text = $"Connecting{new string('.', dotCount)}";
        }).SetLoops(-1, LoopType.Yoyo);
    }

    private void OnConnectionSuccess()
    {
        connectingMassage.text = "Connected!";
        loginUi.SetActive(true);
        gameObject.SetActive(false);
    }

    private void OnConnectionFailure()
    {
        connectingMassage.text = "Connection failed!";
        connectFailUi.SetActive(true);
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (SessionNetworkManager.Instance != null)
        {
            var networkManager = SessionNetworkManager.Instance;
            networkManager.OnConnectionSuccess -= OnConnectionSuccess;
            networkManager.OnConnectionFailure -= OnConnectionFailure;
        }
        DOTween.Kill(this);
    }
}
