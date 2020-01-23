using System.Collections.Generic;
using System.Linq;
using GaussLinearElimination;

namespace ApproximationAltitudeProfile
{
    public partial class CubicSplineInterpolation
    {

        private Algorithm _algorithmType;
        private readonly int _iteratives;

        private readonly List<Point> _analysedPoints;
        private readonly List<double> _secondDerivativesArray;
        private List<double> _distancesBetweenXs;

        public CubicSplineInterpolation(
            List<DataPoint> dataPoints,
            Algorithm algorithType,
            int iteratives = 3)
        {
            _analysedPoints = new List<Point>();
            for (int i = 0; i < dataPoints.Count; i++)
            {
                _analysedPoints.Add(new Point(i, dataPoints[i].X, dataPoints[i].Y));
            }
            _secondDerivativesArray = new List<double>();
            _algorithmType = algorithType;
            _iteratives = iteratives;

            GetDistancesBetweenXs();
            SolveLinearEquation();
        }

        private void SolveLinearEquation()
        {
            var matrixA = new double[_analysedPoints.Count - 2, _analysedPoints.Count - 2];
            var vectorB = new double[_analysedPoints.Count - 2];

            CalculateDataForMatrix(matrixA, vectorB);

            var matrix = new MyMatrix<double>(vectorB.Length, matrixA, vectorB);
            SolveMatrix(matrix);

            _secondDerivativesArray.Add(0);
            _secondDerivativesArray.AddRange(matrix.vectorXgauss);
            _secondDerivativesArray.Add(0);
        }

        private void CalculateDataForMatrix(double[,] matrixA, double[] vectorB)
        {
            for (var i = 0; i < _analysedPoints.Count - 2; i++)
            {
                if (i > 0)
                {
                    matrixA[i, i - 1] = _distancesBetweenXs.ElementAt(i);
                }

                if (i + 1 < _analysedPoints.Count - 2)
                {
                    matrixA[i, i + 1] = _distancesBetweenXs.ElementAt(i + 1);
                }

                matrixA[i, i] = 2 * (_distancesBetweenXs.ElementAt(i) + _distancesBetweenXs.ElementAt(i + 1));

                vectorB[i] = 6 * (
                                 (_analysedPoints.ElementAt(i + 2).Y - _analysedPoints.ElementAt(i + 1).Y) /
                                 _distancesBetweenXs.ElementAt(i + 1)
                                 - (_analysedPoints.ElementAt(i + 1).Y - _analysedPoints.ElementAt(i).Y) / _distancesBetweenXs.ElementAt(i));
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
