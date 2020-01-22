using System;

namespace ApproximationAltitudeProfile
{
    public class AlgorithmIterativePrecisionWithGaussModel
    {
        public double XKey { get; set; }
        public int Iteration { get; set; }
        public double ValueFromFile { get; set; }
        public double ValueForGaussSeidel { get; set; }
        public double ValueForGauss { get; set; }
        public bool IsSameAsGauss { get; set; }
        public bool IsBetterThanGauss { get; set; }
    }

    public class AlgorithmIterativePrecisionModel
    {
        public double XKey { get; set; }
        public int Iteration { get; set; }
        public double ValueFromFile { get; set; }
        public double ValueForGaussSeidel { get; set; }
        public double ValueForJacobi { get; set; }
        public bool IsSameValue { get; set; }
    }


    public class AlgoritmCheckTimeModelWithIterations : AlgoritmCheckTimeModel
    {
        public int Iteration { get; set; }
    }

    public class AlgoritmCheckTimeModel
    {
        public TimeSpan ElapsedTime { get; set; }
        public string AlgoritmType { get; set; }
        public int PointsQuantity { get; set; }
    }
}
