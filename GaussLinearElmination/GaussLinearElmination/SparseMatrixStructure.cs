namespace GaussLinearElimination
{
    public class SparseMatrixStructure
    {
        public SparseMatrixStructure(
            int row,
            int column,
            double value)
        {
            Row = row;
            Column = column;
            Value = value;
        }
        public int Row { get; set; }
        public int Column { get; set; }
        public double Value { get; set; }
    }
}

