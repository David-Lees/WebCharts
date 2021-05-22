using System;

namespace WebCharts.Services
{
    [Flags]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1069:Enums values should not be duplicated", Justification = "<Pending>")]
    public enum AccessibleStates
    {
        AlertHigh = 268435456,// The important information that should be conveyed to the user immediately. For example, a battery-level indicator reaching a critical low level would transition to this state, in which case, a blind access utility would announce this information immediately to the user, and a screen magnification program would scroll the screen so that the battery indicator is in view. This state is also appropriate for any prompt or operation that must be completed before the user can continue.
        AlertLow = 67108864,// The low-priority information that might not be important to the user.
        AlertMedium = 134217728, //The important information that does not need to be conveyed to the user immediately. For example, when a battery-level indicator is starting to reach a low level, it could generate a medium-level alert. Blind access utilities could then generate a sound to let the user know that important information is available, without actually interrupting the user's work. Users can then query the alert information any time they choose.
        Animated = 16384, // The object that rapidly or constantly changes appearance. Graphics that are occasionally animated, but not always, should be defined as GraphicORAnimated. This state should not be used to indicate that the object's location is changing.
        Busy = 2048, // A control that cannot accept input in its current condition.
        Checked = 16,// An object with a selected check box.
        Collapsed = 1024,// The hidden children of the object that are items in an outline or tree structure.
        Default = 256, //The default button or menu item.
        Expanded = 512, //The displayed children of the object that are items in an outline or tree structure.
        ExtSelectable = 33554432, // The altered selection such that all objects between the selection anchor, which is the object with the keyboard focus, and this object take on the anchor object's selection state. If the anchor object is not selected, the objects are removed from the selection. If the anchor object is selected, the selection is extended to include this object and all objects in between. You can set the selection state by combining this with AddSelection or RemoveSelection. This state does not change the focus or the selection anchor unless it is combined with TakeFocus.
        Floating = 4096, // The object that is not fixed to the boundary of its parent object and that does not move automatically along with the parent.
        Focusable = 1048576,// The object on the active window that can receive keyboard focus.
        Focused = 4,// An object with the keyboard focus.
        HasPopup = 1073741824, //The object displays a pop-up menu or window when invoked.
        HotTracked = 128, // The object hot-tracked by the mouse, meaning its appearance is highlighted to indicate the mouse pointer is located over it.
        Indeterminate = 32, // A three-state check box or toolbar button whose state is indeterminate. The check box is neither checked nor unchecked, and it is in the third or mixed state.
        Invisible = 32768, // An object without a visible user interface.
        Linked = 4194304, // A linked object that has not been previously selected.
        Marqueed = 8192, // An object with scrolling or moving text or graphics.
        Mixed = 32,// A three-state check box or toolbar button whose state is indeterminate.The check box is neither checked nor unchecked, and it is in the third or mixed state.
        Moveable = 262144,// A movable object.
        MultiSelectable = 16777216,//An object that accepts multiple selected items.
        None = 0,//	No state.
        Offscreen = 65536,// No on-screen representation. A sound or alert object would have this state, or a hidden window that is never made visible.
        Pressed = 8,// A pressed object.
        Protected = 536870912,//A password-protected edit control.
        ReadOnly = 64,// A read-only object.
        Selectable = 2097152,// An object that can accept selection.
        Selected = 2,// A selected object.
        SelfVoicing = 524288,// The object or child can use text-to-speech (TTS) to describe itself. A speech-based accessibility aid should not announce information when an object with this state has the focus, because the object automatically announces information about itself.
        Sizeable = 131072,// A sizable object.
        Traversed = 8388608,// A linked object that has previously been selected.
        Unavailable = 1,// An unavailable object.
        Valid = 1073741823,// A valid object. This property is deprecated in .NET Framework 2.0.
    }
}