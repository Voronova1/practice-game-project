using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ActionMenu : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private Button moveButton;
    [SerializeField] private Button attackButton;
    [SerializeField] private Button superButton;

    private UnitSelector unitSelector;
    private bool isPointerOverMenu = false;

    void Awake()
    {
        unitSelector = FindObjectOfType<UnitSelector>();
        menuPanel.SetActive(false);

        moveButton.onClick.AddListener(OnMoveClicked);
        attackButton.onClick.AddListener(OnAttackClicked);
        superButton.onClick.AddListener(OnSuperClicked);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOverMenu = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOverMenu = false;
    }

    public bool IsPointerOverMenu() => isPointerOverMenu;

    public void ShowMenu(Unit unit, Vector2 screenPosition)
    {
        menuPanel.transform.position = screenPosition;
        menuPanel.SetActive(true);
        superButton.interactable = TurnManager.Instance.CanUseSuper(unit.owner);
    }

    public void HideMenu()
    {
        menuPanel.SetActive(false);
    }

    private void OnMoveClicked()
    {
        unitSelector.StartMovement(unitSelector.SelectedUnit);
        HideMenu();
    }

    private void OnAttackClicked()
    {
        unitSelector.StartAttack(unitSelector.SelectedUnit);
        HideMenu();
    }

    private void OnSuperClicked()
    {
        unitSelector.StartSuperAttack(unitSelector.SelectedUnit);
        HideMenu();
    }
}