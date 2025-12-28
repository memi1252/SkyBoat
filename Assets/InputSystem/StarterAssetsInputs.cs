using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
	public class StarterAssetsInputs : MonoBehaviour
	{
		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool sprint;
		public bool quickSlot1;
		public bool quickSlot2;
		public bool quickSlot3;
		public bool quickSlot4;
		public bool attack;
		public bool interaction;
		public bool drop;


		[Header("Movement Settings")]
		public bool analogMovement;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;

#if ENABLE_INPUT_SYSTEM
		public void OnMove(InputValue value)
		{
			MoveInput(value.Get<Vector2>());
		}

		public void OnLook(InputValue value)
		{
			if(cursorInputForLook)
			{
				LookInput(value.Get<Vector2>());
			}
		}

		public void OnJump(InputValue value)
		{
			JumpInput(value.isPressed);
		}

		public void OnSprint(InputValue value)
		{
			SprintInput(value.isPressed);
		}

		public void OnQuickSlot1(InputValue value)
		{
			QuickSlot1Input(value.isPressed);
		}
		
		public void OnQuickSlot2(InputValue value)
		{
			QuickSlot2Input(value.isPressed);
		}
		
		public void OnQuickSlot3(InputValue value)
		{
			QuickSlot3Input(value.isPressed);
		}
		
		public void OnQuickSlot4(InputValue value)
		{
			QuickSlot4Input(value.isPressed);
		}
		
		public void OnAttack(InputValue value)
		{
			AttackInput(value.isPressed);
		}

		public void OnInteract(InputValue value)
		{
			InteractInput(value.isPressed);
		}

		public void OnDrop(InputValue value)
		{
			DropInput(value.isPressed);
		}
#endif


		public void MoveInput(Vector2 newMoveDirection)
		{
			move = newMoveDirection;
		} 

		public void LookInput(Vector2 newLookDirection)
		{
			look = newLookDirection;
		}

		public void JumpInput(bool newJumpState)
		{
			jump = newJumpState;
		}

		public void SprintInput(bool newSprintState)
		{
			sprint = newSprintState;
		}

		private void QuickSlot1Input(bool newQuickSlot1State)
		{
			quickSlot1 = newQuickSlot1State;
		}

		private void QuickSlot2Input(bool newQuickSlot2State)
		{
			quickSlot2 = newQuickSlot2State;
		}

		private void QuickSlot3Input(bool newQuickSlot3State)
		{
			quickSlot3 = newQuickSlot3State;
		}

		private void QuickSlot4Input(bool newQuickSlot4State)
		{
			quickSlot4 = newQuickSlot4State;
		}
		
		private void AttackInput(bool newAttackState)
		{
			attack = newAttackState;
		}

		private void InteractInput(bool newInteractState)
		{
			interaction = newInteractState;
		}

		private void DropInput(bool newDropState)
		{
			drop = newDropState;
		}


		private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(cursorLocked);
		}
		

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}
	}
	
}