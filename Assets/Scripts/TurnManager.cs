using UnityEngine;
using TMPro;
using System.Collections;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }
    public enum PlayerTurn { Player1, Player2 }
    public PlayerTurn CurrentTurn { get; private set; } = PlayerTurn.Player1;

    [SerializeField] private UnitSelector unitSelector;
    [SerializeField] private TileHighlighter tileHighlighter;
    [SerializeField] private TMP_Text victoryText;
    [SerializeField] private CardSystem �ardSystem;
    [SerializeField] private float cardRevealDelay = 2f; // �������� ����� ������� ����

    private int turnCount = 1;
    private const int maxTurns = 10;
    private int actionsPerformed = 0;
    private int player1SuperCharges = 0;
    private int player2SuperCharges = 0;
    private bool cardsRevealed = false;

    private enum GamePhase { WaitingForCards, CardSelection, UnitSelection, ActionSelection }
    private GamePhase currentPhase = GamePhase.WaitingForCards;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        if (victoryText != null)
            victoryText.gameObject.SetActive(false);
    }

    void Start()
    {
        // ������������� UI
        UIManager.Instance.InitializeUI();

        // ��������� ��������
        CurrentTurn = PlayerTurn.Player1;
        turnCount = 1;
        actionsPerformed = 0;

        // ���������� UI
        UIManager.Instance.UpdateTurnInfo(turnCount);
        UIManager.Instance.UpdateActionCount(actionsPerformed);
        UIManager.Instance.UpdateSuperMeter(0, 0);

        // �������� � �������� ����� ������� ����
        StartCoroutine(StartTurnWithDelay());

        player1SuperCharges = 0;
        player2SuperCharges = 0;
    }

    void Update()
    {
        // ��������� �������� ������� ������ ����
        CheckPlayersHealth();
    }

    private void CheckPlayersHealth()
    {
        int p1Health = CalculateTotalHealth(Unit.UnitOwner.Player1);
        int p2Health = CalculateTotalHealth(Unit.UnitOwner.Player2);

        // ���� � ������ �� ������� ����������� ����� � HP > 0
        if (p1Health <= 0 || p2Health <= 0)
        {
            EndGame();
        }
    }


    private IEnumerator StartTurnWithDelay()
    {
        
        cardsRevealed = false;
        currentPhase = GamePhase.WaitingForCards;
        UIManager.Instance.ShowHint(CurrentTurn, "���������� ����...");

       
        yield return new WaitForSeconds(cardRevealDelay);

        
        cardsRevealed = true;
        StartCardSelectionPhase();
    }

    public void StartFirstTurn()
    {
        if (UIManager.Instance == null)
        {
            Debug.LogError("UIManager �� ���������������!");
            return;
        }

        CurrentTurn = PlayerTurn.Player1;
        turnCount = 1;
        actionsPerformed = 0;
        player1SuperCharges = 0;
        player2SuperCharges = 0;

        UIManager.Instance.UpdateTurnInfo(turnCount);
        UIManager.Instance.UpdateActionCount(actionsPerformed);
        UIManager.Instance.UpdateSuperMeter(0, 0);

        StartCoroutine(StartTurnWithDelay());
        Debug.Log("������ ���� - ��� Player1");
    }

    private void StartCardSelectionPhase()
    {
        currentPhase = GamePhase.CardSelection;
        UIManager.Instance.ShowHint(CurrentTurn, "�������� ����� �����");
        �ardSystem.StartCardSelection(CurrentTurn == PlayerTurn.Player1 ? Unit.UnitOwner.Player1 : Unit.UnitOwner.Player2);
    }

    public bool CanPerformActions()
    {
        return cardsRevealed && currentPhase != GamePhase.WaitingForCards;
    }

    public void OnCardSelected()
    {
        if (!CanPerformActions() || currentPhase != GamePhase.CardSelection) return;

        currentPhase = GamePhase.UnitSelection;
        UIManager.Instance.ShowHint(CurrentTurn, "�������� �����");
    }

    public void OnUnitSelected()
    {
        if (!CanPerformActions() || currentPhase != GamePhase.UnitSelection) return;

        currentPhase = GamePhase.ActionSelection;
        UIManager.Instance.ShowHint(CurrentTurn, "�������� ��������: ����������� ��� �����");
    }

    public void OnActionSelected()
    {
        if (!CanPerformActions() || currentPhase != GamePhase.ActionSelection) return;
    }

    public void EndTurn()
    {
        
        unitSelector.DeselectUnit();
        tileHighlighter.ClearHighlights();

        // ����������� ������
        CurrentTurn = (CurrentTurn == PlayerTurn.Player1) ? PlayerTurn.Player2 : PlayerTurn.Player1;
        actionsPerformed = 0;

        // ����������� ������� ����� ����� ���������� ���� Player2
        if (CurrentTurn == PlayerTurn.Player1)
        {
            turnCount++;
            if (turnCount > maxTurns)
            {
                EndGame();
                return;
            }
        }

        UIManager.Instance.UpdateTurnInfo(turnCount);
        UIManager.Instance.UpdateActionCount(actionsPerformed);

        // �������� ����� ��� � �������� ����� ������� ����
        StartCoroutine(StartTurnWithDelay());

        Debug.Log($"��� ���������� ��: {CurrentTurn}");
    }

    public void ActionPerformed()
    {
        if (!CanPerformActions()) return;

        actionsPerformed++;
        UIManager.Instance.UpdateActionCount(actionsPerformed);

        if (actionsPerformed >= 1)
        {
            EndTurn();
        }
        else
        {
            
            currentPhase = GamePhase.UnitSelection;
            UIManager.Instance.ShowHint(CurrentTurn, "�������� �����");
        }
    }

    public void RegisterAttack(Unit.UnitOwner owner)
    {
        if (!CanPerformActions()) return;

        if (owner == Unit.UnitOwner.Player1)
        {
            player1SuperCharges++;
            if (player1SuperCharges > 2) player1SuperCharges = 2;
            Debug.Log($"Player1 charges: {player1SuperCharges}");
        }
        else
        {
            player2SuperCharges++;
            if (player2SuperCharges > 2) player2SuperCharges = 2;
            Debug.Log($"Player2 charges: {player2SuperCharges}");
        }

        UIManager.Instance.UpdateSuperMeter(player1SuperCharges, player2SuperCharges);
    }

    public bool CanUseSuper(Unit.UnitOwner owner)
    {
        if (!CanPerformActions()) return false;

        bool canUse = owner == Unit.UnitOwner.Player1 ?
            player1SuperCharges >= 2 :
            player2SuperCharges >= 2;

        Debug.Log($"Can use super: {canUse}");
        return canUse;
    }

    public void UseSuper(Unit.UnitOwner owner)
    {
        if (!CanPerformActions()) return;

        if (owner == Unit.UnitOwner.Player1)
        {
            player1SuperCharges = 0;
        }
        else
        {
            player2SuperCharges = 0;
        }

        UIManager.Instance.UpdateSuperMeter(player1SuperCharges, player2SuperCharges);
        Debug.Log($"Super used by {owner}");
    }

    private void EndGame()
    {
        
        this.enabled = false;

        int p1Health = CalculateTotalHealth(Unit.UnitOwner.Player1);
        int p2Health = CalculateTotalHealth(Unit.UnitOwner.Player2);

        string result;

        if (p1Health <= 0 && p2Health <= 0)
        {
            result = "�����! ��� ����� �������!";
        }
        else if (p1Health <= 0)
        {
            result = "����� 2 ���������! � ������ 1 �� �������� ������!";
        }
        else if (p2Health <= 0)
        {
            result = "����� 1 ���������! � ������ 2 �� �������� ������!";
        }
        else
        {
            
            result = p1Health > p2Health ? "����� 1 ���������!" :
                    p2Health > p1Health ? "����� 2 ���������!" : "�����!";
        }

        if (victoryText != null)
        {
            victoryText.text = result;
            victoryText.gameObject.SetActive(true);
            victoryText.alignment = TextAlignmentOptions.Center;
            victoryText.fontSize = 48;
        }

       
        unitSelector.enabled = false;
        tileHighlighter.enabled = false;
        �ardSystem.enabled = false;

       
        Time.timeScale = 0;

        Debug.Log("���� ���������: " + result);
    }

    public int CalculateTotalHealth(Unit.UnitOwner owner)
    {
        Unit[] units = FindObjectsOfType<Unit>();
        int total = 0;
        foreach (Unit unit in units)
        {
            if (unit.owner == owner && unit.CurrentHealth > 0)
            {
                total += unit.CurrentHealth;
            }
        }
        return total;
    }

    public void CompleteAction()
    {
        if (!CanPerformActions()) return;

        ActionPerformed();
        tileHighlighter.ClearHighlights();
        unitSelector.DeselectUnit();
    }
}