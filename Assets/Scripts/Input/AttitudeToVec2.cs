using UnityEngine;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem;

// Use InputBindingComposite<TValue> as a base class for a composite that returns
// values of type TValue.
// NOTE: It is possible to define a composite that returns different kinds of values
//       but doing so requires deriving directly from InputBindingComposite.
#if UNITY_EDITOR
using UnityEditor;
[InitializeOnLoad] // Automatically register in editor.
#endif
// Determine how GetBindingDisplayString() formats the composite by applying
// the  DisplayStringFormat attribute.
[DisplayStringFormat("{firstPart}+{secondPart}")]
public class AttitudeToVec2 : InputBindingComposite<Vector2>
{
    // Each part binding is represented as a field of type int and annotated with
    // InputControlAttribute. Setting "layout" restricts the controls that
    // are made available for picking in the UI.
    //
    // On creation, the int value is set to an integer identifier for the binding
    // part. This identifier can read values from InputBindingCompositeContext.
    // See ReadValue() below.
    [InputControl(layout = "Value")]
    public int w;
    [InputControl(layout = "Value")]
    public int x;
    [InputControl(layout = "Value")]
    public int y;
    [InputControl(layout = "Value")]
    public int z;

    // This method computes the resulting input value of the composite based
    // on the input from its part bindings.
    public override Vector2 ReadValue(ref InputBindingCompositeContext context)
    {
        float wVal = context.ReadValue<float>(w);
        float xVal = context.ReadValue<float>(x);
        float yVal = context.ReadValue<float>(y);
        float zVal = context.ReadValue<float>(z);

        Quaternion rot = new Quaternion(xVal, yVal, zVal, wVal);
        // shift to -180deg to 180deg
        float xTarget = rot.eulerAngles.x;
        if (xTarget > 180f) xTarget -= 360;
        float yTarget = rot.eulerAngles.y;
        if (yTarget > 180f) yTarget -= 360;

        // 0-1 values
        xTarget /= 90f;
        yTarget /= 90f;
        xTarget = Mathf.Clamp(xTarget, 0, 1);
        yTarget = Mathf.Clamp(yTarget, 0, 1);

        Vector2 dir = (Vector2)(rot * Vector3.up);
        return dir;
    }

    // This method computes the current actuation of the binding as a whole.
    public override float EvaluateMagnitude(ref InputBindingCompositeContext context)
    {
        // Compute normalized [0..1] magnitude value for current actuation level.
        return 1f;
    }


    static AttitudeToVec2()
    {
        InputSystem.RegisterBindingComposite<AttitudeToVec2>();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init() { } // Trigger static constructor.
}
