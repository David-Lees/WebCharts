namespace WebCharts.Services
{
    public enum AccessibleNavigation
    {
        Down = 2,   //Navigation to a sibling object located below the starting object.
        FirstChild = 7, //Navigation to the first child of the object.
        LastChild = 8,//	Navigation to the last child of the object.
        Left = 3,   //Navigation to the sibling object located to the left of the starting object.
        Next = 5,// Navigation to the next logical object, typically from a sibling object to the starting object.
        Previous = 6,//	Navigation to the previous logical object, typically from a sibling object to the starting object.
        Right = 4,  // Navigation to the sibling object located to the right of the starting object.
        Up = 1, // Navigation to a sibling object located above the starting object.
    }
}