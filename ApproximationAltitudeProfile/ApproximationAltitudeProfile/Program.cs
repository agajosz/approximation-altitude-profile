using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CsvHelper;

namespace ApproximationAltitudeProfile
{
    class Program
    {
        // Procesor i5-83500h
        // 20 GB Ram
        // GPU GTX 1050 4gb
        // Visual Studio 2019 Professional
        private const string _csvFile = "Result_%points%_%route%.csv";
        static void Main(string[] args)
        {
            CheckTimeForBestAlghoritmE2();
            CheckTimeForIterativeAlghoritmE3();
            ComputeAltitudeProfileC1(new List<int> { 1, 13, 18 });
            ComputeForCheckPrecisionOffItterativeAlgoritm(new List<int> { 1, 2, 5, 10, 15, 25, 50, 75, 100, 500, 1000, 2000 });
            ComputeForCheckPrecisionOfIterativeAlgoritmWithGauss(new List<int> { 1, 2, 5, 10, 15, 25, 2000 });
            CheckAlgoritmTimes();

            Console.WriteLine("End");
            Console.ReadLine();
        }

        private static void ComputeForCheckPrecisionOffItterativeAlgoritm(List<int> iterativeList)
        {
            var resultList = new List<AlgorithmIterativePrecisionModel>();
            foreach (var iterativeNumber in iterativeList)
            {
                var points = DataParser.ParsePointData(_csvFile.Replace("%points%", "250").Replace("%route%", "1"), 1);

                var pointsToAlg = new List<DataPoint>();
                for (int i = 0; i < points.Count; i++)
                {
                    if (i % 2 == 0)
                    {
                        pointsToAlg.Add(points[i]);
                    }
                }
                var csiGaussSeidel = new CubicSplineInterpolation(pointsToAlg, AlgorithmType.IterativeGaussSeidel, iterativeNumber);
                var csiGaussJacobi = new CubicSplineInterpolation(pointsToAlg, AlgorithmType.IterativeJacobi, iterativeNumber);

                var indexes = new List<double>();
                for (double i = 0; i < points.Count - 2; i++)
                {
                    indexes.Add(i);
                }

                var valuesFromFile = indexes.Select(x => new { x, value = points.First(p => p.X == x).Y, AlgorithmType = AlgorithmType.None }).ToList();

                var expectedValuesForSeidel = indexes.Select(x => new { x, value = csiGaussSeidel.GetObjectiveFunction(x), AlgorithmType = AlgorithmType.IterativeGaussSeidel }).ToList();

                var expectedValuesForJacobi = indexes.Select(x => new { x, value = csiGaussJacobi.GetObjectiveFunction(x), AlgorithmType = AlgorithmType.IterativeJacobi }).ToList();


                resultList.AddRange(expectedValuesForSeidel
                    .Concat(valuesFromFile)
                    .Concat(expectedValuesForJacobi)
                    .GroupBy(x => x.x)
                    .Select(gl => new AlgorithmIterativePrecisionModel
                    {
                        XKey = gl.Key,
                        Iteration = iterativeNumber,
                        ValueFromFile = gl.FirstOrDefault(v => v.AlgorithmType == AlgorithmType.None).value,
                        ValueForGaussSeidel = gl.FirstOrDefault(v => v.AlgorithmType == AlgorithmType.IterativeGaussSeidel).value,
                        ValueForJacobi = gl.FirstOrDefault(v => v.AlgorithmType == AlgorithmType.IterativeJacobi).value,
                    })
                    .Where(r => r.ValueFromFile - r.ValueForGaussSeidel != 0)
                    .ToList());
            }

            foreach (var result in resultList)
            {
                result.IsSameValue = result.ValueForGaussSeidel == result.ValueForJacobi;
            }

            using (var writer = new StreamWriter($"result_check_preciscion_iterative_algo.csv"))
            using (var csv = new CsvWriter(writer))
            {
                csv.WriteRecords(resultList);
            }
        }

        private static void ComputeForCheckPrecisionOfIterativeAlgoritmWithGauss(List<int> iterativeList)
        {
            var resultList = new List<AlgorithmIterativePrecisionWithGaussModel>();
            foreach (var iterativeNumber in iterativeList)
            {
                var points = DataParser.ParsePointData(_csvFile.Replace("%points%", "250").Replace("%route%", "1"), 1);

                var pointsToAlg = new List<DataPoint>();
                for (int i = 0; i < points.Count; i++)
                {
                    if (i % 2 == 0)
                    {
                        pointsToAlg.Add(points[i]);
                    }
                }
                var csiGaussSeidel = new CubicSplineInterpolation(pointsToAlg, AlgorithmType.IterativeGaussSeidel, iterativeNumber);
                var csiGaussJacobi = new CubicSplineInterpolation(pointsToAlg, AlgorithmType.IterativeJacobi, iterativeNumber);
                var csiGaussPartialPivot = new CubicSplineInterpolation(pointsToAlg, AlgorithmType.GaussPartialPivot, iterativeNumber);

                var indexes = new List<double>();
                for (double i = 0; i < points.Count - 2; i++)
                {
                    indexes.Add(i);
                }

                var valuesFromFile = indexes.Select(x => new { x, value = points.First(p => p.X == x).Y, AlgorithmType = AlgorithmType.None }).ToList();

                var expectedValuesForSeidel = indexes.Select(x => new { x, value = csiGaussSeidel.GetObjectiveFunction(x), AlgorithmType = AlgorithmType.IterativeGaussSeidel }).ToList();

                var expectedValuesForJacobi = indexes.Select(x => new { x, value = csiGaussJacobi.GetObjectiveFunction(x), AlgorithmType = AlgorithmType.IterativeJacobi }).ToList();

                var expectedValuesForPartialPivot = indexes.Select(x => new { x, value = csiGaussPartialPivot.GetObjectiveFunction(x), AlgorithmType = AlgorithmType.GaussPartialPivot }).ToList();


                resultList.AddRange(expectedValuesForSeidel
                    .Concat(valuesFromFile)
                    .Concat(expectedValuesForJacobi)
                    .Concat(expectedValuesForPartialPivot)
                    .GroupBy(x => x.x)
                    .Select(gl => new AlgorithmIterativePrecisionWithGaussModel
                    {
                        XKey = gl.Key,
                        Iteration = iterativeNumber,
                        ValueFromFile = gl.FirstOrDefault(v => v.AlgorithmType == AlgorithmType.None).value,
                        ValueForGaussSeidel = gl.FirstOrDefault(v => v.AlgorithmType == AlgorithmType.IterativeGaussSeidel).value,
                        ValueForGauss = gl.FirstOrDefault(v => v.AlgorithmType == AlgorithmType.GaussPartialPivot).value,
                    })
                    .Where(r => r.ValueFromFile - r.ValueForGaussSeidel != 0)
                    .ToList());
            }

            foreach (var result in resultList)
            {
                result.IsBetterThanGauss = Math.Abs((result.ValueForGauss - result.ValueFromFile) / result.ValueFromFile) >
                                           Math.Abs((result.ValueForGaussSeidel - result.ValueFromFile) / result.ValueFromFile);

                result.IsSameAsGauss = (result.ValueForGauss == result.ValueForGaussSeidel);
            }

            using (var writer = new StreamWriter($"result_check_preciscion_iterative_algo_with_gauss.csv"))
            using (var csv = new CsvWriter(writer))
            {
                csv.WriteRecords(resultList);
            }
        }

        static void ComputeAltitudeProfileC1(List<int> routeNumbers)
        {
            var pointsToAlgo = new List<int> { 50, 250, 500 };
            foreach (var routeNumber in routeNumbers)
            {
                foreach (var pointsQuantity in pointsToAlgo)
                {
                    var pointsFromRoute = DataParser.ParsePointData(_csvFile.Replace("%points%", pointsQuantity.ToString()).Replace("%route%", routeNumber.ToString()), routeNumber);

                    var pointsGivenToAlgo = new List<DataPoint>();
                    for (int i = 0; i < pointsFromRoute.Count; i++)
                    {
                        if (i % 2 == 0)
                        {
                            pointsGivenToAlgo.Add(pointsFromRoute[i]);
                        }
                    }

                    var csiGaussSeidel = new CubicSplineInterpolation(pointsGivenToAlgo, AlgorithmType.IterativeGaussSeidel);
                    var csiGaussJacobi = new CubicSplineInterpolation(pointsGivenToAlgo, AlgorithmType.IterativeJacobi);
                    var csiGaussPartialPivot = new CubicSplineInterpolation(pointsGivenToAlgo, AlgorithmType.GaussPartialPivot);
                    var csiSparseIterativeJacobi = new CubicSplineInterpolation(pointsGivenToAlgo, AlgorithmType.SparseIterativeJacobi);
                    var alglib = new CubicSplineInterpolation(pointsGivenToAlgo, AlgorithmType.SparseAlgLibraryType);

                    var indexes = new List<double>();
                    for (double i = 0; i < pointsFromRoute.Count - 2; i++)
                    {
                        indexes.Add(i);
                    }

                    var valuesFromFile = indexes.Select(x => new { x, value = pointsFromRoute.First(p => p.X == x).Y, AlgorithmType = AlgorithmType.None }).ToList();

                    var expectedValuesForAlglib = indexes.Select(x => new { x, value = alglib.GetObjectiveFunction(x), AlgorithmType = AlgorithmType.SparseAlgLibraryType }).ToList();

                    var expectedValuesForSeidel = indexes.Select(x => new { x, value = csiGaussSeidel.GetObjectiveFunction(x), AlgorithmType = AlgorithmType.IterativeGaussSeidel }).ToList();

                    var expectedValuesForJacobi = indexes.Select(x => new { x, value = csiGaussJacobi.GetObjectiveFunction(x), AlgorithmType = AlgorithmType.IterativeJacobi }).ToList();

                    var expectedValuesForPartialPivot = indexes.Select(x => new { x, value = csiGaussPartialPivot.GetObjectiveFunction(x), AlgorithmType = AlgorithmType.GaussPartialPivot }).ToList();

                    var expectedValuesForSparseGaussSeidel = indexes.Select(x => new { x, value = csiSparseIterativeJacobi.GetObjectiveFunction(x), AlgorithmType = AlgorithmType.SparseIterativeJacobi }).ToList();

                    var groupedList = expectedValuesForSeidel
                        .Concat(expectedValuesForAlglib)
                        .Concat(valuesFromFile)
                        .Concat(expectedValuesForJacobi)
                        .Concat(expectedValuesForPartialPivot)
                        .Concat(expectedValuesForSparseGaussSeidel)
                        .GroupBy(x => x.x)
                        .Select(gl => new
                        {
                            gl.Key,
                            ValueFromFile = gl.FirstOrDefault(v => v.AlgorithmType == AlgorithmType.None).value,
                            Alglib = gl.FirstOrDefault(v => v.AlgorithmType == AlgorithmType.SparseAlgLibraryType).value,
                            PartialPivot = gl.FirstOrDefault(v => v.AlgorithmType == AlgorithmType.GaussPartialPivot).value,
                            GaussSeider = gl.FirstOrDefault(v => v.AlgorithmType == AlgorithmType.IterativeGaussSeidel).value,
                            Jacobi = gl.FirstOrDefault(v => v.AlgorithmType == AlgorithmType.IterativeJacobi).value,
                            SparseJacobie = gl.FirstOrDefault(v => v.AlgorithmType == AlgorithmType.SparseIterativeJacobi).value
                        })
                        .Where(r => r.ValueFromFile - r.GaussSeider != 0)
                        .ToList();


                    using (var writer = new StreamWriter($"result_t{routeNumber}_{pointsQuantity}.csv"))
                    using (var csv = new CsvWriter(writer))
                    {
                        csv.WriteRecords(groupedList);
                    }

                    var errors = groupedList.Select(gl => new
                    {
                        AbsoluteErrorForAlgLib = Math.Abs(gl.Alglib - gl.ValueFromFile),
                        RelativeErrorForAlgLib = Math.Abs((gl.Alglib - gl.ValueFromFile) / gl.ValueFromFile),
                        AbsoluteErrorForJacobi = Math.Abs(gl.Jacobi - gl.ValueFromFile),
                        RelativeErrorForJacobi = Math.Abs((gl.Jacobi - gl.ValueFromFile) / gl.ValueFromFile),
                        AbsoluteErrorForSparseJacobi = Math.Abs(gl.SparseJacobie - gl.ValueFromFile),
                        RelativeErrorForSparseJacobi = Math.Abs((gl.SparseJacobie - gl.ValueFromFile) / gl.ValueFromFile),
                        AbsoluteErrorForPartialPivot = Math.Abs(gl.PartialPivot - gl.ValueFromFile),
                        RelativeErrorForPartialPivot = Math.Abs((gl.PartialPivot - gl.ValueFromFile) / gl.ValueFromFile),
                        AbsoluteErrorForGaussSeidel = Math.Abs(gl.GaussSeider - gl.ValueFromFile),
                        RelativeErrorForGaussSeidel = Math.Abs((gl.GaussSeider - gl.ValueFromFile) / gl.ValueFromFile),
                    })
                        .ToList();

                    using (var writer = new StreamWriter($"errors_t{routeNumber}_{pointsQuantity}.csv"))
                    using (var csv = new CsvWriter(writer))
                    {
                        csv.WriteRecords(errors);
                    }
                }
            }
        }

        static void CheckAlgoritmTimes()
        {
            var timeElapsedList = new List<AlgoritmCheckTimeModel>();
            var pointsFromRoute = DataParser.ParsePointData(_csvFile.Replace("%points%", "250").Replace("%route%", "1"), 1);
            for (int j = 0; j < 5; j++)
            {
                var pointsGivenToAlgo = new List<DataPoint>();
                for (int i = 0; i < 1 + 2 * j; i++)
                {
                    pointsGivenToAlgo.AddRange(pointsFromRoute);
                }

                var stopWatch = new Stopwatch();
                stopWatch.Start();
                var csiGaussSeidel =
                    new CubicSplineInterpolation(pointsGivenToAlgo, AlgorithmType.IterativeGaussSeidel);
                stopWatch.Stop();
                timeElapsedList.Add(new AlgoritmCheckTimeModel
                {
                    ElapsedTime = stopWatch.Elapsed,
                    PointsQuantity = pointsGivenToAlgo.Count,
                    AlgoritmType = AlgorithmType.IterativeGaussSeidel.ToString(),
                });

                stopWatch.Reset();
                stopWatch.Start();
                var csiGaussJacobi = new CubicSplineInterpolation(pointsGivenToAlgo, AlgorithmType.IterativeJacobi);
                stopWatch.Stop();

                timeElapsedList.Add(new AlgoritmCheckTimeModel
                {
                    ElapsedTime = stopWatch.Elapsed,
                    PointsQuantity = pointsGivenToAlgo.Count,
                    AlgoritmType = AlgorithmType.IterativeJacobi.ToString(),
                });

                stopWatch.Reset();
                stopWatch.Start();
                var csiGaussPartialPivot =
                    new CubicSplineInterpolation(pointsGivenToAlgo, AlgorithmType.GaussPartialPivot);
                stopWatch.Stop();

                timeElapsedList.Add(new AlgoritmCheckTimeModel
                {
                    ElapsedTime = stopWatch.Elapsed,
                    AlgoritmType = AlgorithmType.GaussPartialPivot.ToString(),
                    PointsQuantity = pointsGivenToAlgo.Count
                });

                stopWatch.Reset();
                stopWatch.Start();
                var csiSparseIterativeJacobi = new CubicSplineInterpolation(pointsGivenToAlgo, AlgorithmType.SparseIterativeJacobi);
                stopWatch.Stop();

                timeElapsedList.Add(new AlgoritmCheckTimeModel
                {
                    ElapsedTime = stopWatch.Elapsed,
                    AlgoritmType = AlgorithmType.SparseIterativeJacobi.ToString(),
                    PointsQuantity = pointsGivenToAlgo.Count
                });

                stopWatch.Reset();
                stopWatch.Start();
                var alglib = new CubicSplineInterpolation(pointsGivenToAlgo, AlgorithmType.SparseAlgLibraryType);
                stopWatch.Stop();
                stopWatch.Stop();

                timeElapsedList.Add(new AlgoritmCheckTimeModel
                {
                    ElapsedTime = stopWatch.Elapsed,
                    AlgoritmType = AlgorithmType.SparseAlgLibraryType.ToString(),
                    PointsQuantity = pointsGivenToAlgo.Count
                });
            }

            using (var writer = new StreamWriter($"times_algr_with_gauss.csv"))
            using (var csv = new CsvWriter(writer))
            {
                csv.WriteRecords(timeElapsedList);
            }
        }

        static void CheckTimeForBestAlghoritmE2()
        {
            var stopWatch = new Stopwatch();

            var timeElapsedList = new List<AlgoritmCheckTimeModelWithIterations>();
            var pointsFromRoute = DataParser.ParsePointData(_csvFile.Replace("%points%", "500").Replace("%route%", "1"), 1);

            for (int j = 0; j < 4; j++)
            {
                var pointsGivenToAlgo = new List<DataPoint>();
                for (int i = 0; i < 25 + 25 * j; i++)
                {
                    pointsGivenToAlgo.AddRange(pointsFromRoute);
                }

                stopWatch.Start();
                var csiSparseIterativeJacobi =
                    new CubicSplineInterpolation(pointsGivenToAlgo, AlgorithmType.SparseIterativeJacobi, 3);
                stopWatch.Stop();

                timeElapsedList.Add(new AlgoritmCheckTimeModelWithIterations
                {
                    ElapsedTime = stopWatch.Elapsed,
                    PointsQuantity = pointsGivenToAlgo.Count,
                    AlgoritmType = AlgorithmType.SparseIterativeJacobi.ToString(),
                });
                stopWatch.Reset();
            }

            using (var writer = new StreamWriter($"times_best_algr.csv"))
            using (var csv = new CsvWriter(writer))
            {
                csv.WriteRecords(timeElapsedList);
            }
        }
        static void CheckTimeForIterativeAlghoritmE3()
        {
            var timeElapsedList = new List<AlgoritmCheckTimeModelWithIterations>();
            var pointsFromRoute = DataParser.ParsePointData(_csvFile.Replace("%points%", "500").Replace("%route%", "1"), 1);

            var pointsGivenToAlgo = new List<DataPoint>();
            for (int i = 0; i < 21; i++)
            {
                pointsGivenToAlgo.AddRange(pointsFromRoute);
            }


            for (int i = 3; i < 40; i += 3)
            {
                var stopWatch = new Stopwatch();
                stopWatch.Start();
                var csiGaussSeidel =
                    new CubicSplineInterpolation(pointsGivenToAlgo, AlgorithmType.IterativeGaussSeidel, i);
                stopWatch.Stop();
                timeElapsedList.Add(new AlgoritmCheckTimeModelWithIterations
                {
                    ElapsedTime = stopWatch.Elapsed,
                    PointsQuantity = pointsGivenToAlgo.Count,
                    AlgoritmType = AlgorithmType.IterativeGaussSeidel.ToString(),
                    Iteration = i
                });

                stopWatch.Reset();
                stopWatch.Start();
                var csiGaussJacobi = new CubicSplineInterpolation(pointsGivenToAlgo, AlgorithmType.IterativeJacobi, i);
                stopWatch.Stop();

                timeElapsedList.Add(new AlgoritmCheckTimeModelWithIterations
                {
                    ElapsedTime = stopWatch.Elapsed,
                    PointsQuantity = pointsGivenToAlgo.Count,
                    AlgoritmType = AlgorithmType.IterativeJacobi.ToString(),
                    Iteration = i
                });

                stopWatch.Reset();
                stopWatch.Start();
                var csiSparseIterativeJacobi =
                    new CubicSplineInterpolation(pointsGivenToAlgo, AlgorithmType.SparseIterativeJacobi, i);
                stopWatch.Stop();

                timeElapsedList.Add(new AlgoritmCheckTimeModelWithIterations
                {
                    ElapsedTime = stopWatch.Elapsed,
                    PointsQuantity = pointsGivenToAlgo.Count,
                    AlgoritmType = AlgorithmType.SparseIterativeJacobi.ToString(),
                    Iteration = i
                });
            }

            using (var writer = new StreamWriter($"times_iterative_algr.csv"))
            using (var csv = new CsvWriter(writer))
            {
                csv.WriteRecords(timeElapsedList);
            }

        }
    }
}
