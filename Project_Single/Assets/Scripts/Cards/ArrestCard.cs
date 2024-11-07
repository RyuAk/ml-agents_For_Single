using System.Collections.Generic;
using UnityEngine;

public class ArrestCard : MonoBehaviour, ICard
{
    private PlayerMove currentPlayer;
    private bool isCardActive = false;

    public void UseCard(PlayerMove playerMove, List<PlayerMove> allPlayers)
    {
        currentPlayer = playerMove;
        ActivateEffect();
    }

    private void ActivateEffect()
    {
        isCardActive = true;
        Debug.Log("ArrestCard가 활성화되었습니다. 다른 플레이어를 클릭하세요.");
    }

    private void Update()
    {
        if (isCardActive && Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                GameObject selectedPlayerObject = hit.collider.gameObject;

                if (selectedPlayerObject != null && selectedPlayerObject != currentPlayer.gameObject)
                {
                    PlayerMove selectedPlayerMove = selectedPlayerObject.GetComponent<PlayerMove>();
                    if (selectedPlayerMove != null)
                    {
                        selectedPlayerMove.movesLeft = 0;
                        selectedPlayerMove.isArrested = true;  // 플레이어를 Arrest 상태로 설정
                        selectedPlayerMove.AfterCardUse();

                        Debug.Log($"{selectedPlayerObject.name}의 움직임이 제한되었습니다.");
                    }

                    isCardActive = false;
                }
                else
                {
                    Debug.Log("유효하지 않은 선택입니다. 다른 플레이어를 클릭하세요.");
                }
            }
        }
    }
}
