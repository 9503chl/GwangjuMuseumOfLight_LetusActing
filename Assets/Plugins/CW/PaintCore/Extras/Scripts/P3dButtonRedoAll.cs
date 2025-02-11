using UnityEngine;
using UnityEngine.EventSystems;
using CW.Common;
using UnityEngine.UI;

namespace PaintIn3D
{
	/// <summary>This component allows you to perform the Redo All action. This can be done by attaching it to a clickable object, or manually from the RedoAll method.</summary>
	[HelpURL(P3dCommon.HelpUrlPrefix + "P3dButtonRedoAll")]
	[AddComponentMenu(P3dCommon.ComponentMenuPrefix + "Button Redo All")]
	public class P3dButtonRedoAll : MonoBehaviour
	{
		private Button button;

		/// <summary>If you want to manually trigger RedoAll, then call this function.</summary>
		[ContextMenu("Redo All")]
		public void RedoAll()
		{
			P3dStateManager.RedoAll();
		}
        private void Start()
        {
            button = GetComponent<Button>();
			button.onClick.AddListener(RedoAll);
        }
        protected virtual void Update()
		{
			if (button != null)
			{
                button.interactable = P3dStateManager.CanRedo == true ? true : false;
			}
		}
	}
}

#if UNITY_EDITOR
namespace PaintIn3D
{
	using UnityEditor;
	using TARGET = P3dButtonRedoAll;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class P3dRedoAll_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Info("This component allows you to perform the Redo All action. This can be done by attaching it to a clickable object, or manually from the RedoAll method.");
		}
	}
}
#endif