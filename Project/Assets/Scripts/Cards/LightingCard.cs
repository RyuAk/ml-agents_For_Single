using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightingCard : MonoBehaviour
{
    private List<GameObject> highlightedTiles = new List<GameObject>(); // 하이라이트된 타일 리스트

    public void ActivateEffect(PlayerMove playerMove)
    {
        HighlightDestructibleTiles();

        // 타일 클릭 시 선택된 타일을 파괴하는 로직
        StartCoroutine(WaitForTileSelection());
    }

    private void HighlightDestructibleTiles()
    {
        ClearHighlightedTiles();

        Vector3 playerPos = transform.position;

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
                    if (tileObj.CompareTag("Tile")) // 타일 태그를 사용하여 타일 오브젝트를 식별
                    {
                        // 타일이 존재하는지 확인
                        if (tileObj.activeInHierarchy)
                        {
                            Renderer renderer = tileObj.GetComponent<Renderer>();
                            if (renderer != null)
                            {
                                // 색상 변경 (임시로 노란색)
                                renderer.material.color = Color.yellow;
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
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    GameObject clickedTile = hit.collider.gameObject;
                    if (clickedTile.CompareTag("Tile") && highlightedTiles.Contains(clickedTile))
                    {
                        // 타일 파괴 로직
                        Destroy(clickedTile); // 타일 오브젝트 파괴
                        yield break; // 타일을 파괴한 후 코루틴 종료
                    }
                }
            }
            yield return null; // 다음 프레임까지 대기
        }
    }
}
