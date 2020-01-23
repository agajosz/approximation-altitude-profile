using System;
namespace ApproximationAltitudeProfile.AlgorithmModels
{
    public class AlgorithmIterativePrecision
    {
        public double XKey { get; set; }
        public int Iteration { get; set; }
        public double ValueFromFile { get; set; }
        public double ValueForGaussSeidel { get; set; }
        public double ValueForJacobi { get; set; }
        public bool IsSameValue { get; set; }
    }
}
