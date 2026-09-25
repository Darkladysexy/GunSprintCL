using GunSprint.Events;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace GunSprint.UI
{
    /// <summary>
    /// Full-screen tap zone that raises the "fire" event. It sits behind the HUD buttons, so tapping
    /// a button never fires the gun, and every finger counts (multi-touch friendly).
    /// Space / gamepad south also fire, for testing in the Editor and on desktop.
    /// </summary>
    public class TapInputArea : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField] private VoidEventChannelSO _firePressed;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                _firePressed.RaiseEvent();
            }
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            Gamepad gamepad = Gamepad.current;
            bool isKeyPressed = keyboard != null && keyboard.spaceKey.wasPressedThisFrame;
            bool isButtonPressed = gamepad != null && gamepad.buttonSouth.wasPressedThisFrame;
            if (isKeyPressed || isButtonPressed)
            {
                _firePressed.RaiseEvent();
            }
        }
    }
}
