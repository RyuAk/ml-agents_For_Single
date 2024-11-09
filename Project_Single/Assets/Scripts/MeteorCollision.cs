using UnityEngine;

public class MeteorCollision : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        // 충돌한 객체가 "Tile" 태그를 가지고 있는지 확인
        if (collision.gameObject.CompareTag("tile"))
        {
            // 타일을 비활성화하거나 파괴
            collision.gameObject.SetActive(false);
            // Destroy(collision.gameObject); // 타일을 완전히 파괴하려면 이 코드 사용

            // 메테오 오브젝트도 파괴 (즉시 파괴할 필요가 없으면 이 부분은 생략 가능)
            Destroy(gameObject);
        }
    }
}
