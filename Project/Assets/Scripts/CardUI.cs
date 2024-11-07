using UnityEngine;
using UnityEngine.UI;

public class CardUI : MonoBehaviour
{
    public Transform[] cardSlots; // 카드가 들어갈 UI 슬롯 배열 (2개의 슬롯)
    private GameObject[] currentCards = new GameObject[2]; // UI 슬롯에 들어간 카드들

    public void SetCard(GameObject card, int index)
    {
        if (index < 0 || index >= cardSlots.Length)
        {
            Debug.LogError("잘못된 슬롯 인덱스입니다.");
            return;
        }

        // 기존 슬롯에 있는 카드를 제거하고 새 카드로 대체
        if (currentCards[index] != null)
        {
            Destroy(currentCards[index]);
        }

        // 새로운 카드를 슬롯에 배치
        card.transform.SetParent(cardSlots[index]); // 카드 슬롯의 자식으로 설정
        card.transform.localScale = Vector3.one; // 크기를 슬롯에 맞게 조정
        card.transform.localPosition = Vector3.zero; // 위치 초기화
        card.transform.localRotation = Quaternion.identity; // 회전 초기화

        // 슬롯 크기에 맞게 카드 크기 조정 (필요시)
        RectTransform cardRectTransform = card.GetComponent<RectTransform>();
        RectTransform slotRectTransform = cardSlots[index].GetComponent<RectTransform>(); // 기존의 cardSlot을 cardSlots[index]로 수정
        cardRectTransform.sizeDelta = slotRectTransform.sizeDelta;  // 슬롯의 크기에 맞게 카드 크기 설정

        currentCards[index] = card; // 슬롯에 카드 저장
    }

    public GameObject GetCardObject(int index)
    {
        if (index < 0 || index >= currentCards.Length)
        {
            Debug.LogError("잘못된 슬롯 인덱스입니다.");
            return null;
        }

        return currentCards[index];
    }
}
