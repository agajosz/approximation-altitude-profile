using System;
using System.Collections.Generic;
using System.Linq;
using MiscUtil;

namespace GaussLinearElimination
{
    public class MyMatrix<T>
    {
        public T[,] matrixA;
        public T[] vectorB;
        public T[] vectorX;
        public T[] vectorXgauss;
        public int dimensions;
        public int[] column;
        public MyMatrix(int dimensions)
        {
            this.dimensions = dimensions;

            vectorX = new T[dimensions];
            vectorXgauss = new T[dimensions];
            column = new int[dimensions];
            for (int i = 0; i < dimensions; i++)
                column[i] = i;
        }
        public MyMatrix(int dimensions, T[,] matrix, T[] vector)
        {
            this.dimensions = dimensions;
            matrixA = matrix;
            vectorB = vector;

            vectorX = new T[dimensions];
            vectorXgauss = new T[dimensions];
            column = new int[dimensions];
            for (int i = 0; i < dimensions; i++)
                column[i] = i;
        }

        public void Multiplication()
        {
            for (int y = 0; y < dimensions; y++)
                for (int x = 0; x < dimensions; x++)
                    vectorB[y] = Operator.Add(vectorB[y], Operator.Multiply(matrixA[x, y], vectorX[x]));
        }
        public void CalculateStep(int n)
        {
            for (int y = n; y < dimensions; y++)
            {
                T a = Operator.Divide(matrixA[column[n - 1], y], matrixA[column[n - 1], n - 1]);
                for (int x = n - 1; x < dimensions; x++)
                    matrixA[column[x], y] = Operator.Subtract(matrixA[column[x], y], Operator.Multiply(a, matrixA[column[x], n - 1]));
                vectorB[y] = Operator.Subtract(vectorB[y], Operator.Multiply(a, vectorB[n - 1]));
            }
        }

        public void CalculateG()
        {
            for (int n = 1; n < dimensions; n++)
                CalculateStep(n);
            CalculateResult();
        }

        public void CalculateGP()
        {
            for (int n = 1; n < dimensions; n++)
            {
                int number = n - 1;
                T max = matrixA[column[n - 1], n - 1];
                for (int i = n - 1; i < dimensions; i++)
                {
                    T actual = Absolute(matrixA[column[n - 1], i]);
                    if (Operator.GreaterThan<T>(actual, max))
                    {
                        max = actual;
                        number = i;
                    }
                }
                if (Operator.NotEqual(number, n - 1))
                {
                    for (int l = n - 1; l < dimensions; l++)
                    {
                        T tempA = matrixA[column[l], number];
                        matrixA[column[l], number] = matrixA[column[l], n - 1];
                        matrixA[column[l], n - 1] = tempA;
                    }
                    T tempB = vectorB[number];
                    vectorB[number] = vectorB[n - 1];
                    vectorB[n - 1] = tempB;
                }
                CalculateStep(n);
            }
            CalculateResult();
        }

        public void CalculateGF()
        {
            for (int n = 1; n < dimensions; n++)
            {

                int number = column[n - 1];
                int numberr = n - 1;
                T max = matrixA[column[n - 1], n - 1];
                for (int i = n - 1; i < dimensions; i++)
                {
                    for (int j = n - 1; j < dimensions; j++)
                    {
                        T actual = Absolute(matrixA[column[i], j]);
                        if (Operator.GreaterThan<T>(actual, max))
                        {
                            max = actual;
                            number = i;
                            numberr = j;
                        }
                    }
                }
                if (Operator.NotEqual(number, column[n] - 1) && Operator.NotEqual(numberr, n - 1))
                {
                    for (int l = n - 1; l < dimensions; l++)
                    {
                        T tempA = matrixA[column[l], numberr];
                        matrixA[column[l], numberr] = matrixA[column[l], n - 1];
                        matrixA[column[l], n - 1] = tempA;
                    }
                    T tempB = vectorB[numberr];
                    vectorB[numberr] = vectorB[n - 1];
                    vectorB[n - 1] = tempB;

                    int temp = column[number];
                    column[number] = column[n - 1];
                    column[n - 1] = temp;
                }
                CalculateStep(n);
            }
            CalculateResult();
        }

        public void CalculateResult()
        {
            for (int y = dimensions - 1; y >= 0; y--)
            {
                vectorXgauss[column[y]] = Operator.Divide(vectorB[y], matrixA[column[y], y]);
                for (int x = dimensions - 1; x > y; x--)
                {
                    matrixA[column[x], y] = Operator.Divide(matrixA[column[x], y], matrixA[column[y], y]);
                    vectorXgauss[column[y]] = Operator.Subtract(vectorXgauss[column[y]], Operator.Multiply(matrixA[column[x], y], vectorXgauss[column[x]]));
                }
            }
            for (int i = 0; i < dimensions; i++)
                vectorB[i] = vectorXgauss[i];
        }

        public T Absolute(T obj)
        {
            T zero = Operator.Subtract(obj, obj);
            if (Operator.LessThan(obj, zero))
                obj = Operator.Subtract(obj, Operator.Add(obj, obj));
            return obj;
        }

        public void CalculateJacobiSparse(T[,] sparseMatrix)
        {
            var newX = new T[dimensions];

            newX[0] = Operator.Multiply(sparseMatrix[0, 2], vectorXgauss[0]);
            newX[dimensions - 1] = Operator.Multiply(sparseMatrix[0, 0], vectorXgauss[dimensions - 1]);

            for (int i = 1; i < dimensions - 1; i++)
            {
                newX[i] = Operator.Multiply(sparseMatrix[i, 0], vectorXgauss[i - 1]);

                newX[i] = Operator.Add(newX[i], Operator.Multiply(sparseMatrix[i, 2], vectorXgauss[i + 1]));
            }

            for (int j = 0; j < vectorXgauss.Length; j++)
            {
                vectorXgauss[j] = Operator.Divide(Operator.Subtract(vectorB[j], newX[j]), sparseMatrix[j, 1]);
            }
        }

        public void CalculateJacobi()
        {
            var newX = new T[dimensions];

            for (int i = 0; i < dimensions; i++)
            {
                for (int j = 0; j < dimensions; j++)
                {
                    if (i != j)
                    {
                        newX[i] = Operator.Add(newX[i], Operator.Multiply(matrixA[i, j], vectorXgauss[j]));
                    }
                }
            }

            for (int j = 0; j < vectorXgauss.Length; j++)
            {
                vectorXgauss[j] = Operator.Divide(Operator.Subtract(vectorB[j], newX[j]), matrixA[j, j]);
            }
        }

        public void CalculateGaussSeidel()
        {
            for (int i = 0; i < dimensions; i++)
            {
                var d = vectorB[i];

                for (int j = 0; j < dimensions; j++)
                {
                    if (i != j)
                    {
                        d = Operator.Subtract(d, Operator.Multiply(matrixA[i, j], vectorXgauss[j]));
                    }
                }

                vectorXgauss[i] = Operator.Divide(d, matrixA[i, i]);
            }
        }

        public void CalculateWithAlglib()
        {
            var threes = Enumerable.Range(0, dimensions - 2).Select(x => 3).ToArray();

            var list = new List<int> { 2 };
            list.AddRange(threes);
            list.Add(2);

            var rowSizes = list.ToArray();

            alglib.sparsecreatecrs(dimensions, dimensions, rowSizes, out var matrix);

            for (var i = 0; i < dimensions; i++)
            {
                for (var j = 0; j < dimensions; j++)
                {
                    if (Convert.ToDouble(matrixA[i, j]) != default)
                    {
                        alglib.sparseset(matrix, i, j, Convert.ToDouble(matrixA[i, j]));
                    }
                }
            }

            alglib.sparsesolvesks(matrix, dimensions, true, vectorB as double[], out var rep, out var x);

            vectorXgauss = x as T[];
        }
    }

    public class SparseArrayModel
    {
        public SparseArrayModel(
            int row,
            int column,
            double value)
        {
            Row = row;
            Column = column;
            Value = value;
        }
        public int Row { get; set; }
        public int Column { get; set; }
        public double Value { get; set; }
    }
}

