using System;
using System.Numerics;

namespace GaussLinearElimination
{
    // Procesor i5-83500h
    // 20 GB Ram
    // GPU GTX 1050 4gb
    // Visual Studio 2019 Professional
    class Program
    {
        public static int MaxValue = 65536; // 2^16

        static void Main(string[] args)
        {
            Random rand = new Random();
            for (int k = 3; k < 500; k++) // k do 5000
            {
                var matrix = new int[k, k];
                var vector = new int[k];
                for (int i = 0; i < k; i++)
                {
                    matrix[i, 0] = (rand.Next(-MaxValue, MaxValue - 1));
                    vector[i] = (rand.Next(-MaxValue, MaxValue - 1));
                    for (int j = 0; j < k; j++)
                    {
                        matrix[i, j] = (rand.Next(-MaxValue, MaxValue - 1));
                    }
                    //matrix[i, 0] = (rand.Next(-5, 5 - 1));
                    //vector[i] = (rand.Next(-5, 5 - 1));
                    //for (int j = 0; j < k; j++)
                    //{
                    //    matrix[i, j] = (rand.Next(-5, 5 - 1));
                    //}
                }
                //ComputeGaussForInt(matrix, vector);
                ComputeGaussForDouble(matrix, vector);

            }
        }

        public static void ComputeGaussForInt(int[,] matrix, int[] vector)
        {
            var matrixClass = new MyMatrix<int>(vector.Length, matrix, vector);
            matrixClass.CalculateG();

            var result = matrixClass.vectorXgauss;
        }

        //tf
        public static void ComputeGaussForFloat(int[,] matrix, int[] vector)
        {
            float[,] floatMatrix = new float[vector.Length, vector.Length];
            float[] floatVector = new float[vector.Length];
            for (int i = 0; i < vector.Length; i++)
            {
                floatMatrix[i, 0] = (float)matrix[i, 0] / MaxValue;
                floatVector[i] = (float)vector[i];
                for (int j = 0; j < vector.Length; j++)
                {
                    floatMatrix[i, j] = (float)matrix[i, j] / MaxValue;
                }
            }

            var matrixClass = new MyMatrix<float>(floatVector.Length, floatMatrix, floatVector);
            //for g

            matrixClass.CalculateG();


            //for pg

            //for fg
        }

        //td
        public static void ComputeGaussForDouble(int[,] matrix, int[] vector)
        {
            double[,] doubleMatrix = new double[vector.Length, vector.Length];
            double[] doubleVector = new double[vector.Length];
            for (int i = 0; i < vector.Length; i++)
            {
                doubleVector[i] = (double)vector[i] / MaxValue;

                for (int j = 0; j < vector.Length; j++)
                {
                    doubleMatrix[i, j] = (double)matrix[i, j] / MaxValue;
                }
            }


            double[,] matrixx = new double[vector.Length, vector.Length];
            double[] vectorr = new double[vector.Length];
            for (int i = 0; i < vector.Length; i++)
            {
                vectorr[i] = doubleVector[i];

                for (int j = 0; j < vector.Length; j++)
                {
                    matrixx[i, j] = doubleMatrix[i, j];
                }
            }

            var matrixClass = new MyMatrix<double>(vector.Length, matrixx, vectorr);

            matrixClass.CalculateG();

            var result = matrixClass.vectorXgauss;


            //for g

            //for pg

            //for fg
        }

        //tc
        public static void ComputeGaussForMyFraction(int[,] matrix, int[] vector)
        {
            MyFraction[,] myFractionMatrix = new MyFraction[vector.Length, vector.Length];
            MyFraction[] myFractionVector = new MyFraction[vector.Length];
            for (int i = 0; i < vector.Length; i++)
            {
                myFractionMatrix[i, 0].Numerator = matrix[i, 0];
                myFractionMatrix[i, 0].Denominator = MaxValue;
                myFractionVector[i].Numerator = vector[i];
                myFractionVector[i].Denominator = MaxValue;
                for (int j = 0; j < vector.Length; j++)
                {
                    myFractionMatrix[i, j].Numerator = matrix[i, j];
                    myFractionMatrix[i, j].Denominator = MaxValue;
                }
            }

            //var matrixClass = new MyMatrix<MyFraction>(vector.Length);

            //for g

            //matrixClass.G();

            //for pg

            //for fg
        }
    }

    public class MyFraction
    {
        public BigInteger Numerator { get; set; }
        public BigInteger Denominator { get; set; }

    }
    public class Matrix<T>
    {
        public T[,] MatrixA { get; set; }
        public T[] VectorB { get; set; }
        public T[] ArrayResults { get; set; }

        public Matrix(T[,] matrixA, T[] vectorB)
        {
            MatrixA = matrixA;
            VectorB = vectorB;
        }


        //public T[] WithoutMainElement()
        //{
        //    int n = MatrixA.Length;

        //    T[][] mergedMatrix = MergeMatrixWithResultVector(MatrixA, VectorB);
        //    dynamic matrix = mergedMatrix;

        //    for (int i = 0; i < n; i++)
        //    {
        //        dynamic first = matrix[i][i];
        //        for (int j = 0; j < n + 1; j++)
        //            matrix[i][j] /= first;

        //        for (int j = i + 1; j < n; j++)
        //        {
        //            double factor = matrix[j][i] / matrix[i][i];
        //            for (int x = i; x < n + 1; x++)
        //                matrix[j][x] -= (matrix[i][x] * factor);
        //        }
        //    }

        //    return null;
        //}
    }
}
