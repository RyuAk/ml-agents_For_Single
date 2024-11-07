using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HolyGroundCard : MonoBehaviour
{
    private List<GameObject> highlightedTiles = new List<GameObject>(); // 하이라이트된 타일 리스트
    private HashSet<GameObject> holyGroundTiles = new HashSet<GameObject>(); // 부술 수 없는 타일 목록

    public void ActivateEffect(PlayerMove playerMove)
    {
        HighlightSelectableTiles();

        // 타일을 선택하고 부술 수 없는 상태로 설정하는 코루틴
        StartCoroutine(WaitForTileSelection());
    }

    private void HighlightSelectableTiles()
    {
        ClearHighlightedTiles();

        Vector3 playerPos = transform.position;

        // 주변의 타일을 확인
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;

                Vector3 tilePos = new Vector3(playerPos.x + x, playerPos.y - 1, playerPos.z + y);
                Collider[] hitColliders = Physics.OverlapSphere(tilePos, 0.1f);

                foreach (Collider hitCollider in hitColliders)
                {
                    GameObject tileObj = hitCollider.gameObject;
                    if (tileObj.CompareTag("Tile") && !holyGroundTiles.Contains(tileObj)) // 이미 부술 수 없는 땅이 아닌 타일만 하이라이트
                    {
                        // 타일이 존재하는지 확인
                        if (tileObj.activeInHierarchy)
                        {
                            Renderer renderer = tileObj.GetComponent<Renderer>();
                            if (renderer != null)
                            {
                                // 하이라이트 색상 설정 (임시로 파란색)
                                renderer.material.color = Color.blue;
                                highlightedTiles.Add(tileObj);
                            }
                        }
                    }
                }
            }
        }
    }

    private void ClearHighlightedTiles()
    {
        foreach (GameObject tileObj in highlightedTiles)
        {
            Renderer renderer = tileObj.GetComponent<Renderer>();
            if (renderer != null)
            {
                // 원래 색상으로 복원
                renderer.material.color = Color.white; // 원래 색상으로 변경 필요
            }
        }
        highlightedTiles.Clear();
    }

    private IEnumerator WaitForTileSelection()
    {
        while (true)
        {
            if (Input.GetMouseButtonDown(0)) // 마우스 클릭 입력
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    GameObject clickedTile = hit.collider.gameObject;
                    if (clickedTile.CompareTag("Tile") && highlightedTiles.Contains(clickedTile))
                    {
                        // 선택된 타일을 부술 수 없는 상태로 설정
                        MarkAsHolyGround(clickedTile);
                        yield break; // 선택이 완료되면 코루틴 종료
                    }
                }
            }
            yield return null; // 다음 프레임까지 대기
        }
    }

    private void MarkAsHolyGround(GameObject tile)
    {
        // 부술 수 없는 땅으로 설정
        holyGroundTiles.Add(tile);

        // 타일의 색상을 변경하여 상태를 표시
        Renderer renderer = tile.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.green; // 부술 수 없는 땅은 녹색으로 표시
        }

        // 부술 수 없는 상태로 관리 (해당 타일을 파괴하는 모든 로직에서 제외해야 함)
    }

    // 부술 수 없는 타일인지 확인하는 메서드를 다른 파괴 관련 코드에서 호출해야 함
    public bool IsHolyGround(GameObject tile)
    {
        return holyGroundTiles.Contains(tile);
    }
}
