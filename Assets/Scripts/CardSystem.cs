using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class CardSystem : MonoBehaviour
{
    [System.Serializable]
    public class CardEffect
    {
        public string cardName;
        [TextArea] public string description;
        public System.Action<Unit.UnitOwner> ApplyEffect;
    }

    [Header("UI References")]
    [SerializeField] private GameObject cardPanel;
    [SerializeField] private Button Card1; // ������ ������ �����
    [SerializeField] private Button Card2; // ������ ������ �����
    [SerializeField] private TextMeshProUGUI Card1Text;
    [SerializeField] private TextMeshProUGUI Card2Text;

    private List<CardEffect> allCards = new List<CardEffect>();
    private CardEffect[] currentCards = new CardEffect[2];
    private Unit.UnitOwner currentPlayer;

    private void Awake()
    {
        InitializeAllCards();
        cardPanel.SetActive(false);

        // ��������� ����������� ������
        Card1.onClick.AddListener(() => OnCardSelected(0));
        Card2.onClick.AddListener(() => OnCardSelected(1));
    }

    private void InitializeAllCards()
    {
        // 1. ����� ���������
        allCards.Add(new CardEffect
        {
            cardName = "������������ ���������",
            description = "+15 ������ ��������",
            ApplyEffect = (owner) => {
                foreach (var unit in FindUnitsByOwner(owner))
                {
                    unit.TakeDamage(-Mathf.RoundToInt(unit.CurrentHealth + 15));
                }
            }
        });

        // 2. ����� �����
        allCards.Add(new CardEffect
        {
            cardName = "������",
            description = "+30% �����,\n�� -15% ��������",
            ApplyEffect = (owner) => {
                foreach (var unit in FindUnitsByOwner(owner))
                {
                    unit.damage = Mathf.RoundToInt(unit.damage * 1.3f);
                    unit.TakeDamage(Mathf.RoundToInt(unit.CurrentHealth * 0.15f));
                }
            }
        });

        // 3. ����� ������
        allCards.Add(new CardEffect
        {
            cardName = "����������",
            description = "+25 ������ ��������",
            ApplyEffect = (owner) => {
                foreach (var unit in FindUnitsByOwner(owner))
                {
                    unit.TakeDamage(-Mathf.RoundToInt(unit.CurrentHealth + 25));
                }
            }
        });

        // 4. ����� �����
        allCards.Add(new CardEffect
        {
            cardName = "�������� ����",
            description = "������ �������� +40% �����, ������ ����� -10%",
            ApplyEffect = (owner) => {
                foreach (var unit in FindUnitsByOwner(owner))
                {
                    if (unit.unitType == Unit.UnitType.Warrior)
                        unit.damage = Mathf.RoundToInt(unit.damage * 1.4f);
                    else
                        unit.damage = Mathf.RoundToInt(unit.damage * 0.9f);
                }
            }
        });

        // 5. ����� �����
        allCards.Add(new CardEffect
        {
            cardName = "���������� �������",
            description = "��� �������� +50% �����, ������ ����� -15%",
            ApplyEffect = (owner) => {
                foreach (var unit in FindUnitsByOwner(owner))
                {
                    if (unit.unitType == Unit.UnitType.Mage)
                        unit.damage = Mathf.RoundToInt(unit.damage * 1.5f);
                    else
                        unit.damage = Mathf.RoundToInt(unit.damage * 0.85f);
                }
            }
        });

        // 6. ����� �������
        allCards.Add(new CardEffect
        {
            cardName = "����������",
            description = "����������� �������� ����\n������ �� �������� ��������",
            ApplyEffect = (owner) => {
                var units = FindUnitsByOwner(owner);
                int totalHealth = 0;
                foreach (var unit in units) totalHealth += unit.CurrentHealth;
                int averageHealth = units.Count > 0 ? totalHealth / units.Count : 0;

                foreach (var unit in units)
                {
                    unit.TakeDamage(-averageHealth);
                }
            }
        });
    }

    private List<Unit> FindUnitsByOwner(Unit.UnitOwner owner)
    {
        Unit[] allUnits = FindObjectsOfType<Unit>();
        List<Unit> playerUnits = new List<Unit>();

        foreach (Unit unit in allUnits)
        {
            if (unit.owner == owner)
            {
                playerUnits.Add(unit);
            }
        }

        return playerUnits;
    }

    public void StartCardSelection(Unit.UnitOwner player)
    {
        currentPlayer = player;
        GenerateRandomCards();
        ShowCardUI();
    }

    private void GenerateRandomCards()
    {
        // ��������, ��� ����� ������
        int firstIndex = Random.Range(0, allCards.Count);
        int secondIndex;
        do
        {
            secondIndex = Random.Range(0, allCards.Count);
        } while (secondIndex == firstIndex);

        currentCards[0] = allCards[firstIndex];
        currentCards[1] = allCards[secondIndex];

        // ��������� UI
        Card1Text.text = $"<b>{currentCards[0].cardName}</b>\n{currentCards[0].description}";
        Card2Text.text = $"<b>{currentCards[1].cardName}</b>\n{currentCards[1].description}";
    }

    private void ShowCardUI()
    {
        cardPanel.SetActive(true);
        // ���������� ��������� ������ (���� �����)
        Card1.interactable = true;
        Card2.interactable = true;
    }

    private void HideCardUI()
    {
        cardPanel.SetActive(false);
    }

    private void OnCardSelected(int cardIndex)
    {
        if (cardIndex < 0 || cardIndex >= currentCards.Length) return;

        // ������ ������ ����������� ����� ������
        Card1.interactable = false;
        Card2.interactable = false;

        // ��������� ������ ��������� �����
        currentCards[cardIndex].ApplyEffect(currentPlayer);
        Debug.Log($"{currentPlayer} ������ �����: {currentCards[cardIndex].cardName}");

        // �������� UI � ��������� ��������� (����� �������� ��������)
        Invoke("HideCardUI", 0.5f);

        // ����������, ��� ����� ������
        Debug.Log("����� ���� ��������, ����� ���������� ����");
    }
}