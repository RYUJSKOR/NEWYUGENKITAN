using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

// 選択されたスライダーに対応するテキスト色をハイライト表示する
// ※ このスクリプトを各スライダーのGameObjectにアタッチしてください
public class SliderLabelHighlight : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    [SerializeField] private TextMeshProUGUI label; // 対応するラベル(TMP)
    [SerializeField] private Color normalColor = Color.white; // 非選択時の色
    [SerializeField] private Color selectedColor = new Color(1f, 0.85f, 0.2f); // 選択時(ハイライト)の色

    // スライダーが選択状態になったとき（キーボード/コントローラ/マウスいずれでも）
    public void OnSelect(BaseEventData eventData)
    {
        if (label != null) label.color = selectedColor;
    }

    // スライダーの選択が外れたとき
    public void OnDeselect(BaseEventData eventData)
    {
        if (label != null) label.color = normalColor;
    }

    // 初期化（パネルを開いた瞬間の見た目を整えるため）
    private void OnEnable()
    {
        // 既に選択中なら選択色、そうでなければ通常色にする
        bool isCurrentlySelected = EventSystem.current != null
            && EventSystem.current.currentSelectedGameObject == gameObject;

        if (label != null) label.color = isCurrentlySelected ? selectedColor : normalColor;
    }
}
