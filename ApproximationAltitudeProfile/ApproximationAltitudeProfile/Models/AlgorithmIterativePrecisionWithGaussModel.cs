using System;
namespace ApproximationAltitudeProfile.AlgorithmModels
{
    public class AlgorithmIterativePrecisionWithGauss
    {
        public double XKey { get; set; }
        public int Iteration { get; set; }
        public double ValueFromFile { get; set; }
        public double ValueForGaussSeidel { get; set; }
        public double ValueForGauss { get; set; }
        public bool IsSameAsGauss { get; set; }
        public bool IsBetterThanGauss { get; set; }
    }
}
