using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardManager : MonoBehaviour
{
    public static CardManager instance;
    public Sprite[] cardSprites; // 카드 이미지(스프라이트) 배열
    public Transform[] cardDecks; // 8개의 카드 덱 위치를 담을 배열
    private Dictionary<GameObject, List<GameObject>> playerCards = new Dictionary<GameObject, List<GameObject>>(); // 각 플레이어가 가진 카드

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        GameObject currentPlayer = GameManager.instance.currentPlayer;
        if (currentPlayer != null && currentPlayer == GameManager.instance.currentPlayer)
        {
            if (Input.GetKeyDown(KeyCode.Z))
            {
                UseCard(0); // 슬롯 1번 카드 사용
            }
            if (Input.GetKeyDown(KeyCode.X))
            {
                UseCard(1); // 슬롯 2번 카드 사용
            }
        }
    }

    public void DistributeCards(GameObject[] players)
    {
        if (cardDecks.Length < players.Length * 2)
        {
            Debug.LogError("카드 덱의 수가 플레이어 수에 맞지 않습니다.");
            return;
        }

        // 각 플레이어에게 2장의 카드 분배
        for (int i = 0; i < players.Length; i++)
        {
            GameObject player = players[i];
            CardUI cardUI = player.GetComponent<CardUI>(); // 플레이어의 CardUI를 가져옴

            if (cardUI == null)
            {
                Debug.LogError($"{player.name}의 CardUI를 찾을 수 없습니다.");
                continue; // CardUI가 없으면 다음 플레이어로 넘어감
            }

            if (!playerCards.ContainsKey(player))
            {
                playerCards.Add(player, new List<GameObject>());
            }

            for (int j = 0; j < 2; j++)
            {
                int randomIndex = Random.Range(0, cardSprites.Length);
                Sprite cardSprite = cardSprites[randomIndex];

                // UI 슬롯에 카드를 배치
                GameObject cardObject = new GameObject("Card"); // 새로 카드 객체 생성
                cardObject.transform.SetParent(cardUI.transform); // 카드 UI 슬롯의 자식으로 설정

                // Image 컴포넌트를 카드 오브젝트에 추가
                Image cardImage = cardObject.AddComponent<Image>();
                cardImage.sprite = cardSprite; // 이미지로 카드 스프라이트 할당

                // AspectRatioFitter 추가 (이미지 비율 유지)
                AspectRatioFitter aspectRatioFitter = cardObject.AddComponent<AspectRatioFitter>();
                aspectRatioFitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent; // 부모에 맞게 이미지 크기 조정
                aspectRatioFitter.aspectRatio = cardSprite.rect.width / cardSprite.rect.height; // 스프라이트의 비율 설정

                switch (randomIndex)
                {
                    case 0: 
                        cardObject.AddComponent<ArrestCard>();
                        break;
                    case 1: 
                        cardObject.AddComponent<BeamCard>();
                        break;
                    case 2:
                        cardObject.AddComponent<ConcessionCard>();
                        break;
                    case 3:
                        cardObject.AddComponent<GatewayCard>();
                        break;
                    case 4:
                        cardObject.AddComponent<HolyGroundCard>();
                        break;
                    case 5:
                        cardObject.AddComponent<LightingCard>();
                        break;
                    case 6:
                        cardObject.AddComponent<MeteoCard>();
                        break;
                    case 7:
                        cardObject.AddComponent<PsychokinesisCard>();
                        break;
                    default:
                        Debug.LogWarning("무작위로 선택된 카드 스크립트가 없습니다.");
                        break;
                }
                // UI에 카드를 배치
                cardUI.SetCard(cardObject, j); // 카드 UI 슬롯에 카드 배치
                playerCards[player].Add(cardObject); // 플레이어의 카드 리스트에 추가
            }
        }
    }

    public void UseCard(int cardIndex)
    {
        GameObject currentPlayer = GameManager.instance.currentPlayer;
        CardUI cardUI = currentPlayer.GetComponent<CardUI>();
        if (cardUI == null)
        {
            Debug.LogError("현재 플레이어의 CardUI를 찾을 수 없습니다.");
            return;
        }

        GameObject cardObject = cardUI.GetCardObject(cardIndex);
        if (cardObject == null)
        {
            Debug.Log("해당 슬롯에 카드가 없습니다.");
            return;
        }

        ICard card = cardObject.GetComponent<ICard>();
        if (card != null)
        {
            PlayerMove currentPlayerMove = currentPlayer.GetComponent<PlayerMove>();
            List<PlayerMove> allPlayers = new List<PlayerMove>(FindObjectsOfType<PlayerMove>());
            card.UseCard(currentPlayerMove, allPlayers); // 카드 사용
        }
        else
        {
            Debug.LogError("해당 슬롯에 ICard 인터페이스가 연결되지 않은 카드입니다.");
        }
    }
}
