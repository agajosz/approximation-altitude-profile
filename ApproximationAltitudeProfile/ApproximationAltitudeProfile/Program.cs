using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ApproximationAltitudeProfile.AlgorithmModels;
using CsvHelper;

namespace ApproximationAltitudeProfile
{
    class Program
    {
        private const string _csvFile = "Result_%points%_%route%.csv";
        static void Main(string[] args)
        {
//             TimeCheckBestAlgorithm_E2();
//             TimeCheckIterativeAlg_E3();
//             AltProfileComputation_C1(new List<int> { 2, 5, 14 });
//             PrecisionCheckIterativeAlgoritms(new List<int> { 1, 2, 5, 10, 15, 25, 50, 75, 100, 500, 1000, 2000 });
//             PrecisionCheckIterativeAlgoritmsAndGauss(new List<int> { 1, 2, 5, 10, 15, 25, 2000 });
//             CheckAlgoritmTimes();
            CheckAlgorithmTimesNoGaussMorePoints();

            Console.WriteLine("Finished calculations, generated results data.");
            Console.ReadLine();
        }

        // obliczenie 4 przykladowych wartosci, by oszacowac ilosc wezlow do przetworzenia w 30min
        static void TimeCheckBestAlgorithm_E2()
        {
            var stopWatch = new Stopwatch();

            var timeElapsedList = new List<AlgoritmCheckTimeWithIterations>();
            var routeKnots = Parser.ParseKnotData(_csvFile.Replace("%points%", "500").Replace("%route%", "2"));

            for (int j = 0; j < 4; j++)
            {
                var providedKnots = new List<DataPoint>();
                for (int i = 0; i < 25 + 25 * j; i++)
                {
                    providedKnots.AddRange(routeKnots);
                }

                stopWatch.Start();
                var _ = new CubicSplineInterpolation(providedKnots, Algorithm.SparseIterativeJacobi, 3);
                stopWatch.Stop();

                timeElapsedList.Add(new AlgoritmCheckTimeWithIterations
                {
                    ElapsedTime = stopWatch.Elapsed,
                    PointsQuantity = providedKnots.Count,
                    AlgorithmType = Algorithm.SparseIterativeJacobi.ToString(),
                });
                stopWatch.Reset();
            }

            using (var writer = new StreamWriter($"times_best_algr.csv"))
            using (var csv = new CsvWriter(writer))
            {
                csv.WriteRecords(timeElapsedList);
            }
        }

        // zaleznosc czasu wykonania od ilosci iteracji i uzyskiwanej dokladnosci
        static void TimeCheckIterativeAlg_E3()
        {
            var timeElapsedList = new List<AlgoritmCheckTimeWithIterations>();
            var pointsFromRoute = Parser.ParseKnotData(_csvFile.Replace("%points%", "500").Replace("%route%", "2"));

            var pointsGivenToAlgo = new List<DataPoint>();
            for (int i = 0; i < 21; i++)
            {
                pointsGivenToAlgo.AddRange(pointsFromRoute);
            }

            for (int i = 3; i < 40; i += 3)
            {
                var stopWatch = new Stopwatch();
                stopWatch.Start();
                var _1 = new CubicSplineInterpolation(pointsGivenToAlgo, Algorithm.IterativeGaussSeidel, i);
                stopWatch.Stop();
                timeElapsedList.Add(new AlgoritmCheckTimeWithIterations
                {
                    ElapsedTime = stopWatch.Elapsed,
                    PointsQuantity = pointsGivenToAlgo.Count,
                    AlgorithmType = Algorithm.IterativeGaussSeidel.ToString(),
                    Iteration = i
                });

                stopWatch.Reset();
                stopWatch.Start();
                var _2 = new CubicSplineInterpolation(pointsGivenToAlgo, Algorithm.IterativeJacobi, i);
                stopWatch.Stop();

                timeElapsedList.Add(new AlgoritmCheckTimeWithIterations
                {
                    ElapsedTime = stopWatch.Elapsed,
                    PointsQuantity = pointsGivenToAlgo.Count,
                    AlgorithmType = Algorithm.IterativeJacobi.ToString(),
                    Iteration = i
                });

                stopWatch.Reset();
                stopWatch.Start();
                var _3 = new CubicSplineInterpolation(pointsGivenToAlgo, Algorithm.SparseIterativeJacobi, i);
                stopWatch.Stop();

                timeElapsedList.Add(new AlgoritmCheckTimeWithIterations
                {
                    ElapsedTime = stopWatch.Elapsed,
                    PointsQuantity = pointsGivenToAlgo.Count,
                    AlgorithmType = Algorithm.SparseIterativeJacobi.ToString(),
                    Iteration = i
                });
            }

            using (var writer = new StreamWriter($"times_iterative_algr.csv"))
            using (var csv = new CsvWriter(writer))
            {
                csv.WriteRecords(timeElapsedList);
            }

        }

        // obliczenie profili wysokosciowych i bledow interpolacji dla tras 2, 5, 14 i liczby wezlow: 50, 250, 500
        static void AltProfileComputation_C1(List<int> routeNumbers)
        {
            var pointsToAlgo = new List<int> { 50, 250, 500 };
            foreach (var routeNumber in routeNumbers)
            {
                foreach (var pointsQuantity in pointsToAlgo)
                {
                    var pointsFromRoute = Parser.ParseKnotData(_csvFile.Replace("%points%", pointsQuantity.ToString()).Replace("%route%", routeNumber.ToString()));

                    var pointsGivenToAlgo = new List<DataPoint>();
                    for (int i = 0; i < pointsFromRoute.Count; i++)
                    {
                        if (i % 2 == 0)
                        {
                            pointsGivenToAlgo.Add(pointsFromRoute[i]);
                        }
                    }

                    var csiGaussSeidel = new CubicSplineInterpolation(pointsGivenToAlgo, Algorithm.IterativeGaussSeidel);
                    var csiGaussJacobi = new CubicSplineInterpolation(pointsGivenToAlgo, Algorithm.IterativeJacobi);
                    var csiGaussPartialPivot = new CubicSplineInterpolation(pointsGivenToAlgo, Algorithm.GaussPartialPivot);
                    var csiSparseIterativeJacobi = new CubicSplineInterpolation(pointsGivenToAlgo, Algorithm.SparseIterativeJacobi);
                    var alglib = new CubicSplineInterpolation(pointsGivenToAlgo, Algorithm.SparseAlgLibraryType);

                    var indexes = new List<double>();
                    for (double i = 0; i < pointsFromRoute.Count - 2; i++)
                    {
                        indexes.Add(i);
                    }

                    var valuesFromFile = indexes.Select(x => new { x, value = pointsFromRoute.First(p => p.X == x).Y, AlgorithmType = Algorithm.None }).ToList();

                    var expectedValuesForAlglib = indexes.Select(x => new { x, value = alglib.GetObjectiveFunction(x), AlgorithmType = Algorithm.SparseAlgLibraryType }).ToList();

                    var expectedValuesForSeidel = indexes.Select(x => new { x, value = csiGaussSeidel.GetObjectiveFunction(x), AlgorithmType = Algorithm.IterativeGaussSeidel }).ToList();

                    var expectedValuesForJacobi = indexes.Select(x => new { x, value = csiGaussJacobi.GetObjectiveFunction(x), AlgorithmType = Algorithm.IterativeJacobi }).ToList();

                    var expectedValuesForPartialPivot = indexes.Select(x => new { x, value = csiGaussPartialPivot.GetObjectiveFunction(x), AlgorithmType = Algorithm.GaussPartialPivot }).ToList();

                    var expectedValuesForSparseGaussSeidel = indexes.Select(x => new { x, value = csiSparseIterativeJacobi.GetObjectiveFunction(x), AlgorithmType = Algorithm.SparseIterativeJacobi }).ToList();

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
                            ValueFromFile = gl.FirstOrDefault(v => v.AlgorithmType == Algorithm.None).value,
                            Alglib = gl.FirstOrDefault(v => v.AlgorithmType == Algorithm.SparseAlgLibraryType).value,
                            PartialPivot = gl.FirstOrDefault(v => v.AlgorithmType == Algorithm.GaussPartialPivot).value,
                            GaussSeider = gl.FirstOrDefault(v => v.AlgorithmType == Algorithm.IterativeGaussSeidel).value,
                            Jacobi = gl.FirstOrDefault(v => v.AlgorithmType == Algorithm.IterativeJacobi).value,
                            SparseJacobie = gl.FirstOrDefault(v => v.AlgorithmType == Algorithm.SparseIterativeJacobi).value
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

        // obliczenie profili i porownanie oczekiwanych wartosci z otrzymanymi dla algorytmow iterowanych
        private static void PrecisionCheckIterativeAlgoritms(List<int> iterativeList)
        {
            var resultList = new List<AlgorithmIterativePrecision>();
            foreach (var iterativeNumber in iterativeList)
            {
                var knots = Parser.ParseKnotData(_csvFile.Replace("%points%", "250").Replace("%route%", "2"));

                var knotsProvided = new List<DataPoint>();
                for (int i = 0; i < knots.Count; i++)
                {
                    if (i % 2 == 0)
                    {
                        knotsProvided.Add(knots[i]);
                    }
                }
                var csiGaussSeidel = new CubicSplineInterpolation(knotsProvided, Algorithm.IterativeGaussSeidel, iterativeNumber);
                var csiGaussJacobi = new CubicSplineInterpolation(knotsProvided, Algorithm.IterativeJacobi, iterativeNumber);

                var indexes = new List<double>();
                for (double i = 0; i < knots.Count - 2; i++)
                {
                    indexes.Add(i);
                }

                var valuesFromFile = indexes.Select(x => new { x, value = knots.First(p => p.X == x).Y, AlgorithmType = Algorithm.None }).ToList();

                var expectedValuesForSeidel = indexes.Select(x => new { x, value = csiGaussSeidel.GetObjectiveFunction(x), AlgorithmType = Algorithm.IterativeGaussSeidel }).ToList();

                var expectedValuesForJacobi = indexes.Select(x => new { x, value = csiGaussJacobi.GetObjectiveFunction(x), AlgorithmType = Algorithm.IterativeJacobi }).ToList();


                resultList.AddRange(expectedValuesForSeidel
                    .Concat(valuesFromFile)
                    .Concat(expectedValuesForJacobi)
                    .GroupBy(x => x.x)
                    .Select(gl => new AlgorithmIterativePrecision
                    {
                        XKey = gl.Key,
                        Iteration = iterativeNumber,
                        ValueFromFile = gl.FirstOrDefault(v => v.AlgorithmType == Algorithm.None).value,
                        ValueForGaussSeidel = gl.FirstOrDefault(v => v.AlgorithmType == Algorithm.IterativeGaussSeidel).value,
                        ValueForJacobi = gl.FirstOrDefault(v => v.AlgorithmType == Algorithm.IterativeJacobi).value,
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

        // obliczenie profili i porownanie oczekiwanych wartosci z otrzymanymi dla algorytmow iterowanych i porownanie z PartialGauss
        private static void PrecisionCheckIterativeAlgoritmsAndGauss(List<int> iterativeList)
        {
            var resultList = new List<AlgorithmIterativePrecisionWithGauss>();
            foreach (var iterativeNumber in iterativeList)
            {
                var points = Parser.ParseKnotData(_csvFile.Replace("%points%", "250").Replace("%route%", "2"));

                var pointsToAlg = new List<DataPoint>();
                for (int i = 0; i < points.Count; i++)
                {
                    if (i % 2 == 0)
                    {
                        pointsToAlg.Add(points[i]);
                    }
                }
                var csiGaussSeidel = new CubicSplineInterpolation(pointsToAlg, Algorithm.IterativeGaussSeidel, iterativeNumber);
                var csiGaussJacobi = new CubicSplineInterpolation(pointsToAlg, Algorithm.IterativeJacobi, iterativeNumber);
                var csiGaussPartialPivot = new CubicSplineInterpolation(pointsToAlg, Algorithm.GaussPartialPivot, iterativeNumber);

                var indexes = new List<double>();
                for (double i = 0; i < points.Count - 2; i++)
                {
                    indexes.Add(i);
                }

                var valuesFromFile = indexes.Select(x => new { x, value = points.First(p => p.X == x).Y, AlgorithmType = Algorithm.None }).ToList();

                var expectedValuesForSeidel = indexes.Select(x => new { x, value = csiGaussSeidel.GetObjectiveFunction(x), AlgorithmType = Algorithm.IterativeGaussSeidel }).ToList();

                var expectedValuesForJacobi = indexes.Select(x => new { x, value = csiGaussJacobi.GetObjectiveFunction(x), AlgorithmType = Algorithm.IterativeJacobi }).ToList();

                var expectedValuesForPartialPivot = indexes.Select(x => new { x, value = csiGaussPartialPivot.GetObjectiveFunction(x), AlgorithmType = Algorithm.GaussPartialPivot }).ToList();


                resultList.AddRange(expectedValuesForSeidel
                    .Concat(valuesFromFile)
                    .Concat(expectedValuesForJacobi)
                    .Concat(expectedValuesForPartialPivot)
                    .GroupBy(x => x.x)
                    .Select(gl => new AlgorithmIterativePrecisionWithGauss
                    {
                        XKey = gl.Key,
                        Iteration = iterativeNumber,
                        ValueFromFile = gl.FirstOrDefault(v => v.AlgorithmType == Algorithm.None).value,
                        ValueForGaussSeidel = gl.FirstOrDefault(v => v.AlgorithmType == Algorithm.IterativeGaussSeidel).value,
                        ValueForGauss = gl.FirstOrDefault(v => v.AlgorithmType == Algorithm.GaussPartialPivot).value,
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

            using (var writer = new StreamWriter($"result_precision_gauss_iterative.csv"))
            using (var csv = new CsvWriter(writer))
            {
                csv.WriteRecords(resultList);
            }
        }

        // sprawdza czasy wykonywania poszczegolonych algorytmow dla trasy nr 2 przy 250 węzłach
        static void CheckAlgoritmTimes() 
        {
            var timeElapsedList = new List<AlgoritmCheckTime>();
            var pointsFromRoute = Parser.ParseKnotData(_csvFile.Replace("%points%", "250").Replace("%route%", "2"));
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
                    new CubicSplineInterpolation(pointsGivenToAlgo, Algorithm.IterativeGaussSeidel);
                stopWatch.Stop();
                timeElapsedList.Add(new AlgoritmCheckTime
                {
                    ElapsedTime = stopWatch.Elapsed,
                    PointsQuantity = pointsGivenToAlgo.Count,
                    AlgorithmType = Algorithm.IterativeGaussSeidel.ToString(),
                });

                stopWatch.Reset();
                stopWatch.Start();
                var csiGaussJacobi = new CubicSplineInterpolation(pointsGivenToAlgo, Algorithm.IterativeJacobi);
                stopWatch.Stop();

                timeElapsedList.Add(new AlgoritmCheckTime
                {
                    ElapsedTime = stopWatch.Elapsed,
                    PointsQuantity = pointsGivenToAlgo.Count,
                    AlgorithmType = Algorithm.IterativeJacobi.ToString(),
                });

                stopWatch.Reset();
                stopWatch.Start();
                var csiGaussPartialPivot =
                    new CubicSplineInterpolation(pointsGivenToAlgo, Algorithm.GaussPartialPivot);
                stopWatch.Stop();

                timeElapsedList.Add(new AlgoritmCheckTime
                {
                    ElapsedTime = stopWatch.Elapsed,
                    AlgorithmType = Algorithm.GaussPartialPivot.ToString(),
                    PointsQuantity = pointsGivenToAlgo.Count
                });

                stopWatch.Reset();
                stopWatch.Start();
                var csiSparseIterativeJacobi = new CubicSplineInterpolation(pointsGivenToAlgo, Algorithm.SparseIterativeJacobi);
                stopWatch.Stop();

                timeElapsedList.Add(new AlgoritmCheckTime
                {
                    ElapsedTime = stopWatch.Elapsed,
                    AlgorithmType = Algorithm.SparseIterativeJacobi.ToString(),
                    PointsQuantity = pointsGivenToAlgo.Count
                });

                stopWatch.Reset();
                stopWatch.Start();
                var alglib = new CubicSplineInterpolation(pointsGivenToAlgo, Algorithm.SparseAlgLibraryType);
                stopWatch.Stop();
                stopWatch.Stop();

                timeElapsedList.Add(new AlgoritmCheckTime
                {
                    ElapsedTime = stopWatch.Elapsed,
                    AlgorithmType = Algorithm.SparseAlgLibraryType.ToString(),
                    PointsQuantity = pointsGivenToAlgo.Count
                });
            }

            using (var writer = new StreamWriter($"times_algr_with_gauss.csv"))
            using (var csv = new CsvWriter(writer))
            {
                csv.WriteRecords(timeElapsedList);
            }
        }
        
        // sprawdza czasy wykonywania poszczegolonych algorytmow dla trasy nr 2 przy 100 < knots <= 1100
        static void CheckAlgorithmTimesNoGaussMorePoints() 
        {
            var timeElapsedList = new List<AlgoritmCheckTime>();
            var pointsFromRoute = Parser.ParseKnotData(_csvFile.Replace("%points%", "10000").Replace("%route%", "2"));
            var pointsGivenToAlgo = new List<DataPoint>();
            for (int i = 100; i <= 10000; i++)
            {
                if (i % 500 == 0) {
                    pointsGivenToAlgo.AddRange(pointsFromRoute);
                    
                    var stopWatch = new Stopwatch();
                    stopWatch.Start();
                    var _1 = new CubicSplineInterpolation(pointsGivenToAlgo, Algorithm.IterativeGaussSeidel);
                    stopWatch.Stop();
                    timeElapsedList.Add(new AlgoritmCheckTime
                    {
                        ElapsedTime = stopWatch.Elapsed,
                        PointsQuantity = pointsGivenToAlgo.Count,
                        AlgorithmType = Algorithm.IterativeGaussSeidel.ToString(),
                    });

                    stopWatch.Reset();
                    stopWatch.Start();
                    var _2 = new CubicSplineInterpolation(pointsGivenToAlgo, Algorithm.IterativeJacobi);
                    stopWatch.Stop();

                    timeElapsedList.Add(new AlgoritmCheckTime
                    {
                        ElapsedTime = stopWatch.Elapsed,
                        PointsQuantity = pointsGivenToAlgo.Count,
                        AlgorithmType = Algorithm.IterativeJacobi.ToString(),
                    });

                    stopWatch.Reset();
                    stopWatch.Start();
                    var _3 = new CubicSplineInterpolation(pointsGivenToAlgo, Algorithm.GaussPartialPivot);
                    stopWatch.Stop();

                    timeElapsedList.Add(new AlgoritmCheckTime
                    {
                        ElapsedTime = stopWatch.Elapsed,
                        AlgorithmType = Algorithm.GaussPartialPivot.ToString(),
                        PointsQuantity = pointsGivenToAlgo.Count
                    });

                    stopWatch.Reset();
                    stopWatch.Start();
                    var _4 = new CubicSplineInterpolation(pointsGivenToAlgo, Algorithm.SparseIterativeJacobi);
                    stopWatch.Stop();

                    timeElapsedList.Add(new AlgoritmCheckTime
                    {
                        ElapsedTime = stopWatch.Elapsed,
                        AlgorithmType = Algorithm.SparseIterativeJacobi.ToString(),
                        PointsQuantity = pointsGivenToAlgo.Count
                    });

                    stopWatch.Reset();
                    stopWatch.Start();
                    var _5 = new CubicSplineInterpolation(pointsGivenToAlgo, Algorithm.SparseAlgLibraryType);
                    stopWatch.Stop();
                    stopWatch.Stop();

                    timeElapsedList.Add(new AlgoritmCheckTime
                    {
                        ElapsedTime = stopWatch.Elapsed,
                        AlgorithmType = Algorithm.SparseAlgLibraryType.ToString(),
                        PointsQuantity = pointsGivenToAlgo.Count
                    });
                }
             
            }

            using (var writer = new StreamWriter($"times_algr_no_gauss_more_data.csv"))
            using (var csv = new CsvWriter(writer))
            {
                csv.WriteRecords(timeElapsedList);
            }
        }
    }
}
