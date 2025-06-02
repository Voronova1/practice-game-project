using UnityEngine;

public class Unit : MonoBehaviour
{
    public enum UnitOwner { Player1, Player2 }
    public enum UnitType { Warrior, Archer, Mage }

    [Header("Basic Settings")]
    public UnitOwner owner;
    public UnitType unitType;
    public int moveRange = 1;

    [Header("Combat Stats")]
    public int attackRange;
    public int damage;
    [SerializeField] private int _baseHealth = 100;
    [SerializeField] private int _maxHealth = 100;

    [SerializeField] private int _currentHealth;
    public int CurrentHealth => _currentHealth;
    public int MaxHealth { get; private set; }

    void Start()
    {
        InitializeUnitStats();
        MaxHealth = _baseHealth;
        _currentHealth = MaxHealth;
    }

    private void InitializeUnitStats()
    {
        switch (unitType)
        {
            case UnitType.Archer:
                attackRange = 3; // Лучник - дальний бой
                damage = 15;
                break;

            case UnitType.Mage:
                attackRange = 1; // Маг - радиусный урон
                damage = 25;
                break;

            default: // Warrior
                attackRange = 1; // Мечник - ближний бой
                damage = 30;
                break;
        }
        _maxHealth = _baseHealth;
    }

    public void TakeDamage(int amount)
    {
        _currentHealth = Mathf.Clamp(_currentHealth - amount, 0, MaxHealth);

        // Обновление UI здоровья - добавляем проверку на null
        if (TurnManager.Instance != null && UIManager.Instance != null)
        {
            int p1Health = TurnManager.Instance.CalculateTotalHealth(UnitOwner.Player1);
            int p2Health = TurnManager.Instance.CalculateTotalHealth(UnitOwner.Player2);
            UIManager.Instance.UpdateHealthDisplay(p1Health, p2Health);
        }

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // Обновляем здоровье перед уничтожением объекта
        if (TurnManager.Instance != null && UIManager.Instance != null)
        {
            int p1Health = TurnManager.Instance.CalculateTotalHealth(UnitOwner.Player1);
            int p2Health = TurnManager.Instance.CalculateTotalHealth(UnitOwner.Player2);
            UIManager.Instance.UpdateHealthDisplay(p1Health, p2Health);
        }

        Destroy(gameObject);
    }

    public bool CanAttack(Vector3 targetPosition)
    {
        float distance = Vector3.Distance(
            new Vector3(transform.position.x, 0, transform.position.z),
            new Vector3(targetPosition.x, 0, targetPosition.z)
        );

        return distance <= attackRange * 5f; // 5 - размер клетки
    }
}