using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using MText.EditorHelper;
#endif

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace MText
{
    [ExecuteAlways]
    public class AutoUpdateInputSystemToSampleScene : MonoBehaviour
    {
#if UNITY_EDITOR
        void Awake()
        {
#if ENABLE_INPUT_SYSTEM
            if (!gameObject.GetComponent<PlayerInput>())
                gameObject.AddComponent<PlayerInput>();
            MText_Settings settings = MText_FindResource.VerifySettings(null);

            if (settings)
                gameObject.GetComponent<PlayerInput>().actions = settings.inputActionAsset;
#endif
        }
#endif
    }
}