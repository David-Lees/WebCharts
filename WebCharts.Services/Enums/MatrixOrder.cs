namespace WebCharts.Services
{
    public enum MatrixOrder
    {
        Append = 1, // The new operation is applied after the old operation.
        Prepend = 0, // The new operation is applied before the old operation.
    }
}