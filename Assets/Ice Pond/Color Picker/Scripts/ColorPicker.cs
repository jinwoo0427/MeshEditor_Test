using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
#if COLORPICKER_TMPRO
using TMPro;
using CPInputField = TMPro.TMP_InputField;
#else
using CPInputField = UnityEngine.UI.InputField;
#endif

namespace ColorPickerUtil
{
    [System.Serializable]
    public class ColorPickerColorEvent : UnityEvent<Color> { }
    [System.Serializable]
    public class ColorPickerFloatEvent : UnityEvent<float> { }
    [System.Serializable]
    public class ColorPickerVector2Event : UnityEvent<Vector2> { }

    public class ColorPicker : MonoBehaviour
    {
        public enum Mode {
            HSV_H, HSV_S, HSV_V,
            RGB_R, RGB_G, RGB_B,
            Lab_L, Lab_a, Lab_b
        }

        [SerializeField] Mode mode;
        [SerializeField] Image newColorImage;
        [SerializeField] Image currentColorImage;
        [Header("Picking Components")]
        [SerializeField] PickArea pickArea;
        [SerializeField] PickBar pickBar;
        [SerializeField] Slider alphaSlider;
        [SerializeField] Eyedropper eyedropper;
        [SerializeField] ColorSwatch colorSwatch;
        [Header("Input Fields")]
        [SerializeField] CPInputField input_HSV_H;
        [SerializeField] CPInputField input_HSV_S;
        [SerializeField] CPInputField input_HSV_V;
        [SerializeField] CPInputField input_RGB_R;
        [SerializeField] CPInputField input_RGB_G;
        [SerializeField] CPInputField input_RGB_B;
        [SerializeField] CPInputField input_Lab_L;
        [SerializeField] CPInputField input_Lab_a;
        [SerializeField] CPInputField input_Lab_b;
        [SerializeField] CPInputField input_CMYK_C;
        [SerializeField] CPInputField input_CMYK_M;
        [SerializeField] CPInputField input_CMYK_Y;
        [SerializeField] CPInputField input_CMYK_K;
        [SerializeField] CPInputField input_Hex;
        [SerializeField] CPInputField input_Alpha;
        [Header("Mode Toggles")]
        [SerializeField] Toggle toggle_HSV_H;
        [SerializeField] Toggle toggle_HSV_S;
        [SerializeField] Toggle toggle_HSV_V;
        [SerializeField] Toggle toggle_RGB_R;
        [SerializeField] Toggle toggle_RGB_G;
        [SerializeField] Toggle toggle_RGB_B;
        [SerializeField] Toggle toggle_Lab_L;
        [SerializeField] Toggle toggle_Lab_a;
        [SerializeField] Toggle toggle_Lab_b;
        [Header("End Pick Events")]
        [SerializeField] Button endPickButton;
        public ColorPickerColorEvent onEndPick;
        [SerializeField] Button cancelButton;
        public UnityEvent onCancel;

        bool inputFieldLock = false;
        bool pickBarLock = false;
        bool pickAreaLock = false;
        float m_alpha = 1.0f;

        private ColorHSV m_newColorHSV;
        public ColorHSV newColorHSV
        {
            set { SetNewColorHSV(value); }
            get { return new ColorHSV(m_newColorHSV.h, m_newColorHSV.s, m_newColorHSV.v, m_alpha); }
        }

        private ColorLab m_newColorLab;
        public ColorLab newColorLab
        {
            set { SetNewColorLab(value); }
            get { return new ColorLab(m_newColorLab.L, m_newColorLab.a, m_newColorLab.b, m_alpha); }
        }

        private ColorHex m_newColorHex;
        public ColorHex newColorHex
        {
            set { SetNewColorHex(value); }
            get { return new ColorHex(m_newColorHex.hex, m_alpha); }
        }

        private ColorCMYK m_newColorCMYK;
        public ColorCMYK newColorCMYK
        {
            set { SetNewColorCMYK(value); }
            get { return new ColorCMYK(m_newColorCMYK.c, m_newColorCMYK.m, m_newColorCMYK.y, m_newColorCMYK.k, m_alpha); }
        }

        private Color m_newColor;
        public Color newColor
        {
            set { SetNewColor(value); }
            get { return new Color(m_newColor.r, m_newColor.g, m_newColor.b, m_alpha); }
        }

        private Color m_currentColor;
        public Color currentColor
        {
            set { m_currentColor = value; SetNewColor(value); }
            get { return m_currentColor; }
        }

        private void Awake()
        {
            Initialize();
        }

        void Initialize()
        {
            if (endPickButton != null)
            {
                endPickButton.onClick.AddListener(() => { onEndPick.Invoke(newColor); });
            }
            if (cancelButton != null)
            {
                cancelButton.onClick.AddListener(() => { onCancel.Invoke(); });
            }

            //picking components
            if (pickBar != null && pickArea != null)
            {
                pickBar.mode = mode;
                pickArea.mode = mode;
                pickBar.Initialize();
                pickArea.Initialize();
                pickBar.onValueChanged.AddListener(OnPickBarChanged);
                pickArea.onValueChanged.AddListener(OnPickAreaChanged);
            }
            if (alphaSlider != null)
            {
                alphaSlider.onValueChanged.AddListener(OnAlphaSliderChanged);
            }
            if (eyedropper != null)
            {
                eyedropper.onValueChanged.AddListener(OnEyedropperChanged);
                eyedropper.onEndSampling.AddListener(SetNewColor);
            }
            if (colorSwatch != null)
            {
                colorSwatch.targetGraphic = null;
                colorSwatch.sourceGraphic = newColorImage;
                colorSwatch.onPickColor.AddListener(SetNewColor);
            }

            //input fields
            if (input_HSV_H != null)
            {
                input_HSV_H.onValueChanged.AddListener(OnInputValueChangedHSV_H);
                input_HSV_H.onEndEdit.AddListener(OnInputValueChangedHSV_H);
            }
            if (input_HSV_S != null)
            {
                input_HSV_S.onValueChanged.AddListener(OnInputValueChangedHSV_S);
                input_HSV_S.onEndEdit.AddListener(OnInputValueChangedHSV_S);
            }
            if (input_HSV_V != null)
            {
                input_HSV_V.onValueChanged.AddListener(OnInputValueChangedHSV_V);
                input_HSV_V.onEndEdit.AddListener(OnInputValueChangedHSV_V);
            }
            if (input_RGB_R != null)
            {
                input_RGB_R.onValueChanged.AddListener(OnInputValueChangedRGB_R);
                input_RGB_R.onEndEdit.AddListener(OnInputValueChangedRGB_R);
            }
            if (input_RGB_G != null)
            {
                input_RGB_G.onValueChanged.AddListener(OnInputValueChangedRGB_G);
                input_RGB_G.onEndEdit.AddListener(OnInputValueChangedRGB_G);
            }
            if (input_RGB_B != null)
            {
                input_RGB_B.onValueChanged.AddListener(OnInputValueChangedRGB_B);
                input_RGB_B.onEndEdit.AddListener(OnInputValueChangedRGB_B);
            }
            if (input_Lab_L != null)
            {
                input_Lab_L.onValueChanged.AddListener(OnInputValueChangedLab_L);
                input_Lab_L.onEndEdit.AddListener(OnInputValueChangedLab_L);
            }
            if (input_Lab_a != null)
            {
                input_Lab_a.onValueChanged.AddListener(OnInputValueChangedLab_a);
                input_Lab_a.onEndEdit.AddListener(OnInputValueChangedLab_a);
            }
            if (input_Lab_b != null)
            {
                input_Lab_b.onValueChanged.AddListener(OnInputValueChangedLab_b);
                input_Lab_b.onEndEdit.AddListener(OnInputValueChangedLab_b);
            }
            if (input_CMYK_C != null)
            {
                input_CMYK_C.onValueChanged.AddListener(OnInputValueChangedCMYK_C);
                input_CMYK_C.onEndEdit.AddListener(OnInputValueChangedCMYK_C);
            }
            if (input_CMYK_M != null)
            {
                input_CMYK_M.onValueChanged.AddListener(OnInputValueChangedCMYK_M);
                input_CMYK_M.onEndEdit.AddListener(OnInputValueChangedCMYK_M);
            }
            if (input_CMYK_Y != null)
            {
                input_CMYK_Y.onValueChanged.AddListener(OnInputValueChangedCMYK_Y);
                input_CMYK_Y.onEndEdit.AddListener(OnInputValueChangedCMYK_Y);
            }
            if (input_CMYK_K != null)
            {
                input_CMYK_K.onValueChanged.AddListener(OnInputValueChangedCMYK_K);
                input_CMYK_K.onEndEdit.AddListener(OnInputValueChangedCMYK_K);
            }
            if (input_Hex != null)
            {
                //input_Hex.onValueChanged.AddListener(OnInputValueChangedHex);
                input_Hex.onEndEdit.AddListener(OnInputValueChangedHex);
            }
            if (input_Alpha != null)
            {
                input_Alpha.onValueChanged.AddListener(OnInputValueChangedAlpha);
                input_Alpha.onEndEdit.AddListener(OnInputValueChangedAlpha);
            }

            //mode toggles
            ToggleGroup tGroup = GetComponent<ToggleGroup>();
            if (tGroup == null) tGroup = gameObject.AddComponent<ToggleGroup>();
            if (toggle_HSV_H != null)
            {
                toggle_HSV_H.onValueChanged.AddListener(OnToggleValueChangedHSV_H);
                toggle_HSV_H.group = tGroup;
            }
            if (toggle_HSV_S != null)
            {
                toggle_HSV_S.onValueChanged.AddListener(OnToggleValueChangedHSV_S);
                toggle_HSV_S.group = tGroup;
            }
            if (toggle_HSV_V != null)
            {
                toggle_HSV_V.onValueChanged.AddListener(OnToggleValueChangedHSV_V);
                toggle_HSV_V.group = tGroup;
            }
            if (toggle_RGB_R != null)
            {
                toggle_RGB_R.onValueChanged.AddListener(OnToggleValueChangedRGB_R);
                toggle_RGB_R.group = tGroup;
            }
            if (toggle_RGB_G != null)
            {
                toggle_RGB_G.onValueChanged.AddListener(OnToggleValueChangedRGB_G);
                toggle_RGB_G.group = tGroup;
            }
            if (toggle_RGB_B != null)
            {
                toggle_RGB_B.onValueChanged.AddListener(OnToggleValueChangedRGB_B);
                toggle_RGB_B.group = tGroup;
            }
            if (toggle_Lab_L != null)
            {
                toggle_Lab_L.onValueChanged.AddListener(OnToggleValueChangedLab_L);
                toggle_Lab_L.group = tGroup;
            }
            if (toggle_Lab_a != null)
            {
                toggle_Lab_a.onValueChanged.AddListener(OnToggleValueChangedLab_a);
                toggle_Lab_a.group = tGroup;
            }
            if (toggle_Lab_b != null)
            {
                toggle_Lab_b.onValueChanged.AddListener(OnToggleValueChangedLab_b);
                toggle_Lab_b.group = tGroup;
            }
        }

        //=====================================================================
        //Set Color
        //=====================================================================

        void SetNewColor(Color color)
        {
            m_newColor = color;
            m_newColorHSV = new ColorHSV(m_newColor);
            m_newColorLab = new ColorLab(m_newColor);
            m_newColorHex = new ColorHex(m_newColor);
            m_newColorCMYK = new ColorCMYK(m_newColor);
            RefreshInputField();
            RefreshPicker();
        }

        void SetNewColorHSV(ColorHSV colorHSV)
        {
            m_newColor = colorHSV.ToColor();
            m_newColorHSV = colorHSV;
            m_newColorLab = new ColorLab(m_newColor);
            m_newColorHex = new ColorHex(m_newColor);
            m_newColorCMYK = new ColorCMYK(m_newColor);
            RefreshInputField();
            RefreshPicker();
        }

        void SetNewColorLab(ColorLab colorLab)
        {
            m_newColor = colorLab.ToColor();
            m_newColorHSV = new ColorHSV(m_newColor);
            m_newColorLab = colorLab;
            m_newColorHex = new ColorHex(m_newColor);
            m_newColorCMYK = new ColorCMYK(m_newColor);
            RefreshInputField();
            RefreshPicker();
        }

        void SetNewColorCMYK(ColorCMYK colorCMYK)
        {
            m_newColor = colorCMYK.ToColor();
            m_newColorHSV = new ColorHSV(m_newColor);
            m_newColorLab = new ColorLab(m_newColor);
            m_newColorHex = new ColorHex(m_newColor);
            m_newColorCMYK = colorCMYK;
            RefreshInputField();
            RefreshPicker();
        }

        void SetNewColorHex(ColorHex colorHex)
        {
            m_newColor = colorHex.ToColor();
            m_newColorHSV = new ColorHSV(m_newColor);
            m_newColorLab = new ColorLab(m_newColor);
            m_newColorHex = colorHex;
            m_newColorCMYK = new ColorCMYK(m_newColor);
            RefreshInputField();
            RefreshPicker();
        }

        void SetPickedColor(Vector2 pickAreaValue, float pickBarValue)
        {
            switch (mode)
            {
                case Mode.HSV_H:
                    {
                        float h = pickBarValue * 360.0f;
                        float s = pickAreaValue.x;
                        float v = pickAreaValue.y;

                        m_newColorHSV = new ColorHSV(h, s, v);
                        m_newColor = m_newColorHSV.ToColor();
                        m_newColorLab = new ColorLab(m_newColor);
                    }
                    break;
                case Mode.HSV_S:
                    {
                        float h = pickAreaValue.x * 360.0f;
                        float s = pickBarValue;
                        float v = pickAreaValue.y;

                        m_newColorHSV = new ColorHSV(h, s, v);
                        m_newColor = m_newColorHSV.ToColor();
                        m_newColorLab = new ColorLab(m_newColor);
                    }
                    break;
                case Mode.HSV_V:
                    {
                        float h = pickAreaValue.x * 360.0f;
                        float s = pickAreaValue.y;
                        float v = pickBarValue;

                        m_newColorHSV = new ColorHSV(h, s, v);
                        m_newColor = m_newColorHSV.ToColor();
                        m_newColorLab = new ColorLab(m_newColor);
                    }
                    break;
                case Mode.RGB_R:
                    {
                        float r = pickBarValue;
                        float g = pickAreaValue.y;
                        float b = pickAreaValue.x;

                        m_newColor = new Color(r, g, b);
                        m_newColorHSV = new ColorHSV(m_newColor);
                        m_newColorLab = new ColorLab(m_newColor);
                    }
                    break;
                case Mode.RGB_G:
                    {
                        float r = pickAreaValue.y;
                        float g = pickBarValue;
                        float b = pickAreaValue.x;

                        m_newColor = new Color(r, g, b);
                        m_newColorHSV = new ColorHSV(m_newColor);
                        m_newColorLab = new ColorLab(m_newColor);
                    }
                    break;
                case Mode.RGB_B:
                    {
                        float r = pickAreaValue.x;
                        float g = pickAreaValue.y;
                        float b = pickBarValue;

                        m_newColor = new Color(r, g, b);
                        m_newColorHSV = new ColorHSV(m_newColor);
                        m_newColorLab = new ColorLab(m_newColor);
                    }
                    break;
                case Mode.Lab_L:
                    {
                        float L = pickBarValue * 100.0f;
                        float a = pickAreaValue.x * 255.0f - 128.0f;
                        float b = pickAreaValue.y * 255.0f - 128.0f;

                        m_newColorLab = new ColorLab(L, a, b);
                        m_newColor = m_newColorLab.ToColor();
                        m_newColorHSV = new ColorHSV(m_newColor);
                    }
                    break;
                case Mode.Lab_a:
                    {
                        float L = pickAreaValue.y * 100.0f;
                        float a = pickBarValue * 255.0f - 128.0f;
                        float b = pickAreaValue.x * 255.0f - 128.0f;

                        m_newColorLab = new ColorLab(L, a, b);
                        m_newColor = m_newColorLab.ToColor();
                        m_newColorHSV = new ColorHSV(m_newColor);
                    }
                    break;
                case Mode.Lab_b:
                    {
                        float L = pickAreaValue.y * 100.0f;
                        float a = pickAreaValue.x * 255.0f - 128.0f;
                        float b = pickBarValue * 255.0f - 128.0f;

                        m_newColorLab = new ColorLab(L, a, b);
                        m_newColor = m_newColorLab.ToColor();
                        m_newColorHSV = new ColorHSV(m_newColor);
                    }
                    break;
                default:
                    break;
            }
            m_newColorHex = new ColorHex(m_newColor);
            m_newColorCMYK = new ColorCMYK(m_newColor);
            RefreshInputField();
            RefreshPicker();
        }
        
        //=====================================================================
        //Refresh
        //=====================================================================

        void RefreshInputField()
        {
            inputFieldLock = true;

            if (input_HSV_H != null) input_HSV_H.text = m_newColorHSV.h.ToString("0");
            if (input_HSV_S != null) input_HSV_S.text = (m_newColorHSV.s * 100.0f).ToString("0");
            if (input_HSV_V != null) input_HSV_V.text = (m_newColorHSV.v * 100.0f).ToString("0");

            if (input_RGB_R != null) input_RGB_R.text = (m_newColor.r * 255.0f).ToString("0");
            if (input_RGB_G != null) input_RGB_G.text = (m_newColor.g * 255.0f).ToString("0");
            if (input_RGB_B != null) input_RGB_B.text = (m_newColor.b * 255.0f).ToString("0");

            if (input_Lab_L != null) input_Lab_L.text = m_newColorLab.L.ToString("0");
            if (input_Lab_a != null) input_Lab_a.text = m_newColorLab.a.ToString("0");
            if (input_Lab_b != null) input_Lab_b.text = m_newColorLab.b.ToString("0");

            if (input_CMYK_C != null) input_CMYK_C.text = (m_newColorCMYK.c * 100.0f).ToString("0");
            if (input_CMYK_M != null) input_CMYK_M.text = (m_newColorCMYK.m * 100.0f).ToString("0");
            if (input_CMYK_Y != null) input_CMYK_Y.text = (m_newColorCMYK.y * 100.0f).ToString("0");
            if (input_CMYK_K != null) input_CMYK_K.text = (m_newColorCMYK.k * 100.0f).ToString("0");

            if (input_Hex != null) input_Hex.text = m_newColorHex.hex;

            if (newColorImage != null) newColorImage.color = newColor;
            if (currentColorImage != null) currentColorImage.color = currentColor;

            inputFieldLock = false;
        }

        void RefreshPicker()
        {
            if (pickArea == null || pickBar == null) return;

            Vector2 pickAreaValue = new Vector2();
            float pickBarValue = 0.0f;
            switch (mode)
            {
                case Mode.HSV_H:
                    {
                        pickBarValue = m_newColorHSV.h / 360.0f;
                        pickAreaValue.x = m_newColorHSV.s;
                        pickAreaValue.y = m_newColorHSV.v;
                    }
                    break;
                case Mode.HSV_S:
                    {
                        pickBarValue = m_newColorHSV.s;
                        pickAreaValue.x = m_newColorHSV.h / 360.0f;
                        pickAreaValue.y = m_newColorHSV.v;
                    }
                    break;
                case Mode.HSV_V:
                    {
                        pickBarValue = m_newColorHSV.v;
                        pickAreaValue.x = m_newColorHSV.h / 360.0f;
                        pickAreaValue.y = m_newColorHSV.s;
                    }
                    break;
                case Mode.RGB_R:
                    {
                        pickBarValue = m_newColor.r;
                        pickAreaValue.x = m_newColor.b;
                        pickAreaValue.y = m_newColor.g;
                    }
                    break;
                case Mode.RGB_G:
                    {
                        pickBarValue = m_newColor.g;
                        pickAreaValue.x = m_newColor.b;
                        pickAreaValue.y = m_newColor.r;
                    }
                    break;
                case Mode.RGB_B:
                    {
                        pickBarValue = m_newColor.b;
                        pickAreaValue.x = m_newColor.r;
                        pickAreaValue.y = m_newColor.g;
                    }
                    break;
                case Mode.Lab_L:
                    {
                        pickBarValue = m_newColorLab.L / 100.0f;
                        pickAreaValue.x = (m_newColorLab.a + 128.0f) / 255.0f;
                        pickAreaValue.y = (m_newColorLab.b + 128.0f) / 255.0f;
                    }
                    break;
                case Mode.Lab_a:
                    {
                        pickBarValue = (m_newColorLab.a + 128.0f) / 255.0f;
                        pickAreaValue.x = (m_newColorLab.b + 128.0f) / 255.0f;
                        pickAreaValue.y = m_newColorLab.L / 100.0f;
                    }
                    break;
                case Mode.Lab_b:
                    {
                        pickBarValue = (m_newColorLab.b + 128.0f) / 255.0f;
                        pickAreaValue.x = (m_newColorLab.a + 128.0f) / 255.0f;
                        pickAreaValue.y = m_newColorLab.L / 100.0f;
                    }
                    break;
                default:
                    break;
            }
            pickArea.lValue = m_newColorLab.L;
            if (!pickAreaLock)
            {
                pickArea.pickBarValue = pickBarValue;
                pickArea.value = pickAreaValue;
            }
            if (!pickBarLock)
            {
                pickBar.pickAreaValue = pickAreaValue;
                pickBar.value = pickBarValue;
            }
        }

        //=====================================================================
        //Set Mode
        //=====================================================================

        public void SetMode(Mode mode)
        {
            this.mode = mode;
            pickArea.mode = mode;
            pickBar.mode = mode;
            RefreshPicker();
        }

        void OnToggleValueChangedHSV_H(bool isOn)
        {
            if (!isOn) return;
            SetMode(Mode.HSV_H);
        }

        void OnToggleValueChangedHSV_S(bool isOn)
        {
            if (!isOn) return;
            SetMode(Mode.HSV_S);
        }

        void OnToggleValueChangedHSV_V(bool isOn)
        {
            if (!isOn) return;
            SetMode(Mode.HSV_V);
        }

        void OnToggleValueChangedRGB_R(bool isOn)
        {
            if (!isOn) return;
            SetMode(Mode.RGB_R);
        }

        void OnToggleValueChangedRGB_G(bool isOn)
        {
            if (!isOn) return;
            SetMode(Mode.RGB_G);
        }

        void OnToggleValueChangedRGB_B(bool isOn)
        {
            if (!isOn) return;
            SetMode(Mode.RGB_B);
        }

        void OnToggleValueChangedLab_L(bool isOn)
        {
            if (!isOn) return;
            SetMode(Mode.Lab_L);
        }

        void OnToggleValueChangedLab_a(bool isOn)
        {
            if (!isOn) return;
            SetMode(Mode.Lab_a);
        }

        void OnToggleValueChangedLab_b(bool isOn)
        {
            if (!isOn) return;
            SetMode(Mode.Lab_b);
        }

        //=====================================================================
        //Picked Color Changed
        //=====================================================================

        void OnAlphaSliderChanged(float value)
        {
            if (inputFieldLock) return;
            m_alpha = value;
            inputFieldLock = true;
            if (input_Alpha != null) input_Alpha.text = (m_alpha * 100.0f).ToString("0");
            inputFieldLock = false;
            if (newColorImage != null) newColorImage.color = newColor;
        }

        void OnPickAreaChanged(Vector2 value)
        {
            pickAreaLock = true;
            SetPickedColor(value, pickBar.value);
            pickAreaLock = false;
        }

        void OnPickBarChanged(float value)
        {
            pickBarLock = true;
            SetPickedColor(pickArea.value, value);
            pickBarLock = false;
        }

        void OnEyedropperChanged(Color value)
        {
            pickBarLock = true;
            pickAreaLock = true;
            SetNewColor(value);
            pickBarLock = false;
            pickAreaLock = false;
        }

        //=====================================================================
        //Input Changed
        //=====================================================================

        void OnInputValueChangedHSV_H(string str)
        {
            if (str == "" || str == "-" || inputFieldLock) return;
            m_newColorHSV.h = Mathf.Clamp(int.Parse(str), 0, 360);
            SetNewColorHSV(m_newColorHSV);
        }

        void OnInputValueChangedHSV_S(string str)
        {
            if (str == "" || str == "-" || inputFieldLock) return;
            m_newColorHSV.s = Mathf.Clamp01(int.Parse(str) / 100.0f);
            SetNewColorHSV(m_newColorHSV);
        }

        void OnInputValueChangedHSV_V(string str)
        {
            if (str == "" || str == "-" || inputFieldLock) return;
            m_newColorHSV.v = Mathf.Clamp01(int.Parse(str) / 100.0f);
            SetNewColorHSV(m_newColorHSV);
        }

        void OnInputValueChangedRGB_R(string str)
        {
            if (str == "" || str == "-" || inputFieldLock) return;
            m_newColor.r = Mathf.Clamp01(int.Parse(str) / 255.0f);
            SetNewColor(m_newColor);
        }

        void OnInputValueChangedRGB_G(string str)
        {
            if (str == "" || str == "-" || inputFieldLock) return;
            m_newColor.g = Mathf.Clamp01(int.Parse(str) / 255.0f);
            SetNewColor(m_newColor);
        }

        void OnInputValueChangedRGB_B(string str)
        {
            if (str == "" || str == "-" || inputFieldLock) return;
            m_newColor.b = Mathf.Clamp01(int.Parse(str) / 255.0f);
            SetNewColor(m_newColor);
        }

        void OnInputValueChangedLab_L(string str)
        {
            if (str == "" || str == "-" || inputFieldLock) return;
            m_newColorLab.L = Mathf.Clamp(int.Parse(str), 0, 100);
            SetNewColorLab(m_newColorLab);
        }

        void OnInputValueChangedLab_a(string str)
        {
            if (str == "" || str == "-" || inputFieldLock) return;
            m_newColorLab.a = Mathf.Clamp(int.Parse(str), -128, 127);
            SetNewColorLab(m_newColorLab);
        }

        void OnInputValueChangedLab_b(string str)
        {
            if (str == "" || str == "-" || inputFieldLock) return;
            m_newColorLab.b = Mathf.Clamp(int.Parse(str), -128, 127);
            SetNewColorLab(m_newColorLab);
        }

        void OnInputValueChangedCMYK_C(string str)
        {
            if (str == "" || str == "-" || inputFieldLock) return;
            m_newColorCMYK.c = Mathf.Clamp01(int.Parse(str) / 100.0f);
            SetNewColorCMYK(m_newColorCMYK);
        }

        void OnInputValueChangedCMYK_M(string str)
        {
            if (str == "" || str == "-" || inputFieldLock) return;
            m_newColorCMYK.m = Mathf.Clamp01(int.Parse(str) / 100.0f);
            SetNewColorCMYK(m_newColorCMYK);
        }

        void OnInputValueChangedCMYK_Y(string str)
        {
            if (str == "" || str == "-" || inputFieldLock) return;
            m_newColorCMYK.y = Mathf.Clamp01(int.Parse(str) / 100.0f);
            SetNewColorCMYK(m_newColorCMYK);
        }

        void OnInputValueChangedCMYK_K(string str)
        {
            if (str == "" || str == "-" || inputFieldLock) return;
            m_newColorCMYK.k = Mathf.Clamp01(int.Parse(str) / 100.0f);
            SetNewColorCMYK(m_newColorCMYK);
        }

        void OnInputValueChangedHex(string str)
        {
            if (str == m_newColorHex.hex || inputFieldLock) return;
            m_newColorHex = new ColorHex(str);
            SetNewColorHex(m_newColorHex);
        }

        void OnInputValueChangedAlpha(string str)
        {
            if (str == "" || str == "-" || inputFieldLock) return;
            m_alpha = Mathf.Clamp01(int.Parse(str) / 100.0f);
            inputFieldLock = true;
            if (input_Alpha != null) input_Alpha.text = (m_alpha * 100.0f).ToString("0");
            if (alphaSlider != null) alphaSlider.value = m_alpha;
            inputFieldLock = false;
            if (newColorImage != null) newColorImage.color = newColor;
        }

        //=====================================================================
        //Input End
        //=====================================================================

        void OnInputEndEditHSV_H(string str)
        {
            if (str == "" || str == "-") { str = "0"; OnInputValueChangedHSV_H(str); }
        }

        void OnInputEndEditHSV_S(string str)
        {
            if (str == "" || str == "-") { str = "0"; OnInputValueChangedHSV_S(str); }
        }

        void OnInputEndEditHSV_V(string str)
        {
            if (str == "" || str == "-") { str = "0"; OnInputValueChangedHSV_V(str); }
        }

        void OnInputEndEditRGB_R(string str)
        {
            if (str == "" || str == "-") { str = "0"; OnInputValueChangedRGB_R(str); }
        }

        void OnInputEndEditRGB_G(string str)
        {
            if (str == "" || str == "-") { str = "0"; OnInputValueChangedRGB_G(str); }
        }

        void OnInputEndEditRGB_B(string str)
        {
            if (str == "" || str == "-") { str = "0"; OnInputValueChangedRGB_B(str); }
        }

        void OnInputEndEditLab_L(string str)
        {
            if (str == "" || str == "-") { str = "0"; OnInputValueChangedLab_L(str); }
        }

        void OnInputEndEditLab_a(string str)
        {
            if (str == "" || str == "-") { str = "0"; OnInputValueChangedLab_a(str); }
        }

        void OnInputEndEditLab_b(string str)
        {
            if (str == "" || str == "-") { str = "0"; OnInputValueChangedLab_b(str); }
        }

        void OnInputEndEditCMYK_C(string str)
        {
            if (str == "" || str == "-") { str = "0"; OnInputValueChangedCMYK_C(str); }
        }

        void OnInputEndEditCMYK_M(string str)
        {
            if (str == "" || str == "-") { str = "0"; OnInputValueChangedCMYK_M(str); }
        }

        void OnInputEndEditCMYK_Y(string str)
        {
            if (str == "" || str == "-") { str = "0"; OnInputValueChangedCMYK_Y(str); }
        }

        void OnInputEndEditCMYK_K(string str)
        {
            if (str == "" || str == "-") { str = "0"; OnInputValueChangedCMYK_K(str); }
        }

        void OnInputEndEditHex(string str)
        {
            while (str.Length < 6) { str = '0' + str; } OnInputValueChangedHex(str);
        }

        void OnInputEndEditAlpha(string str)
        {
            if (str == "" || str == "-") { str = "0"; OnInputValueChangedAlpha(str); }
        }
    }
}