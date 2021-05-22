﻿namespace WebCharts.Services
{
    public enum AccessibleRole
    {
        Alert = 8, // An alert or condition that you can notify a user about. Use this role only for objects that embody an alert but are not associated with another user interface element, such as a message box, graphic, text, or sound.
        Animation = 54, // An animation control, which contains content that is changing over time, such as a control that displays a series of bitmap frames, like a filmstrip.Animation controls are usually displayed when files are being copied, or when some other time-consuming task is being performed.
        Application = 14, // The main window for an application.
        Border = 19, // A window border.The entire border is represented by a single object, rather than by separate objects for each side.
        ButtonDropDown = 56,    // A button that drops down a list of items.
        ButtonDropDownGrid = 58,// A button that drops down a grid.
        ButtonMenu = 57, // A button that drops down a menu.
        Caret = 7, // A caret, which is a flashing line, block, or bitmap that marks the location of the insertion point in a window's client area.
        Cell = 29, // A cell within a table.
        Character = 32, // A cartoon-like graphic object, such as Microsoft Office Assistant, which is typically displayed to provide help to users of an application.
        Chart = 17, // A graphical image used to represent data.
        CheckButton = 44, // A check box control, which is an option that can be turned on or off independent of other options.
        Client = 10, // A window's user area.
        Clock = 61, // A control that displays the time.
        Column = 27, // A column of cells within a table.
        ColumnHeader = 25, // A column header, which provides a visual label for a column in a table.
        ComboBox = 46, // A combo box, which is an edit control with an associated list box that provides a set of predefined choices.
        Cursor = 6, // A mouse pointer.
        Default = -1, // A system-provided role.
        Diagram = 53, // A graphical image used to diagram data.
        Dial = 49, // A dial or knob. This can also be a read-only object, like a speedometer.
        Dialog = 18, // A dialog box or message box.
        Document = 15, // A document window, which is always contained within an application window. This role applies only to multiple-document interface (MDI) windows and refers to an object that contains the MDI title bar.
        DropList = 47, // A drop-down list box.This control shows one item and allows the user to display and select another from a list of alternative choices.
        Equation = 55, // A mathematical equation.
        Graphic = 40, // A picture.
        Grip = 4, // A special mouse pointer, which allows a user to manipulate user interface elements such as a window.For example, a user can click and drag a sizing grip in the lower-right corner of a window to resize it.
        Grouping = 20, // The objects grouped in a logical manner.There can be a parent-child relationship between the grouping object and the objects it contains.
        HelpBalloon = 31, // A Help display in the form of a ToolTip or Help balloon, which contains buttons and labels that users can click to open custom Help topics.
        HotkeyField = 50, // A hot-key field that allows the user to enter a combination or sequence of keystrokes to be used as a hot key, which enables users to perform an action quickly. A hot-key control displays the keystrokes entered by the user and ensures that the user selects a valid key combination.
        Indicator = 39, // An indicator, such as a pointer graphic, that points to the current item.
        IpAddress = 63, // A control designed for entering Internet Protocol (IP) addresses.
        Link = 30, // A link, which is a connection between a source document and a destination document. This object might look like text or a graphic, but it acts like a button.
        List = 33, // A list box, which allows the user to select one or more items.
        ListItem = 34, // An item in a list box or the list portion of a combo box, drop-down list box, or drop-down combo box.
        MenuBar = 2, // A menu bar, usually beneath the title bar of a window, from which users can select menus.
        MenuItem = 12, // A menu item, which is an entry in a menu that a user can choose to carry out a command, select an option, or display another menu. Functionally, a menu item can be equivalent to a push button, radio button, check box, or menu.
        MenuPopup = 11, // A menu, which presents a list of options from which the user can make a selection to perform an action. All menu types must have this role, including drop-down menus that are displayed by selection from a menu bar, and shortcut menus that are displayed when the right mouse button is clicked.
        None = 0, // No role.
        Outline = 35, // An outline or tree structure, such as a tree view control, which displays a hierarchical list and usually allows the user to expand and collapse branches.
        OutlineButton = 64, // A control that navigates like an outline item.
        OutlineItem = 36, // An item in an outline or tree structure.
        PageTab = 37, // A property page that allows a user to view the attributes for a page, such as the page's title, whether it is a home page, or whether the page has been modified. Normally, the only child of this control is a grouped object that contains the contents of the associated page.
        PageTabList = 60, // A container of page tab controls.
        Pane = 16, // A separate area in a frame, a split document window, or a rectangular area of the status bar that can be used to display information.Users can navigate between panes and within the contents of the current pane, but cannot navigate between items in different panes. Thus, panes represent a level of grouping lower than frame windows or documents, but above individual controls. Typically, the user navigates between panes by pressing TAB, F6, or CTRL+TAB, depending on the context.
        ProgressBar = 48, // A progress bar, which indicates the progress of a lengthy operation by displaying colored lines inside a horizontal rectangle. The length of the lines in relation to the length of the rectangle corresponds to the percentage of the operation that is complete.This control does not take user input.
        PropertyPage = 38, // A property page, which is a dialog box that controls the appearance and the behavior of an object, such as a file or resource. A property page's appearance differs according to its purpose.
        PushButton = 43, // A push button control, which is a small rectangular control that a user can turn on or off. A push button, also known as a command button, has a raised appearance in its default off state and a sunken appearance when it is turned on.
        RadioButton = 45, // An option button, also known as a radio button.All objects sharing a single parent that have this attribute are assumed to be part of a single mutually exclusive group. You can use grouped objects to divide option buttons into separate groups when necessary.
        Row = 28, // A row of cells within a table.
        RowHeader = 26, // A row header, which provides a visual label for a table row.
        ScrollBar = 3, // A vertical or horizontal scroll bar, which can be either part of the client area or used in a control.
        Separator = 21, // A space divided visually into two regions, such as a separator menu item or a separator dividing split panes within a window.
        Slider = 51, // A control, sometimes called a trackbar, that enables a user to adjust a setting in given increments between minimum and maximum values by moving a slider.The volume controls in the Windows operating system are slider controls.
        Sound = 5, // A system sound, which is associated with various system events.
        SpinButton = 52, // A spin box, also known as an up-down control, which contains a pair of arrow buttons.A user clicks the arrow buttons with a mouse to increment or decrement a value.A spin button control is most often used with a companion control, called a buddy window, where the current value is displayed.
        SplitButton = 62, // A toolbar button that has a drop-down list icon directly adjacent to the button.
        StaticText = 41, // The read-only text, such as in a label, for other controls or instructions in a dialog box.Static text cannot be modified or selected.
        StatusBar = 23, // A status bar, which is an area typically at the bottom of an application window that displays information about the current operation, state of the application, or selected object. The status bar can have multiple fields that display different kinds of information, such as an explanation of the currently selected menu command in the status bar.
        Table = 24, // A table containing rows and columns of cells and, optionally, row headers and column headers.
        Text = 42, // The selectable text that can be editable or read-only.
        TitleBar = 1, // A title or caption bar for a window.
        ToolBar = 22, // A toolbar, which is a grouping of controls that provide easy access to frequently used features.
        ToolTip = 13, // A tool tip, which is a small rectangular pop-up window that displays a brief description of the purpose of a button.
        WhiteSpace = 59, // A blank space between other objects.
        Window = 9, // A window frame, which usually contains child objects such as a title bar, client, and other objects typically contained in a window.
    }
}