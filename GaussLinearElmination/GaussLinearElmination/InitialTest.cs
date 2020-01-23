using System;

namespace GaussLinearElimination
{
    public class InitialTest
    {
        private MyMatrix<float> matrixfloat;
        private MyMatrix<double> matrixdouble;
        private MyMatrix<Fraction> matrixfraction;
        //private double difference;

        public InitialTest()
        {
            matrixfloat = new MyMatrix<float>(4);
            matrixdouble = new MyMatrix<double>(4);
            matrixfraction = new MyMatrix<Fraction>(4);
        }

        public void Run()
        {
            Console.Write("Test wstępny");
            FillMatrix();
            Multiplication();
            CalculateGP();
        }

        public void FillMatrix()
        {
            //float[] test = { 4, -2, 4, -2, -1, 3, 1, 4, 2, 2, 2, 4, 2, 1, 3, 2, -2, 4, 2, -2 };
            //int t = 0;
            //for (int y = 0; y < 4; y++)
            //{
            //    for (int x = 0; x < 4; x++)
            //    {
            //        matrixfloat.SetMatrixA(x, y, test[t]);
            //        matrixdouble.SetMatrixA(x, y, test[t]);
            //        matrixfraction.SetMatrixA(x, y, test[t]);
            //        t++;
            //    }
            //    matrixfloat.SetVectorX(y, test[t]);
            //    matrixdouble.SetVectorX(y, test[t]);
            //    matrixfraction.SetVectorX(y, test[t]);

            //    matrixfloat.SetVectorB(y, 0);
            //    matrixdouble.SetVectorB(y, 0);
            //    matrixfraction.SetVectorB(y, 0);
            //    t++;
            //}
        }
        public void Multiplication()
        {
            matrixfloat.Multiplication();
            matrixdouble.Multiplication();
            matrixfraction.Multiplication();
        }
        public void CalculateGP()
        {
            matrixfloat.CalculateGP();
            matrixdouble.CalculateGP();
            matrixfraction.CalculateGP();
        }
        public void CalculateGF()
        {
            matrixfloat.CalculateGF();
            matrixdouble.CalculateGF();
            matrixfraction.CalculateGF();
        }

    }
}
