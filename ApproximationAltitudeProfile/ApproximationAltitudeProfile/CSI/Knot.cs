namespace ApproximationAltitudeProfile
{
    public class Knot
    {
        public int Index { get; }
        public double X { get; }
        public double Y { get; }

        public Knot(
            int index,
            double x, 
            double y)
        {
            Index = index;
            X = x;
            Y = y;
        }
    }
}
