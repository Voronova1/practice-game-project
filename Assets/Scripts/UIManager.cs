using UnityEngine;
using TMPro;
using System.Collections;
using static TurnManager;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Player 1 UI")]
    public TextMeshProUGUI player1HealthText;
    public TextMeshProUGUI player1SuperText;
    public TextMeshProUGUI player1HintText;

    [Header("Player 2 UI")]
    public TextMeshProUGUI player2HealthText;
    public TextMeshProUGUI player2SuperText;
    public TextMeshProUGUI player2HintText;

    [Header("Common UI")]
    public TextMeshProUGUI turnCountText;
    public TextMeshProUGUI actionCountText;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void InitializeUI()
    {
        // Инициализация текстовых полей
        UpdateHealthDisplay(300, 300);
        UpdateSuperMeter(0, 0);
        UpdateTurnInfo(1);
        UpdateActionCount(0);
        ClearHints();
    }

    public void UpdateHealthDisplay(int p1Health, int p2Health)
    {
        player1HealthText.text = $"HP: {p1Health}";
        player2HealthText.text = $"HP: {p2Health}";
    }

    public void UpdateSuperMeter(int p1Charges, int p2Charges)
    {
        player1SuperText.text = $"Супер-атака: {p1Charges}/2";
        player2SuperText.text = $"Супер-атака: {p2Charges}/2";
    }

    public void UpdateTurnInfo(int turnCount)
    {
        turnCountText.text = $"Ход: {turnCount}/10";
    }

    public void UpdateActionCount(int actionsPerformed)
    {
        actionCountText.text = $"Действия: {actionsPerformed}/1";
    }

    public void ShowHint(TurnManager.PlayerTurn player, string message)
    {
        if (player == TurnManager.PlayerTurn.Player1)
        {
            player1HintText.text = message;
            StartCoroutine(ClearHintAfterDelay(player1HintText, 3f));
        }
        else
        {
            player2HintText.text = message;
            StartCoroutine(ClearHintAfterDelay(player2HintText, 3f));
        }
    }

    public void ClearHints()
    {
        player1HintText.text = "";
        player2HintText.text = "";
    }

    private IEnumerator ClearHintAfterDelay(TextMeshProUGUI hintText, float delay)
    {
        yield return new WaitForSeconds(delay);
        hintText.text = "";
    }

    public void ShowContextMessage(string message)
    {
        if (TurnManager.Instance.CurrentTurn == PlayerTurn.Player1)
            player1HintText.text = message;
        else
            player2HintText.text = message;
    }
}