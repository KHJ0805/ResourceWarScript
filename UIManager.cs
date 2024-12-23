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
    public int selectedMonsterIndex = -1; // 선택된 몬스터의 인덱스 선택 안된상태
    //private CraftBench currentCraftBench;

    public void CraftBenchSetting()
    {
        mapcraftBench = MapManager.Instance.CraftingBench;
        craftBenchUI.ImageSetting();
        craftBench = mapcraftBench.GetComponent<CraftBench>();
    }

    public void SelectMonsterSlot(MonsterSlotUI slot)
    {
        //// 새로운 CraftBench를 찾아 설정
        //CraftBench newCraftBench = slot.GetComponentInParent<CraftBench>();

        //// 새로 선택된 CraftBench 업데이트
        //if (newCraftBench != null)
        //{
        //    newCraftBench.SelectSlot(slot);
        //    currentCraftBench = newCraftBench; // 현재 CraftBench로 업데이트
        //}

        //// 슬롯 및 인덱스 갱신
        //currentSelectedSlot = slot;
        //selectedMonsterIndex = slot.monsterIndex;
    }




    //이전 플젝에서 쓰던 건데 쓸지는 모르겠음
    //public void ShowUI(string uiName)
    //{
    //    // 모든 UI를 비활성화
    //    monsterSelectUI.gameObject.SetActive(false);
    //    monsterPurchaseUI.gameObject.SetActive(false);

    //    // 선택한 UI만 활성화
    //    switch (uiName)
    //    {
    //        case "MonsterSelectUI":
    //            monsterSelectUI.gameObject.SetActive(true);
    //            break;
    //        case "MonsterPurchaseUI":
    //            monsterPurchaseUI.gameObject.SetActive(true);
    //            break;
    //        default:
    //            Debug.LogWarning($"UI '{uiName}'를 찾을 수 없습니다.");
    //            break;
    //    }
    //}
}
