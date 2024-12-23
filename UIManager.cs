using System.Collections;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{

    [Header("UI")]
    public MonsterSelectUI monsterSelectUI;
    public MonsterPurchaseUI monsterPurchaseUI;
    public MonsterSortieUI monsterSortieUI;
    public GameObject mapcraftBench;
    public CraftBench craftBench;
    public CraftBenchUI craftBenchUI;
    public SettingBtnAction settingBtnAction;

    [Header("Index")]
    public MonsterSlotUI currentSelectedSlot;
    public int selectedMonsterIndex = -1; // ���õ� ������ �ε��� ���� �ȵȻ���
    //private CraftBench currentCraftBench;

    public void CraftBenchSetting()
    {
        mapcraftBench = MapManager.Instance.CraftingBench;
        craftBenchUI.ImageSetting();
        craftBench = mapcraftBench.GetComponent<CraftBench>();
    }

    public void SelectMonsterSlot(MonsterSlotUI slot)
    {
        //// ���ο� CraftBench�� ã�� ����
        //CraftBench newCraftBench = slot.GetComponentInParent<CraftBench>();

        //// ���� ���õ� CraftBench ������Ʈ
        //if (newCraftBench != null)
        //{
        //    newCraftBench.SelectSlot(slot);
        //    currentCraftBench = newCraftBench; // ���� CraftBench�� ������Ʈ
        //}

        //// ���� �� �ε��� ����
        //currentSelectedSlot = slot;
        //selectedMonsterIndex = slot.monsterIndex;
    }




    //���� �������� ���� �ǵ� ������ �𸣰���
    //public void ShowUI(string uiName)
    //{
    //    // ��� UI�� ��Ȱ��ȭ
    //    monsterSelectUI.gameObject.SetActive(false);
    //    monsterPurchaseUI.gameObject.SetActive(false);

    //    // ������ UI�� Ȱ��ȭ
    //    switch (uiName)
    //    {
    //        case "MonsterSelectUI":
    //            monsterSelectUI.gameObject.SetActive(true);
    //            break;
    //        case "MonsterPurchaseUI":
    //            monsterPurchaseUI.gameObject.SetActive(true);
    //            break;
    //        default:
    //            Debug.LogWarning($"UI '{uiName}'�� ã�� �� �����ϴ�.");
    //            break;
    //    }
    //}
}
