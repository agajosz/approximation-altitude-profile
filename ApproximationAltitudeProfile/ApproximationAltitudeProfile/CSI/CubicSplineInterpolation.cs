using System.Collections.Generic;
using System.Linq;
using GaussLinearElimination;

namespace ApproximationAltitudeProfile
{
    public class CubicSplineInterpolation
    {
        private readonly List<Point> _points;

        private readonly List<double> _unknownsSecondDerivatives;

        private List<double> _distancesBetweenXs;

        private Algorithm _algorithmType;
        private readonly int _iteratives;

        public CubicSplineInterpolation(
            List<DataPoint> dataPoints,
            Algorithm algorithType,
            int iteratives = 3)
        {
            _points = new List<Point>();
            for (int i = 0; i < dataPoints.Count; i++)
            {
                _points.Add(new Point(i, dataPoints[i].X, dataPoints[i].Y));
            }
            _unknownsSecondDerivatives = new List<double>();
            _algorithmType = algorithType;
            _iteratives = iteratives;

            GetDifferences();
            SolvingLinearEquation();
        }

        public double GetObjectiveFunction(double x)
        {
            var xk = _points.Last(c => c.X <= x);
            var xk1 = _points.ElementAt(xk.Index + 1);

            var hk = _distancesBetweenXs.ElementAt(xk.Index);

            var yppk = _unknownsSecondDerivatives.ElementAt(xk.Index);
            var yppk1 = _unknownsSecondDerivatives.ElementAt(xk1.Index);

            return GetA(xk.Y)
                   + (x - xk.X) * (GetB(hk, xk.Y, xk1.Y, yppk, yppk1) +
                                   (x - xk.X) * (GetC(yppk) + (x - xk.X) * GetD(hk, yppk, yppk1)));
        }

        private void GetDifferences()
        {
            _distancesBetweenXs = new List<double>();
            for (var i = 0; i < _points.Count - 1; i++)
            {
                _distancesBetweenXs.Add(_points.ElementAt(i + 1).X - _points.ElementAt(i).X);
            }
        }

        private static double GetA(double yk)
            => yk;

        private static double GetB(double hk, double yk, double yk1, double yppk, double yppk1)
            => (yk1 - yk) / hk - (2d * yppk + yppk1) / 6d * hk;

        private static double GetC(double yppk)
            => yppk / 2d;

        private static double GetD(double hk, double yppk, double yppk1)
            => (yppk1 - yppk) / (6d * hk);

        private void SolvingLinearEquation()
        {
            var matrixA = new double[_points.Count - 2, _points.Count - 2];
            var vectorB = new double[_points.Count - 2];

            CreateMatrixData(matrixA, vectorB);

            var matrix = new MyMatrix<double>(vectorB.Length, matrixA, vectorB);
            SolveMatrix(matrix);

            _unknownsSecondDerivatives.Add(0);
            _unknownsSecondDerivatives.AddRange(matrix.vectorXgauss);
            _unknownsSecondDerivatives.Add(0);
        }

        private void CreateMatrixData(double[,] matrixA, double[] vectorB)
        {
            for (var i = 0; i < _points.Count - 2; i++)
            {
                if (i > 0)
                {
                    matrixA[i, i - 1] = _distancesBetweenXs.ElementAt(i);
                }

                if (i + 1 < _points.Count - 2)
                {
                    matrixA[i, i + 1] = _distancesBetweenXs.ElementAt(i + 1);
                }

                matrixA[i, i] = 2 * (_distancesBetweenXs.ElementAt(i) + _distancesBetweenXs.ElementAt(i + 1));

                vectorB[i] = 6 * (
                                 (_points.ElementAt(i + 2).Y - _points.ElementAt(i + 1).Y) /
                                 _distancesBetweenXs.ElementAt(i + 1)
                                 - (_points.ElementAt(i + 1).Y - _points.ElementAt(i).Y) / _distancesBetweenXs.ElementAt(i));
            }
        }

        private void SolveMatrix(MyMatrix<double> matrix)
        {
            switch (_algorithmType)
            {
                case Algorithm.GaussPartialPivot:
                    matrix.CalculateGP();
                    break;
                case Algorithm.IterativeJacobi:
                    for (int i = 0; i < _iteratives; i++)
                    {
                        matrix.CalculateJacobi();
                    }

                    break;
                case Algorithm.IterativeGaussSeidel:
                    for (int i = 0; i < _iteratives; i++)
                    {
                        matrix.CalculateGaussSeidel();
                    }
                    break;
                case Algorithm.SparseIterativeJacobi:
                    var sparseMatrix = new double[matrix.dimensions, 3];
                    for (var i = 0; i < matrix.dimensions; i++)
                    {
                        if (i > 0)
                        {
                            sparseMatrix[i, 0] = matrix.matrixA[i, i - 1];
                        }

                        sparseMatrix[i, 1] = matrix.matrixA[i, i];
                        if (i + 1 < matrix.dimensions)
                        {
                            sparseMatrix[i, 2] = matrix.matrixA[i, i + 1];
                        }
                    }

                    for (var i = 0; i < _iteratives; i++)
                    {
                        matrix.CalculateJacobiSparse(sparseMatrix);
                    }

                    break;
                case Algorithm.SparseAlgLibraryType:
                    matrix.CalculateWithAlglib();
                    break;
                default:
                    matrix.CalculateGP();
                    break;
            }
        }
    }
}
