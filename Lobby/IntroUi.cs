using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IntroUi : BaseUi
{
    public Button closeIntroUi;
    public GameObject ConnectUi;
    public TextMeshProUGUI pressAnyKeyText;

    protected override void Start()
    {
        base.Start();

        AddButtonListener(closeIntroUi, OnCancelButtonClicked);
        Blink();
    }
    private void OnCancelButtonClicked()
    {
        closeIntroUi.interactable = false;

        if (ConnectUi != null)
        {
            ConnectUi.SetActive(true);
        }
    }
    private void Blink()
    {
        pressAnyKeyText.DOFade(0, 0.5f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }
}
