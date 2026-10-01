using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class EventSystemController : MonoBehaviour
{
    public EventSystem e;
    public InputSystemUIInputModule inputModule;
    public PanelObject[] Panels;

    public InputActionAsset target;

    public GameObject lastSelectedObject;
    private GameObject currentSelection;

    private void Awake()
    {
        inputModule = GetComponent<InputSystemUIInputModule>();
    }

    private void Update()
    {
        if (EventSystem.current == null) return;

        if(inputModule.actionsAsset != target) inputModule.actionsAsset = target;

        currentSelection = EventSystem.current.currentSelectedGameObject;

        if (currentSelection != null && currentSelection != lastSelectedObject)
        {
            if(currentSelection.TryGetComponent<MapSaveWipeButton>(out MapSaveWipeButton mswb) 
                || currentSelection.TryGetComponent<ClassicBtnAction>(out ClassicBtnAction cba) 
                || currentSelection.TryGetComponent<MapParent>(out MapParent mp))
            {
                lastSelectedObject = currentSelection;
            }
        }
    }


    private void OnValidate()
    {
        if (Panels == null) return;

        foreach (var panel in Panels)
        {
            if (panel.Panel == null) continue;
            panel.PanelName = panel.Panel.name;
        }
    }

    public void SetPanel(string panelName)
    {
        return;
        Debug.Log(panelName + " tried to set");
        foreach (var panel in Panels)
        {
            if(panel.PanelName == panelName)
            {
                e.SetSelectedGameObject(panel.PanelThatWithStart);
                return;
            }
        }
    }

    public GameObject GetLastSelected()
    {
        return lastSelectedObject;
    }

    public void DisableUIInput()
    {
        inputModule.enabled = false;
    }
    public void EnableUIInput()
    {
        inputModule.enabled = true;
    }

}


[System.Serializable]
public class PanelObject
{
    public string PanelName;
    public GameObject Panel;
    public GameObject PanelThatWithStart;
    public PanelObject() { }
}