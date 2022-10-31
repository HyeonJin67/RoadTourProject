using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GoToPopup : MonoBehaviour
{
    [SerializeField]
    Button _gotoURLButton;

    public void Initialize(Action onComplete)
    {
        if(_gotoURLButton != null)
            _gotoURLButton.onClick.AddListener(()=> 
            {
                Application.OpenURL("https://assetstore.unity.com/packages/templates/systems/simple-3d-tilemap-editor-134189");
                gameObject.SetActive(false);

                if(onComplete != null)
                    onComplete();
            });
    }
}
