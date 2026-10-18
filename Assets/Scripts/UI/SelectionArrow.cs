using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectionArrow : MonoBehaviour
{
    [SerializeField] private RectTransform[] buttons;
    [SerializeField] private AudioClip changeSound;
    private RectTransform arrow;
    private int currentPosition;

    private void Awake()
    {
        arrow = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        currentPosition = 0;
        AssignPosition();
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(buttons[0].gameObject);
    }

    private void Update()
    {
        GameObject selected = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i].gameObject == selected && currentPosition != i)
            {
                currentPosition = i;
                AssignPosition();
                if (SoundManager.instance != null && changeSound != null)
                    SoundManager.instance.PlaySound(changeSound);
                break;
            }
        }

        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.KeypadEnter))
            buttons[currentPosition].GetComponent<Button>().onClick.Invoke();
    }

    private void AssignPosition()
    {
        if (arrow == null)
            arrow = GetComponent<RectTransform>();
        arrow.position = new Vector3(arrow.position.x, buttons[currentPosition].position.y, arrow.position.z);
    }
}
