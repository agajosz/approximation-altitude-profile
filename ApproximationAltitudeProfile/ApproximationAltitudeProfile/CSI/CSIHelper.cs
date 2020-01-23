using System.Collections.Generic;
using System.Linq;
using GaussLinearElimination;

namespace ApproximationAltitudeProfile
{
    public partial class CubicSplineInterpolation
    {
        private void GetDistancesBetweenXs()
        {
            _distancesBetweenXs = new List<double>();
            for (var i = 0; i < _analysedPoints.Count - 1; i++)
            {
                _distancesBetweenXs.Add(_analysedPoints.ElementAt(i + 1).X - _analysedPoints.ElementAt(i).X);
            }
        }

        private static double GetA(double yk)
        {
            return yk;
        }

        private static double GetB(double hk, double yk, double yk1, double yppk, double yppk1)
        {
            return (yk1 - yk) / hk - (2d * yppk + yppk1) / 6d * hk;
        }

        private static double GetC(double yppk)
        {
            return yppk / 2d;
        }

        private static double GetD(double hk, double yppk, double yppk1)
        {
            return (yppk1 - yppk) / (6d * hk);

        }

        public double GetObjectiveFunction(double x)
        {
            var xk = _analysedPoints.Last(c => c.X <= x);
            var xk1 = _analysedPoints.ElementAt(xk.Index + 1);

            var hk = _distancesBetweenXs.ElementAt(xk.Index);

            var yppk = _secondDerivativesArray.ElementAt(xk.Index);
            var yppk1 = _secondDerivativesArray.ElementAt(xk1.Index);

            return GetA(xk.Y) + (x - xk.X) * (GetB(hk, xk.Y, xk1.Y, yppk, yppk1) + (x - xk.X) * (GetC(yppk) + (x - xk.X) * GetD(hk, yppk, yppk1)));
        }

    }
}
