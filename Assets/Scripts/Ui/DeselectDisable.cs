using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DeselectDisable : MonoBehaviour
{
    public GameObject defaultSelect;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        if (EventSystem.current.currentSelectedGameObject == null || !EventSystem.current.currentSelectedGameObject.activeInHierarchy)
        {
            var eventSystem = EventSystem.current;
            var target = defaultSelect ?? eventSystem.firstSelectedGameObject;
            eventSystem.SetSelectedGameObject( target, new BaseEventData(eventSystem));
        }
    }
}

