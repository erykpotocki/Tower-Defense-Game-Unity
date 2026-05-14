using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ShopHoldToRepeatButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [Header("Hold Settings")]
    [SerializeField] private bool triggerOnceImmediately = true;
    [SerializeField] private float holdStartDelay = 0.45f;
    [SerializeField] private float repeatInterval = 0.12f;

    public event Action OnPressedOrRepeated;

    private Button button;
    private Coroutine holdCoroutine;
    private bool isHolding;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!CanUseButton())
            return;

        isHolding = true;

        if (triggerOnceImmediately)
            TriggerAction();

        holdCoroutine = StartCoroutine(HoldRoutine());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        StopHolding();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StopHolding();
    }

    private IEnumerator HoldRoutine()
    {
        yield return new WaitForSeconds(holdStartDelay);

        while (isHolding && CanUseButton())
        {
            TriggerAction();
            yield return new WaitForSeconds(repeatInterval);
        }

        StopHolding();
    }

    private void TriggerAction()
    {
        if (!CanUseButton())
        {
            StopHolding();
            return;
        }

        OnPressedOrRepeated?.Invoke();
    }

    private bool CanUseButton()
    {
        return button != null && button.interactable;
    }

    private void StopHolding()
    {
        isHolding = false;

        if (holdCoroutine != null)
        {
            StopCoroutine(holdCoroutine);
            holdCoroutine = null;
        }
    }

    private void OnDisable()
    {
        StopHolding();
    }
}