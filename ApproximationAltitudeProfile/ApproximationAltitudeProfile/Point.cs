namespace ApproximationAltitudeProfile
{
    public class Point
    {
        public int Index { get; }
        public double X { get; }
        public double Y { get; }

        public Point(
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
