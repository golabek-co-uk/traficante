using System;
using System.Collections.Generic;
using System.Text;
using Traficante.TSQL.Lib.Attributes;

namespace Traficante.TSQL.Lib
{
    public partial class Library
    {
        private readonly Random _rand = new Random();

        [BindableMethod]
        public decimal? Abs(decimal? value)
        {
            if (!value.HasValue) return null;
            return Math.Abs(value.Value);
        }

        [BindableMethod]
        public double? Abs(double? value)
        {
            if (!value.HasValue) return null;
            return Math.Abs(value.Value);
        }

        [BindableMethod]
        public short? Abs(short? value)
        {
            if (!value.HasValue) return null;
            return Math.Abs(value.Value);
        }


        [BindableMethod]
        public long? Abs(long? value)
        {
            if (!value.HasValue) return null;
            return Math.Abs(value.Value);
        }

        [BindableMethod]
        public int? Abs(int? value)
        {
            if (!value.HasValue) return null;
            return Math.Abs(value.Value);
        }

        [BindableMethod]
        public sbyte? Abs(sbyte? value)
        {
            if (!value.HasValue) return null;
            return Math.Abs(value.Value);
        }

        [BindableMethod]
        public float? Abs(float? value)
        {
            if (!value.HasValue) return null;
            return Math.Abs(value.Value);
        }

        [BindableMethod]
        public decimal? Ceil(decimal? value)
        {
            if (!value.HasValue) return null;
            return Math.Ceiling(value.Value);
        }

        [BindableMethod]
        public double? Ceil(double? value)
        {
            if (!value.HasValue) return null;
            return Math.Ceiling(value.Value);
        }

        [BindableMethod]
        public decimal? Floor(decimal? value)
        {
            if (!value.HasValue) return null;
            return Math.Floor(value.Value);
        }

        [BindableMethod]
        public double? Floor(double? value)
        {
            if (!value.HasValue) return null;
            return Math.Floor(value.Value);
        }

        [BindableMethod]
        public double? Acos(double? value)
        {
            if (!value.HasValue) return null;
            return Math.Acos(value.Value);
        }

        [BindableMethod]
        public double? Asin(double? value)
        {
            if (!value.HasValue) return null;
            return Math.Asin(value.Value);
        }

        [BindableMethod]
        public double? Atan(double? value)
        {
            if (!value.HasValue) return null;
            return Math.Atan(value.Value);
        }

        [BindableMethod]
        public double? Atan2(double? y, double? x)
        {
            if (!y.HasValue) return null;
            if (!x.HasValue) return null;
            return Math.Atan2(y.Value, x.Value);
        }

        [BindableMethod]
        public double? Cos(double? value)
        {
            if (!value.HasValue) return null;
            return Math.Cos(value.Value);
        }

        [BindableMethod]
        public double? Cosh(double? value)
        {
            if (!value.HasValue) return null;
            return Math.Cosh(value.Value);
        }

        [BindableMethod]
        public double? Tan(double? value)
        {
            if (!value.HasValue) return null;
            return Math.Tan(value.Value);
        }

        [BindableMethod]
        public double? Cot(double? value)
        {
            if (!value.HasValue) return null;
            return 1 / Math.Tan(value.Value);
        }

        [BindableMethod]
        public double? Degrees(double? value)
        {
            if (!value.HasValue) return null;
            return (180 / Math.PI) * value.Value;
        }

        [BindableMethod]
        public double? Radians(double? value)
        {
            if (!value.HasValue) return null;
            return (value.Value * Math.PI) / 1800;
        }

        [BindableMethod]
        public double? Exp(double? value)
        {
            if (!value.HasValue) return null;
            return Math.Exp(value.Value);
        }

        [BindableMethod]
        public double? Log(double? value)
        {
            if (!value.HasValue) return null;
            return Math.Log(value.Value);
        }

        public static double? Log(double? value, double? newBase)
        {
            if (!value.HasValue) return null;
            if (!value.HasValue) Math.Log(value.Value);
            return Math.Log(value.Value, newBase.Value);
        }

        [BindableMethod]
        public double? Log10(double? value)
        {
            if (!value.HasValue) return null;
            return Math.Log10(value.Value);
        }

        [BindableMethod]
        public double? Pi()
        {
            return Math.PI;
        }

        [BindableMethod]
        public double? Power(double? value, double? pow)
        {
            if (!value.HasValue) return null;
            if (!pow.HasValue) return null;
            return Math.Pow(value.Value, pow.Value);
        }


        [BindableMethod]
        public decimal? Sign(decimal? value)
        {
            if (!value.HasValue) return null;
            if (value.Value > 0) return 1;
            if (value.Value == 0)  return 0;
            return -1;
        }

        [BindableMethod]
        public double? Sign(double? value)
        {
            if (!value.HasValue) return null;
            if (value.Value > 0) return 1;
            if (value.Value == 0) return 0;
            return -1;
        }

        [BindableMethod]
        public long? Sign(long? value)
        {
            if (!value.HasValue) return null;
            if (value.Value > 0) return 1;
            if (value.Value == 0) return 0;
            return -1;
        }

        [BindableMethod]
        public int? Sign(int? value)
        {
            if (!value.HasValue) return null;
            if (value.Value > 0) return 1;
            if (value.Value == 0) return 0;
            return -1;
        }

        [BindableMethod]
        public short? Sign(short? value)
        {
            if (!value.HasValue) return null;
            if (value.Value > 0) return 1;
            if (value.Value == 0) return 0;
            return -1;
        }

        [BindableMethod]
        public sbyte? Sign(sbyte? value)
        {
            if (!value.HasValue) return null;
            if (value.Value > 0) return 1;
            if (value.Value == 0) return 0;
            return -1;
        }

        [BindableMethod]
        public float? Sign(float? value)
        {
            if (!value.HasValue) return null;
            if (value.Value > 0) return 1;
            if (value.Value == 0) return 0;
            return -1;
        }


        [BindableMethod]
        public double? Sin(double? value)
        {
            if (!value.HasValue) return null;
            return Math.Sin(value.Value);
        }

        [BindableMethod]
        public double? Sqrt(double? value)
        {
            if (!value.HasValue) return null;
            return Math.Sqrt(value.Value);
        }

        [BindableMethod]
        public double? Square(double? value)
        {
            if (!value.HasValue) return null;
            return value * value;
        }

        [BindableMethod]
        public decimal? Square(decimal? value)
        {
            if (!value.HasValue) return null;
            return value * value;
        }

        [BindableMethod]
        public int? Square(int? value)
        {
            if (!value.HasValue) return null;
            return value * value;
        }

        [BindableMethod]
        public long? Square(long? value)
        {
            if (!value.HasValue) return null;
            return value * value;
        }

        [BindableMethod]
        public float? Square(float? value)
        {
            if (!value.HasValue) return null;
            return value * value;
        }

        [BindableMethod]
        public decimal? Round(decimal? value, int precision)
        {
            if (!value.HasValue) return null;
            return Math.Round(value.Value, precision);
        }

        [BindableMethod]
        public double? Round(double? value, int precision)
        {
            if (!value.HasValue) return null;
            return Math.Round(value.Value, precision);
        }

        [BindableMethod]
        public decimal? PercentOf(decimal? value, decimal? max)
        {
            if (!value.HasValue) return null;
            if (!max.HasValue) return null;
            return value * 100 / max;
        }

        [BindableMethod]
        public double? PercentOf(double? value, double? max)
        {
            if (!value.HasValue) return null;
            if (!max.HasValue) return null;
            return value * 100 / max;
        }

        [BindableMethod]
        public int Rand()
            => _rand.Next();

        [BindableMethod]
        public int Rand(int min, int max)
            => _rand.Next(min, max);
    }
}
