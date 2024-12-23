using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class WarningUi : BaseUi
{
    public Button closeButton;
    protected override void Start()
    {
        base.Start();

        AddButtonListener(closeButton, OnCButtonClicked);
    }

    private void OnCButtonClicked()
    {
        gameObject.SetActive(false);
    }
}
