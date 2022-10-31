using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace MText
{
    /// <summary>
    /// This component is used to cast a ray from camera to interact with 3D UI Elements
    /// </summary>


    [DisallowMultipleComponent]
    [AddComponentMenu("Modular 3D Text/Raycast Selector (M3D)")]
    public class MText_UI_RaycastSelector : MonoBehaviour
    {
        #region Variable Declaration

        #region Raycast settings
        [Tooltip("If not assigned, it will automatically get Camera.main on Start")]
        public Camera myCamera;
        [SerializeField] LayerMask UILayer = ~0;
        [SerializeField] float maxDistance = 5000;
        #endregion Raycast settings


        [Space]
        #region Behavior Settings
        [Tooltip("True = How normal UI works. It toggles if clicking a inputfield enables it " +
            "and clicking somewhere else disables it")]
        public bool onlyOneTargetFocusedAtOnce = true;

        [Tooltip("Unhovering mouse from a Btn will unselect it")]
        public bool unselectBtnOnUnhover = true;
        #endregion Behavior Settings


        private Transform currentTarget = null;
        private Transform clickedTarget = null;
        private bool dragging = false;

        #endregion Variable Declaration



        #region Unity Things
        private void Start()
        {
            //If no camera assigned, get Camera.main
            if (!myCamera)
                myCamera = Camera.main;
        }

        private void Update()
        {
            if (!myCamera)
                return;

            //If Already dragging stuff, do dragging stuff
            if (dragging)
            {
                Dragging(currentTarget);
                DetectDragEnd();
            }
            else
            {
                //Check if mouse is on something
                Transform pointerOnUI = RaycastCheck();

                //If mouse not on the old UI, unselect old one
                if (pointerOnUI != currentTarget)
                    UnSelectOldTarget(currentTarget);


                //If mouse on a UI
                if (pointerOnUI)
                {
                    //If it's a new target, select that
                    if (pointerOnUI != currentTarget)
                        SelectNewTarget(pointerOnUI);

#if ENABLE_INPUT_SYSTEM
                    if (Mouse.current.leftButton.wasPressedThisFrame)
#else
                    if (Input.GetMouseButtonDown(0))
#endif
                    //If the UI is clicked 
                    {
                        PressTarget(pointerOnUI);
                        dragging = true;
                    }
                }
                currentTarget = pointerOnUI;
            }
        }
        #endregion Unity Things



        private Transform RaycastCheck()
        {
#if ENABLE_INPUT_SYSTEM
            Ray ray = myCamera.ScreenPointToRay(Pointer.current.position.ReadValue());
#else
            Ray ray = myCamera.ScreenPointToRay(Input.mousePosition);
#endif

            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, UILayer))
                return hit.transform;

            return null;
        }



        void PressTarget(Transform hit)
        {
            if (onlyOneTargetFocusedAtOnce)
                UnFocusPreviouslySelectedItems(hit);

            PressInputString(hit);
            PressButton(hit);
            PressSlider(hit);
        }
        void PressInputString(Transform hit)
        {
            MText_UI_InputField inputString = hit.gameObject.GetComponent<MText_UI_InputField>();
            if (!InteractWithInputString(inputString))
                return;

            inputString.Select();
            clickedTarget = hit;
        }
        void PressSlider(Transform hit)
        {
            MText_UI_SliderHandle sliderHandle = hit.gameObject.GetComponent<MText_UI_SliderHandle>();
            if (!InteractWithSlider(sliderHandle))
                return;

            hit.gameObject.GetComponent<MText_UI_SliderHandle>().slider.ClickedVisual();
        }
        void PressButton(Transform hit)
        {
            MText_UI_Button button = hit.gameObject.GetComponent<MText_UI_Button>();
            if (!InteractWithButton(button))
                return;

            button.PressButtonClick();
        }


        void UnFocusPreviouslySelectedItems(Transform hit)
        {
            if (hit != clickedTarget)
            {
                if (clickedTarget)
                {
                    if (clickedTarget.gameObject.GetComponent<MText_UI_InputField>())
                    {
                        if (clickedTarget.gameObject.GetComponent<MText_UI_InputField>().interactable)
                        {
                            clickedTarget.gameObject.GetComponent<MText_UI_InputField>().Focus(false);
                        }
                    }
                }
            }
        }


        void SelectNewTarget(Transform hit)
        {
            SelectButton(hit);
            SelectSlider(hit);
        }
        void SelectSlider(Transform hit)
        {
            MText_UI_SliderHandle sliderHandle = hit.gameObject.GetComponent<MText_UI_SliderHandle>();
            if (!InteractWithSlider(sliderHandle))
                return;

            sliderHandle.slider.SelectedVisual();
        }
        void SelectButton(Transform hit)
        {
            MText_UI_Button button = hit.gameObject.GetComponent<MText_UI_Button>();
            if (!InteractWithButton(button))
                return;

            button.SelectButton();
        }



#region Unselect
        private void UnSelectOldTarget(Transform hit)
        {
            if (!hit)
                return;

            UnselectButton(hit);
            UnselectSlider(hit);
        }
        private void UnselectSlider(Transform hit)
        {
            MText_UI_SliderHandle sliderHandle = hit.gameObject.GetComponent<MText_UI_SliderHandle>();
            if (!InteractWithSlider(sliderHandle))
                return;

            sliderHandle.slider.UnSelectedVisual();
        }
        private void UnselectButton(Transform hit)
        {
            MText_UI_Button button = hit.gameObject.GetComponent<MText_UI_Button>();
            if (!InteractWithButton(button))
                return;

            if (unselectBtnOnUnhover)
            {
                MText_UI_List list = MText_Utilities.GetParentList(button.transform);
                if (!list)
                    button.UnselectButton();
                else
                    list.UnselectEverythingDontChangeSelectedItemValue();
            }
            else
            {
                //button.UnselectButton();
            }
        }
#endregion Unselect


#region Drag

#region Dragging
        private void Dragging(Transform hit)
        {
            DragSlider(hit);
            DragButton(hit);
        }
        private void DragSlider(Transform hit)
        {
            MText_UI_SliderHandle sliderHandle = hit.gameObject.GetComponent<MText_UI_SliderHandle>();
            if (!InteractWithSlider(sliderHandle))
                return;

            //Get the screen position of the slider handle
            Vector3 screenPoint = myCamera.WorldToScreenPoint(hit.position);
#if ENABLE_INPUT_SYSTEM
            //Get the mouse position on screen
            Vector3 cursorScreenPoint = new Vector3(Pointer.current.position.ReadValue().x, Pointer.current.position.ReadValue().y, screenPoint.z);
#else
            //Get the mouse position on screen
            Vector3 cursorScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
#endif
            //Convert cursor position to world position
            Vector3 cursorPosition = myCamera.ScreenToWorldPoint(cursorScreenPoint);
            //cursorPosition in slider handle's local space
            Vector3 localPosition = hit.parent.InverseTransformPoint(cursorPosition); //used to be hit.inverseTransformPoint

            //Remove Y Z position from handle
            localPosition = new Vector3(localPosition.x, 0, 0);

            float size = sliderHandle.slider.backgroundSize;
            localPosition.x = Mathf.Clamp(localPosition.x, -size / 2, size / 2);

            hit.localPosition = localPosition;

            sliderHandle.slider.GetCurrentValueFromHandle();
            sliderHandle.slider.ValueChanged();
        }
        private void DragButton(Transform hit)
        {
            MText_UI_Button button = hit.gameObject.GetComponent<MText_UI_Button>();
            if (!InteractWithButton(button))
                return;
            button.ButtonBeingPressed();
        }

#endregion Dragging

#region Drag End
        private void DetectDragEnd()
        {
            if (Input.touchCount > 0)
            {
#if ENABLE_INPUT_SYSTEM
                if (Input.touches[0].phase == UnityEngine.TouchPhase.Ended)
#else
                if (Input.touches[0].phase == TouchPhase.Ended)
#endif
                {
                    dragging = false;
                    DragEnded(currentTarget, RaycastCheck());
                }
            }
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current.leftButton.wasReleasedThisFrame && dragging)
#else
            if (Input.GetMouseButtonUp(0) && dragging)
#endif
            {
                dragging = false;
                DragEnded(currentTarget, RaycastCheck());
            }
        }
        private void DragEnded(Transform hit, Transform currentTarget)
        {
            DragEndOnSlider(hit);
            DragEndOnButton(hit, currentTarget);
        }
        private void DragEndOnSlider(Transform hit)
        {
            MText_UI_SliderHandle sliderHandle = hit.gameObject.GetComponent<MText_UI_SliderHandle>();
            if (!InteractWithSlider(sliderHandle))
                return;

            sliderHandle.slider.ValueChangeEnded();
        }
        private void DragEndOnButton(Transform hit, Transform currentTarget)
        {
            MText_UI_Button button = hit.gameObject.GetComponent<MText_UI_Button>();
            if (!InteractWithButton(button))
                return;

            if (currentTarget != hit) button.isSelected = false;
            button.PressedButtonClickStopped();
        }
#endregion Drag End

#endregion Drag



#region CanItIteractWithIt
        private bool InteractWithButton(MText_UI_Button button)
        {
            if (!button)
                return false;
            if (button.interactable && button.interactableByMouse)
                return true;

            return false;
        }
        private bool InteractWithSlider(MText_UI_SliderHandle sliderHandle)
        {
            if (!sliderHandle)
                return false;
            if (sliderHandle.slider && sliderHandle.slider.interactable)
                return true;

            return false;
        }
        private bool InteractWithInputString(MText_UI_InputField inputString)
        {
            if (!inputString)
                return false;
            if (inputString.interactable)
                return true;

            return false;
        }
#endregion CanItIteractWithIt
    }
}