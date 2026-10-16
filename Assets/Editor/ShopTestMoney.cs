#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class ShopTestMoney
{
    private static string AppliedKey => $"{Application.dataPath}.ShopRandomItemTestMoney.20261016";

    [InitializeOnLoadMethod]
    private static void ApplyOnce()
    {
        if (!EditorPrefs.GetBool(AppliedKey, false))
        {
            EditorApplication.delayCall += SetMoneyTo2000;
        }
    }

    [MenuItem("Tools/Volleyball/상점 테스트 금액 2000원 설정")]
    public static void SetMoneyTo2000()
    {
        if (WalletManager.Instance != null)
        {
            WalletManager.Instance.SetMoney(2000);
        }
        else
        {
            PlayerSaveManager.SaveMoney(2000);
        }

        EditorPrefs.SetBool(AppliedKey, true);
        Debug.Log("상점 검증용 보유 금액을 2,000원으로 설정했습니다.");
    }
}
#endif
