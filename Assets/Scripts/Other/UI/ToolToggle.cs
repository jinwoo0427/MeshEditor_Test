using System;
using UnityEngine;
using UnityEngine.UI;
using XDPaint.Controllers;
using XDPaint.Core;

namespace XDPaint.Demo.UI
{
    public class ToolToggle : MonoBehaviour
    {
        public event Action<PaintTool> OnToolSwitched;

        [SerializeField] private PaintTool tool;
        [SerializeField] private Toggle toggle;
        private PaintManager paintManager;

        public Toggle Toggle => toggle;
        public PaintTool Tool => tool;

        private void OnEnable()
        {
            toggle.onValueChanged.AddListener(OnToggle);
        }

        private void OnDisable()
        {
            toggle.onValueChanged.RemoveListener(OnToggle);
        }

        private void OnToggle(bool isOn)
        {
            if (!isOn) 
                return;

            if (PaintController.Instance.UseSharedSettings)
            {
                PaintController.Instance.Tool = tool;
            }
            else if (paintManager != null)
            {
                paintManager.Tool = tool;
            }
            PlayerPrefs.SetInt("XDPaintDemoTool", (int)tool);
            OnToolSwitched?.Invoke(tool);
        }

        public void SetPaintManager(PaintManager paintManagerInstance)
        {
            paintManager = paintManagerInstance;
        }
    }
}