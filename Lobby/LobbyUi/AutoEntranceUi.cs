using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AutoEntranceUi : BaseUi
{
    [Header("UI Elements")]
    public Button closeAutoEntranceBtn;
    public TextMeshProUGUI matchProgress;
    public TextMeshProUGUI matchName;

    protected override void Start()
    {
        base.Start();

        AddButtonListener(closeAutoEntranceBtn, OnCloseAutoEntranceUiClicked);
        Blink();
        //todo : 자동매칭 로직 메서드
    }

    private void OnCloseAutoEntranceUiClicked()
    {
        gameObject.SetActive(false);
    }
    private void Blink()
    {
        matchName.DOFade(0, 0.5f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }
}
