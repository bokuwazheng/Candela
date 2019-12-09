using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Candela
{
    public class Matrix
    {
        private double[,] _data;
        private int m_rowSize;
        private int m_colSize;

        public Matrix(int rowSize, int colSize)
        {
            m_rowSize = rowSize;
            m_colSize = colSize;
            _data = new double[rowSize, colSize];
        }

        public Matrix(double[,] data, int rowSize = 4, int colSize = 4)
        {
            m_rowSize = rowSize;
            m_colSize = colSize;
            _data = data;
        }

        public Matrix(double[] rgb, double alpha)
        {
            m_rowSize = 4;
            m_colSize = 4;
            _data = new double[,]
            {
                { rgb[0], .0, .0, .0 },
                { .0, rgb[1], .0, .0 },
                { .0, .0, rgb[2], .0 },
                { .0, .0, .0, alpha }
            };
        }

        public Matrix()
        {
            m_rowSize = 4;
            m_colSize = 4;
            _data = new double[,]
            {
                { 1.0, .0, .0, .0 },
                { .0, 1.0, .0, .0 },
                { .0, .0, 1.0, .0 },
                { .0, .0, .0, 1.0 }
            };
        }

        public double this[int row, int col]
        {
            get
            {
                return _data[row, col];
            }
            set
            {
                _data[row, col] = value;
            }
        }

        public void Execute(Action<int, int> func)
        {
            for (var i = 0; i < this.m_rowSize; i++)
            {
                for (var j = 0; j < this.m_colSize; j++)
                {
                    func(i, j);
                }
            }
        }

        // Scalar multiplication
        public static Matrix operator *(Matrix matrix, double scalar)
        {
            var result = new Matrix(matrix.m_rowSize, matrix.m_colSize);
            result.Execute((i, j) => result[i, j] = matrix[i, j] * scalar);
            return result;
        }

        // Scalar addition
        public static Matrix operator +(Matrix matrix, double scalar)
        {
            var result = new Matrix(matrix.m_rowSize, matrix.m_colSize);
            result.Execute((i, j) => result[i, j] = matrix[i, j] + scalar);
            return result;
        }

        // Scalar subtraction
        public static Matrix operator -(Matrix matrix, double scalar)
        {
            var result = new Matrix(matrix.m_rowSize, matrix.m_colSize);
            result.Execute((i, j) => result[i, j] = matrix[i, j] - scalar);
            return result;
        }

        // Scalar division
        public static Matrix operator /(Matrix matrix, double scalar)
        {
            var result = new Matrix(matrix.m_rowSize, matrix.m_colSize);
            result.Execute((i, j) => result[i, j] = matrix[i, j] / scalar);
            return result;
        }


        // Vector multiplication
        public static double[] operator *(double[] vector, Matrix matrix)
        {
            if (matrix.m_colSize != vector.Length)
            {
                throw new ArgumentException("Matrix size doesn't match array size.");
            }
            return new double[]
            {
                vector[0] * matrix[0, 0] +
                vector[1] * matrix[1, 0] +
                vector[2] * matrix[2, 0] +
                vector[3] * matrix[3, 0],

                vector[0] * matrix[0, 1] +
                vector[1] * matrix[1, 1] +
                vector[2] * matrix[2, 1] +
                vector[3] * matrix[3, 1],

                vector[0] * matrix[0, 2] +
                vector[1] * matrix[1, 2] +
                vector[2] * matrix[2, 2] +
                vector[3] * matrix[3, 2],

                vector[0] * matrix[0, 3] +
                vector[1] * matrix[1, 3] +
                vector[2] * matrix[2, 3] +
                vector[3] * matrix[3, 3],
            };
        }

        // Multiplication of two matrices
        public static Matrix operator *(Matrix matrix1, Matrix matrix2)
        {
            if (matrix1.m_colSize != matrix2.m_rowSize)
            {
                throw new ArgumentException("Matrix sizes don't match.");
            }
            var result = new Matrix(matrix1.m_rowSize, matrix2.m_colSize);
            result.Execute((i, j) =>
            {
                for (var k = 0; k < matrix1.m_colSize; k++)
                {
                    result[i, j] += matrix1[i, k] * matrix2[k, j];
                }
            });
            return result;
        }

        // Vector addition
        public static double[] operator +(double[] vector, Matrix matrix)
        {
            if (matrix.m_colSize != vector.Length)
            {
                throw new ArgumentException("Matrix size doesn't match array size.");
            }
            var result = new double[vector.Length];
            for (var i = 0; i < vector.Length; i++)
            {
                result[i] += vector[i] + matrix[i, i];
            }

            return result;
        }

        // Addition of two matrices
        public static Matrix operator +(Matrix matrix1, Matrix matrix2)
        {
            if (matrix1.m_rowSize != matrix2.m_rowSize || matrix1.m_colSize != matrix2.m_colSize)
            {
                throw new ArgumentException("Matrix sizes don't match.");
            }
            var result = new Matrix(matrix1.m_rowSize, matrix1.m_colSize);
            result.Execute((i, j) =>
                result[i, j] = matrix1[i, j] + matrix2[i, j]);
            return result;
        }

        // Subtraction of two matrices
        public static Matrix operator -(Matrix matrix1, Matrix matrix2)
        {
            return matrix1 + (matrix2 * -1);
        }

        public Matrix Transpose()
        {
            var result = new Matrix(this.m_colSize, this.m_rowSize);
            result.Execute((i, j) => result[i, j] = this[j, i]);
            return result;
        }

        public Matrix Rotate(double sin, double cos, RotationOption ro)
        {
            if (ro == RotationOption.X)
            {
                return this * new Matrix(new double[,]
                {
                    { 1, 0, 0, 0 },
                    { 0, cos, -sin, 0 },
                    { 0, sin, cos, 0 },
                    { 0, 0, 0, 1 },
                });
            }
            if (ro == RotationOption.Y)
            {
                return this * new Matrix(new double[,]
                {
                    { cos, 0, sin, 0 },
                    { 0, 1, 0, 0 },
                    { -sin, 0, cos, 0 },
                    { 0, 0, 0, 1 },
                });
            }
            if (ro == RotationOption.Z)
            {
                return this * new Matrix(new double[,]
                {
                    { cos, -sin, 0, 0 },
                    { sin, cos, 0, 0 },
                    { 0, 0, 1, 0 },
                    { 0, 0, 0, 1 },
                });
            }
            return null;
        }

        public enum RotationOption
        {
            X, Y, Z
        }

        public override string ToString()
        {
            var result = "";

            for (int i = 0; i < this.m_rowSize; i++)
            {
                for (int j = 0; j < this.m_colSize; j++)
                {
                    result += this[i, j] + " ";
                }

                result += "\n";
            }

            return result;
        }
    }
}
