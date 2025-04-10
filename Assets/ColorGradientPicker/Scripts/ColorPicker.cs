using System;
using UnityEngine;
using UnityEngine.UI;
public class ColorPicker : MonoBehaviour
{
    /// <summary>
    /// Event that gets called by the ColorPicker
    /// </summary>
    /// <param name="c">received Color</param>
    public delegate void ColorEvent(Color c);

    private static ColorPicker instance;
    /// <returns>
    /// True when the ColorPicker is closed
    /// </returns>
    public static bool done = true;

    private static Texture originalBaseMap;  // Stocker la texture initiale
    private static Renderer currentRenderer; // Stocker le renderer actuel

    //onColorChanged event
    private static ColorEvent onCC;
    //onColorSelected event
    private static ColorEvent onCS;

    //Color before editing
    private static Color32 originalColor;
    //current Color
    private static Color32 modifiedColor;
    private static HSV modifiedHsv;

    //useAlpha bool
    private static bool useA;

    private bool interact;



    // these can only work with the prefab and its children
    public RectTransform positionIndicator;
    public Slider mainComponent;
    public RawImage colorComponent;

    public RawImage chosenColor;

    private void Awake()
    {
        instance = this;
        gameObject.SetActive(false);
        originalColor = Color.white; // Initialiser en blanc
        modifiedColor = Color.white;
    }



    /// <summary>
    /// Creates a new Colorpicker
    /// </summary>
    /// <param name="original">Color before editing</param>
    /// <param name="message">Display message</param>
    /// <param name="onColorChanged">Event that gets called when the color gets modified</param>
    /// <param name="onColorSelected">Event that gets called when one of the buttons done or cancel get pressed</param>
    /// <param name="useAlpha">When set to false the colors used don't have an alpha channel</param>
    /// <returns>
    /// False if the instance is already running
    /// </returns>
    public static bool Create(Color? original = null, string message = "", Renderer renderer = null, ColorEvent onColorChanged = null, ColorEvent onColorSelected = null, bool useAlpha = false)
    {
        if (instance is null)
        {
            return false;
        }
        if (done)
        {
            done = false;

            originalColor = original ?? Color.white; // Défaut à blanc
            modifiedColor = originalColor;
            onCC = onColorChanged;
            onCS = onColorSelected;


            useA = useAlpha;
            instance.gameObject.SetActive(true);

            currentRenderer = renderer;
            if (renderer != null)
            {
                originalBaseMap = currentRenderer.material.GetTexture("_BaseMap");
                currentRenderer.material.SetTexture("_BaseMap", null); // Retirer la texture actuelle
            }

            instance.RecalculateMenu(true);
            return true;
        }
        else
        {
            Done();
            return false;
        }
    }



    //called when color is modified, to update other UI components
    private void RecalculateMenu(bool recalculateHSV)
    {
        interact = false;
        if (recalculateHSV)
        {
            modifiedHsv = new HSV(modifiedColor);
        }
        else
        {
            modifiedColor = modifiedHsv.ToColor();
        }

        mainComponent.value = (float)modifiedHsv.H;
        colorComponent.color = modifiedColor; // Preview final
        UpdateChosenColor(); // Mise à jour de ChosenColor
        onCC?.Invoke(modifiedColor);

        interact = true;
    }


    private void UpdateChosenColor()
    {
        if (chosenColor != null)
        {
            // Crée une couleur HSV avec H = teinte du slider, S = 1, V = 1
            HSV baseHsv = new HSV(modifiedHsv.H, 1.0, 1.0);
            chosenColor.color = baseHsv.ToColor();
        }
        else
        {
            Debug.LogError("chosenColor n'est pas assigné dans ColorPicker !");
        }
    }


    //used by EventTrigger to calculate the chosen value in color box
    public void SetChooser()
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(positionIndicator.parent as RectTransform, Input.mousePosition, GetComponentInParent<Canvas>().worldCamera, out Vector2 localpoint);
        localpoint = Rect.PointToNormalized((positionIndicator.parent as RectTransform).rect, localpoint);
        if (positionIndicator.anchorMin != localpoint)
        {
            positionIndicator.anchorMin = localpoint;
            positionIndicator.anchorMax = localpoint;
            modifiedHsv.S = localpoint.x;
            modifiedHsv.V = localpoint.y;
            RecalculateMenu(false);
        }
    }

    // Méthode appelée quand le slider change
    public void SetMain(float value)
    {
        if (interact)
        {
            modifiedHsv.H = value;
            RecalculateMenu(false);
            UpdateChosenColor(); // Mise à jour de ChosenColor
        }
    }

    //cancel button call
    public void CCancel()
    {
        Cancel();
    }
    /// <summary>
    /// Manually cancel the ColorPicker and recover the default value
    /// </summary>
    public static void Cancel()
    {
        modifiedColor = originalColor;

        // Restaurer la texture originale
        if (currentRenderer != null)
        {
            currentRenderer.material.SetTexture("_BaseMap", originalBaseMap);
        }

        Done();
    }
    //done button call
    public void CDone()
    {
        Done();
    }

    /// <summary>
    /// Manually close the ColorPicker and apply the selected color
    /// </summary>
    public static void Done()
    {
        done = true;
        onCC?.Invoke(modifiedColor);


        instance.transform.gameObject.SetActive(false);
    }

    //HSV helper class
    private sealed class HSV
    {
        public double H = 0, S = 1, V = 1;
        public byte A = 255;
        public HSV () { }
        public HSV (double h, double s, double v)
        {
            H = h;
            S = s;
            V = v;
        }
        public HSV (Color color)
        {
            float max = Mathf.Max(color.r, Mathf.Max(color.g, color.b));
            float min = Mathf.Min(color.r, Mathf.Min(color.g, color.b));

            float hue = (float)H;
            if (min != max)
            {
                if (max == color.r)
                {
                    hue = (color.g - color.b) / (max - min);

                }
                else if (max == color.g)
                {
                    hue = 2f + (color.b - color.r) / (max - min);

                }
                else
                {
                    hue = 4f + (color.r - color.g) / (max - min);
                }

                hue *= 60;
                if (hue < 0) hue += 360;
            }

            H = hue;
            S = (max == 0) ? 0 : 1d - ((double)min / max);
            V = max;
            A = (byte)(color.a * 255);
        }
        public Color32 ToColor()
        {
            int hi = Convert.ToInt32(Math.Floor(H / 60)) % 6;
            double f = H / 60 - Math.Floor(H / 60);

            double value = V * 255;
            byte v = (byte)Convert.ToInt32(value);
            byte p = (byte)Convert.ToInt32(value * (1 - S));
            byte q = (byte)Convert.ToInt32(value * (1 - f * S));
            byte t = (byte)Convert.ToInt32(value * (1 - (1 - f) * S));

            switch(hi)
            {
                case 0:
                    return new Color32(v, t, p, A);
                case 1:
                    return new Color32(q, v, p, A);
                case 2:
                    return new Color32(p, v, t, A);
                case 3:
                    return new Color32(p, q, v, A);
                case 4:
                    return new Color32(t, p, v, A);
                case 5:
                    return new Color32(v, p, q, A);
                default:
                    return new Color32();
            }
        }
    }
}