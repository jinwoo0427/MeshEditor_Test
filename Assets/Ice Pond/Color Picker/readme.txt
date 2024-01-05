-Basic usage: 
    1.drag and drop "ColorPicker" prefab into the canvas
    2.Option A:
        Use "ColorPickingButton" prefab to quickly set up color picking events
    2.Option B:
        Write your own scripts for color picking events:
        -use "colorPicker.currentColor = yourCurrentColor" to set colorPicker's current color.
        -use "newColor = colorPicker.newColor" to get the picked color.
        -add listener to "colorPicker.onEndPick/onCancel" for callback functions.

-Eyedropper tool:
    -use "newColor = eyedropper.newColor" to get the picked color from eyedropper tool.
    -add listener to "eyedropper.onValueChanged" for callback function when eyedropper tool is picking colors.
    -add listener to "eyedropper.onSamplingEnd" for callback function when color picking is completed.

-"ColorFormat.cs" contains color conversion classes:
    -including 4 classes: ColorHSV, ColorLab, ColorHex and ColorCMYK.
    -use "FromColor" and "ToColor" functions to do the conversion.
    -example:
         ColorHSV myColor = new ColorHSV();
         myColor.FromColor(Color.white);
         Color newColor = myColor.ToColor();

Notes:
    -To enable Text Mesh Pro support, Add "COLORPICKER_TMPRO" into "Player Settings - Other Settings - Scripting Define Symbols", then you will need to replace all standard Texts with Text Mesh Pro Texts manually.