using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.EventSystems;

public class UnitSelector : MonoBehaviour
{
    private Unit selectedUnit;
    private Camera mainCamera;

    [Header("Grid Settings")]
    [SerializeField] private float gridSize = 5f;
    [SerializeField] private float moveSpeed = 10f;

    [Header("References")]
    [SerializeField] private TurnManager turnManager;
    [SerializeField] private TileHighlighter tileHighlighter;
    [SerializeField] private ParticleSystem AttackEffect;
    [SerializeField] private ActionMenu actionMenu;

    private bool isSelectingTarget = false;
    private bool isSuperAttack = false;
    private Vector3 moveTargetPosition;


    public Unit SelectedUnit => selectedUnit;

    void Start()
    {
        mainCamera = Camera.main;
        if (turnManager == null) turnManager = FindObjectOfType<TurnManager>();
        if (tileHighlighter == null) tileHighlighter = FindObjectOfType<TileHighlighter>();
    }

    void Update()
    {
        HandleUnitSelection();
        HandleTargetSelection();
    }

    public void DeselectUnit()
    {
        selectedUnit = null;
        tileHighlighter.ClearHighlights();
        isSelectingTarget = false;
        isSuperAttack = false;
    }

    public void StartMovement(Unit unit)
    {
        if (unit == null || unit.owner.ToString() != turnManager.CurrentTurn.ToString())
        {
            Debug.LogWarning("Попытка начать движение с недействительным юнитом");
            return;
        }

        if (selectedUnit != null && selectedUnit != unit)
        {
            tileHighlighter.ClearHighlights();
        }

        selectedUnit = unit;
        tileHighlighter.HighlightTiles(unit.transform.position, unit.moveRange);
        UIManager.Instance.ShowHint(TurnManager.Instance.CurrentTurn, "Выберите клетку для перемещения");
    }

    public void StartAttack(Unit unit)
    {
        selectedUnit = unit;
        isSelectingTarget = true;
        isSuperAttack = false;

        if (unit.unitType == Unit.UnitType.Mage)
        {
            PerformMageAttack();
            return;
        }

        UIManager.Instance.ShowHint(TurnManager.Instance.CurrentTurn, "Выберите цель для атаки");
    }

    public void StartSuperAttack(Unit unit)
    {
        if (!TurnManager.Instance.CanUseSuper(unit.owner))
        {
            UIManager.Instance.ShowHint(TurnManager.Instance.CurrentTurn, "Недостаточно зарядов!");
            return;
        }

        selectedUnit = unit;

        // Для мага выполняем атаку сразу
        if (unit.unitType == Unit.UnitType.Mage)
        {
            PerformMageSuperAttack();
            return;
        }

        isSelectingTarget = true;
        isSuperAttack = true;
        UIManager.Instance.ShowHint(TurnManager.Instance.CurrentTurn, "Выберите цель для супер-атаки");
    }

    private void PerformMageSuperAttack()
    {
        bool enemiesFound = false;
        Vector3 magePos = selectedUnit.transform.position;
        int centerX = Mathf.RoundToInt(magePos.x / 5) * 5;
        int centerZ = Mathf.RoundToInt(magePos.z / 5) * 5;

        for (int xOffset = -1; xOffset <= 1; xOffset++)
        {
            for (int zOffset = -1; zOffset <= 1; zOffset++)
            {
                if (xOffset == 0 && zOffset == 0) continue;

                int targetX = centerX + xOffset * 5;
                int targetZ = centerZ + zOffset * 5;

                Collider[] colliders = Physics.OverlapBox(
                    new Vector3(targetX, 0, targetZ),
                    new Vector3(2.5f, 2.5f, 2.5f)
                );

                foreach (var collider in colliders)
                {
                    Unit enemy = collider.GetComponent<Unit>();
                    if (enemy != null && enemy.owner != selectedUnit.owner)
                    {
                        Instantiate(AttackEffect, enemy.transform.position, Quaternion.identity);
                        // Увеличиваем урон в 2 раза для супер-атаки
                        enemy.TakeDamage(selectedUnit.damage * 2);
                        enemiesFound = true;
                    }
                }
            }
        }

        if (enemiesFound)
        {
            TurnManager.Instance.RegisterAttack(selectedUnit.owner);
            TurnManager.Instance.UseSuper(selectedUnit.owner); // Используем супер-атаку
            TurnManager.Instance.ActionPerformed();
        }
        else
        {
            UIManager.Instance.ShowHint(TurnManager.Instance.CurrentTurn, "Нет врагов рядом!");
        }

        DeselectUnit();
    }

    private void PerformMageAttack()
    {
        bool enemiesFound = false;
        Vector3 magePos = selectedUnit.transform.position;
        int centerX = Mathf.RoundToInt(magePos.x / 5) * 5;
        int centerZ = Mathf.RoundToInt(magePos.z / 5) * 5;

        for (int xOffset = -1; xOffset <= 1; xOffset++)
        {
            for (int zOffset = -1; zOffset <= 1; zOffset++)
            {
                if (xOffset == 0 && zOffset == 0) continue;

                int targetX = centerX + xOffset * 5;
                int targetZ = centerZ + zOffset * 5;

                Collider[] colliders = Physics.OverlapBox(
                    new Vector3(targetX, 0, targetZ),
                    new Vector3(2.5f, 2.5f, 2.5f)
                );

                foreach (var collider in colliders)
                {
                    Unit enemy = collider.GetComponent<Unit>();
                    if (enemy != null && enemy.owner != selectedUnit.owner)
                    {
                        Instantiate(AttackEffect, enemy.transform.position, Quaternion.identity);
                        enemy.TakeDamage(selectedUnit.damage);
                        enemiesFound = true;
                    }
                }
            }
        }

        if (enemiesFound)
        {
            TurnManager.Instance.RegisterAttack(selectedUnit.owner);
            TurnManager.Instance.ActionPerformed();
        }
        else
        {
            UIManager.Instance.ShowHint(TurnManager.Instance.CurrentTurn, "Нет врагов рядом!");
        }

        DeselectUnit();
    }

    private void HandleUnitSelection()
    {
       
        if (EventSystem.current.IsPointerOverGameObject()) return;

        if (isSelectingTarget) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Unit unit = hit.collider.GetComponent<Unit>();
                if (unit != null && unit.owner.ToString() == turnManager.CurrentTurn.ToString())
                {
                    selectedUnit = unit;
                    actionMenu.ShowMenu(unit, Mouse.current.position.ReadValue());
                }
                else
                {
                    
                    actionMenu.HideMenu();

                    if (selectedUnit != null && hit.collider.CompareTag("Tile"))
                    {
                        moveTargetPosition = GetTileCenterPosition(hit.point);
                        int distance = CalculateGridDistance(selectedUnit.transform.position, moveTargetPosition);

                        if (distance <= selectedUnit.moveRange)
                        {
                            StartCoroutine(MoveUnitCoroutine(moveTargetPosition));
                            TurnManager.Instance.ActionPerformed();
                        }
                        else
                        {
                            UIManager.Instance.ShowHint(TurnManager.Instance.CurrentTurn, "Слишком далеко!");
                        }
                    }
                }
            }
            else
            {
                
                actionMenu.HideMenu();
            }
        }
    }

    private void HandleTargetSelection()
    {
        if (!isSelectingTarget) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
           
            if (EventSystem.current.IsPointerOverGameObject()) return;

            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                
                Unit clickedUnit = hit.collider.GetComponent<Unit>();
                if (clickedUnit != null && clickedUnit.owner == selectedUnit.owner)
                {
                    
                    actionMenu.ShowMenu(clickedUnit, Mouse.current.position.ReadValue());
                    isSelectingTarget = false;
                    isSuperAttack = false;
                    return;
                }

              
                Unit target = hit.collider.GetComponent<Unit>();
                if (target != null && target.owner != selectedUnit.owner)
                {
                    if (selectedUnit.CanAttack(target.transform.position))
                    {
                        if (isSuperAttack)
                            PerformSuperAttack(target);
                        else
                            PerformRegularAttack(target);
                    }
                    else
                    {
                        UIManager.Instance.ShowHint(TurnManager.Instance.CurrentTurn, "Цель вне досягаемости!");
                       
                    }
                }
                
            }
        }
    }


    private void PerformRegularAttack(Unit target)
    {
        switch (selectedUnit.unitType)
        {
            case Unit.UnitType.Archer:
            case Unit.UnitType.Warrior:
                target.TakeDamage(selectedUnit.damage);
                break;
        }

        Instantiate(AttackEffect, target.transform.position, Quaternion.identity);
        TurnManager.Instance.RegisterAttack(selectedUnit.owner);
        TurnManager.Instance.ActionPerformed();
        DeselectUnit();
    }

    private void PerformSuperAttack(Unit target)
    {
        int superDamage = selectedUnit.damage * 2;
        target.TakeDamage(superDamage);
        Instantiate(AttackEffect, target.transform.position, Quaternion.identity);
        TurnManager.Instance.UseSuper(selectedUnit.owner);
        TurnManager.Instance.ActionPerformed();
        DeselectUnit();
    }

    private Vector3 GetTileCenterPosition(Vector3 worldPosition)
    {
        return new Vector3(
            Mathf.Round(worldPosition.x / gridSize) * gridSize,
            0,
            Mathf.Round(worldPosition.z / gridSize) * gridSize
        );
    }

    private int CalculateGridDistance(Vector3 from, Vector3 to)
    {
        int fromX = Mathf.RoundToInt(from.x / gridSize);
        int fromZ = Mathf.RoundToInt(from.z / gridSize);
        int toX = Mathf.RoundToInt(to.x / gridSize);
        int toZ = Mathf.RoundToInt(to.z / gridSize);

        return Mathf.Max(
            Mathf.Abs(fromX - toX),
            Mathf.Abs(fromZ - toZ)
        );
    }

    private IEnumerator MoveUnitCoroutine(Vector3 targetPos)
    {
        
        Unit unit = selectedUnit;
        if (unit == null)
            yield break;

        Transform unitTransform = unit.transform;
        Vector3 startPos = unitTransform.position;
        targetPos.y = startPos.y;

        while (Vector3.Distance(unitTransform.position, targetPos) > 0.1f)
        {
            
            if (unit == null)
                yield break;

            unitTransform.position = Vector3.MoveTowards(
                unitTransform.position,
                targetPos,
                moveSpeed * Time.deltaTime
            );
            yield return null;
        }

        
        if (unit != null)
        {
            unitTransform.position = targetPos;
            tileHighlighter.ClearHighlights();
        }
    }
}