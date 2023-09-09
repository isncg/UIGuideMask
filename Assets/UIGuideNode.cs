using UnityEngine;

public class UIGuideNode : MonoBehaviour
{

    [ContextMenu("Show ui guide mask")]
    private void ShowUIGuidMask()
    {
        UIGuideRoot.ShowUIGuideMask(gameObject.GetComponent<RectTransform>());
    }
    
    [ContextMenu("Hide ui guide mask")]
    private void HideUIGuidMask()
    {
        UIGuideRoot.HideUIGuideMask();
    }
    
}
