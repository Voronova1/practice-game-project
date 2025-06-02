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
    [SerializeField] private Button Card1; // Кнопка первой карты
    [SerializeField] private Button Card2; // Кнопка второй карты
    [SerializeField] private TextMeshProUGUI Card1Text;
    [SerializeField] private TextMeshProUGUI Card2Text;

    private List<CardEffect> allCards = new List<CardEffect>();
    private CardEffect[] currentCards = new CardEffect[2];
    private Unit.UnitOwner currentPlayer;

    private void Awake()
    {
        InitializeAllCards();
        cardPanel.SetActive(false);

        // Назначаем обработчики кнопок
        Card1.onClick.AddListener(() => OnCardSelected(0));
        Card2.onClick.AddListener(() => OnCardSelected(1));
    }

    private void InitializeAllCards()
    {
        // 1. Карта исцеления
        allCards.Add(new CardEffect
        {
            cardName = "Божественное исцеление",
            description = "+15 единиц здоровья",
            ApplyEffect = (owner) => {
                foreach (var unit in FindUnitsByOwner(owner))
                {
                    unit.TakeDamage(-Mathf.RoundToInt(unit.CurrentHealth + 15));
                }
            }
        });

        // 2. Карта атаки
        allCards.Add(new CardEffect
        {
            cardName = "Ярость",
            description = "+30% урона,\nно -15% здоровья",
            ApplyEffect = (owner) => {
                foreach (var unit in FindUnitsByOwner(owner))
                {
                    unit.damage = Mathf.RoundToInt(unit.damage * 1.3f);
                    unit.TakeDamage(Mathf.RoundToInt(unit.CurrentHealth * 0.15f));
                }
            }
        });

        // 3. Карта защиты
        allCards.Add(new CardEffect
        {
            cardName = "Укрепление",
            description = "+25 единиц здоровья",
            ApplyEffect = (owner) => {
                foreach (var unit in FindUnitsByOwner(owner))
                {
                    unit.TakeDamage(-Mathf.RoundToInt(unit.CurrentHealth + 25));
                }
            }
        });

        // 4. Карта магии
        allCards.Add(new CardEffect
        {
            cardName = "Стальная воля",
            description = "Мечник получает +40% урона, другие юниты -10%",
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

        // 5. Карта магии
        allCards.Add(new CardEffect
        {
            cardName = "Магическая вспышка",
            description = "Маг получает +50% урона, другие юниты -15%",
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

        // 6. Карта баланса
        allCards.Add(new CardEffect
        {
            cardName = "Равновесие",
            description = "Выравнивает здоровье всех\nюнитов до среднего значения",
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
        // Убедимся, что карты разные
        int firstIndex = Random.Range(0, allCards.Count);
        int secondIndex;
        do
        {
            secondIndex = Random.Range(0, allCards.Count);
        } while (secondIndex == firstIndex);

        currentCards[0] = allCards[firstIndex];
        currentCards[1] = allCards[secondIndex];

        // Обновляем UI
        Card1Text.text = $"<b>{currentCards[0].cardName}</b>\n{currentCards[0].description}";
        Card2Text.text = $"<b>{currentCards[1].cardName}</b>\n{currentCards[1].description}";
    }

    private void ShowCardUI()
    {
        cardPanel.SetActive(true);
        // Сбрасываем выделение кнопок (если нужно)
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

        // Делаем кнопки неактивными после выбора
        Card1.interactable = false;
        Card2.interactable = false;

        // Применяем эффект выбранной карты
        currentCards[cardIndex].ApplyEffect(currentPlayer);
        Debug.Log($"{currentPlayer} выбрал карту: {currentCards[cardIndex].cardName}");

        // Скрываем UI с небольшой задержкой (можно добавить анимацию)
        Invoke("HideCardUI", 0.5f);

        // Уведомляем, что выбор сделан
        Debug.Log("Выбор карт завершен, можно продолжать игру");
    }
}