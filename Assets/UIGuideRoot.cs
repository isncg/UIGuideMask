using UnityEngine;
using UnityEngine.UI;

public class UIGuideRoot : MonoBehaviour
{
    private static UIGuideRoot Instance = null;
    [SerializeField]
    private UIGuideMask mask = null;
    private void Awake()
    {
        if (null == Instance)
            Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public static void ShowUIGuideMask(RectTransform target)
    {
        if (null == Instance)
            return;
        Instance.mask.m_Target = target;
        var img = target.GetComponent<Image>();
        Instance.mask.m_Sprite = img == null ? null : img.sprite;
        Instance.mask.SetVerticesDirty();
        Instance.mask.SetMaterialDirty();
        Instance.mask.gameObject.SetActive(true);
    }

    public static void HideUIGuideMask()
    {
        if (null == Instance)
            return;
        Instance.mask.gameObject.SetActive(false);
    }
}
