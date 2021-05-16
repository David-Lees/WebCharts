using System;

namespace WebCharts.Services.Enums
{
    [Flags]
    public enum AccessibleSelection
    {
        AddSelection = 8, // Adds the object to the selection.
        ExtendSelection = 4, // Selects all objects between the anchor and the selected object.
        None = 0, // The selection or focus of an object is unchanged.
        RemoveSelection = 16, // Removes the object from the selection.
        TakeFocus = 1,// Assigns focus to an object and makes it the anchor, which is the starting point for the selection. Can be combined with TakeSelection, ExtendSelection, AddSelection, or RemoveSelection.
        TakeSelection = 2, //Selects the object and deselects all other objects in the container.
    }
}
