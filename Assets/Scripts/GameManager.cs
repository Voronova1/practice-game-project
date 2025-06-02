using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UIManager uiManager;
    [SerializeField] private TurnManager turnManager;

    void Awake()
    {
        
        if (uiManager == null) uiManager = FindObjectOfType<UIManager>();
        if (turnManager == null) turnManager = FindObjectOfType<TurnManager>();

        if (uiManager == null || turnManager == null)
        {
            Debug.LogError("�� ������� ����������� ��������� �� �����!");
            return;
        }

        InitializeGame();
    }

    private void InitializeGame()
    {
        uiManager.InitializeUI();
        turnManager.StartFirstTurn();

        Debug.Log("���� ������� ����������������");
    }
}