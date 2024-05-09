using ArcGIS.Desktop.Mapping;
using ArcGIS.Core.Geometry;
using Rose_AGP.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;

namespace Rose_AGP
{
    public class RasterFields : RoseFields
    { }

    public class MovingStatsFields : RoseFields
    { }

    public class FlapParameters
    {
        public string RoseType { get; set; }
        public int SubCellsize { get; set; }
        public int NoOfColumns { get; set; }
        public int NoOfRows { get; set; }
        public int Interval { get; set; }
        public bool SelectedFeatures { get; set; }
        public int RangeFrom { get; set; }
        public int RangeTo { get; set; }
        public string FieldNameForAzi { get; set; }
        public int XBlocks { get; set; }
        public int YBlocks { get; set; }
        public int TotalBlocks { get; set; }
        public double SearchWindow { get; set; }

        public List<FlapParameter> flapParameters { get; set; }

        public FlapParameters()
        {

        }

        public int CalculateTotalBlocks(int _XBlocks, int _YBlocks)
        {
            XBlocks = _XBlocks;
            YBlocks = _YBlocks;

            if (_XBlocks > 0 && _YBlocks > 0)
                TotalBlocks = ((_XBlocks * 2) + 1) * ((_YBlocks * 2) + 1);
            else if (_XBlocks == 0)
                TotalBlocks = (_YBlocks * 2) + 1;
            else
                TotalBlocks = (_XBlocks * 2) + 1;

            return TotalBlocks;
        }

        public void SetProperties(Envelope myEnvelope)
        {
            double dblColumns = myEnvelope.Width / SubCellsize;
            double dblRows = myEnvelope.Height / SubCellsize;

            NoOfColumns = Convert.ToInt16(Math.Floor(dblColumns) + 1);
            NoOfRows = Convert.ToInt16(Math.Floor(dblRows) + 1);
        }
    }

    public class FlapParameter
    {
        public int CellID { get; set; }
        public List<double> ArrayValues { get; set; }
        public int Count { get; set; }
        public double SearchCount { get; set; }

        public List<FreqLen> LenAzi { get; set; } //raw input from lines
        public List<BinRangeClass> BinRange { get; set; }
        public double GridValue { get; set; }
        public double MeanValue { get; set; }
        public double MinValue { get; set; } //Min
        public double MaxValue { get; set; }
        public double SumValue { get; set; }
        public double StdValue { get; set; }
        public double AdjustedValue { get; set; } //adjusted gridvalue
        public double ExtentHeight { get; set; }
        public double ExtentWidth { get; set; }
        public double ExtentArea { get; set; }
        public double CentreX { get; set; }
        public double CentreY { get; set; }
        public double XMin { get; set; }
        public double XMax { get; set; }
        public double YMin { get; set; }
        public double YMax { get; set; }
        public bool CreateCell { get; set; }
        public FishnetStatistics FishStats { get; set; }
        public RoseDiagramParameters Rose { get; set; }

        public FlapParameter()
        {
            CellID = 0;
            ArrayValues = new List<double>();
            Count = 0;
            LenAzi = new List<FreqLen>();
            BinRange = new List<BinRangeClass>();

            GridValue = 0.0;
            MinValue = 0.0;
            MaxValue = 0.0;
            SumValue = 0.0;
            StdValue = 0.0;

            this.CreateCell = true;
        }

        public void SetProperties(FeatureLayer InputLayer, Envelope myEnv)
        {

            if (myEnv == null)
                return; //create custom exception

            this.ExtentHeight = myEnv.Height;
            this.ExtentWidth = myEnv.Width;
            this.ExtentArea = myEnv.Area;
            this.CentreX = myEnv.XMin + ((myEnv.XMax - myEnv.XMin) / 2);
            this.CentreY = myEnv.YMin + ((myEnv.YMax - myEnv.YMin) / 2);
            this.XMin = myEnv.XMin;
            this.YMin = myEnv.YMin;
            this.XMax = myEnv.XMax;
            this.YMax = myEnv.YMax;
        }

        public void CalcArrayValues(bool bDirection, int NoOfElements, RoseType _roseType, int binSize)
        {
            int counter = NoOfElements;

            //counter = counter * 2;

            for (int i = 0; i <= counter - 1; i++)
            {

                this.ArrayValues.Add(0);
            }

            double dblElement = 0.0;

            for (int i = 0; i < this.LenAzi.Count; i++)
            {
                int nElement = 0;

                dblElement = LenAzi[i].Azimuth / Convert.ToDouble(binSize);

                nElement = (int)Math.Floor(dblElement);

                if (_roseType == RoseType.Length)
                {
                    ArrayValues[nElement] = ArrayValues[nElement] + LenAzi[i].Length;
                }
                else
                {
                    ArrayValues[nElement] = ArrayValues[nElement] + 1;
                }

            }

            /*
            //Run second iteration for symmetry if not point
            if (!bDirection)
            {
                int nIncrement = NoOfElements;

                for (int i = 0; i <= NoOfElements - 1; i++)
                {
                    FreqArray[nIncrement] = FreqArray[i];

                    nIncrement++; ;
                }

            }
            */

        }

    }

    public class BinRangeClass
    {
        public int LowerRange { get; set; }
        public int UpperRange { get; set; }

        public BinRangeClass()
        {
            LowerRange = 0;
            UpperRange = 0;
        }
    }

    public class FreqLen
    {
        public double Azimuth { get; set; }
        public double Length { get; set; }
        int Element { get; set; }
    }

    public class RoseDiagramParameters
    {
        #region Properties
        public int CellID { get; set; }
        public List<double> RoseArray { get; set; }
        public double[,] RoseArrayBin { get; set; }
        public List<double> MinAzi { get; set; }
        public List<double> MaxAzi { get; set; }
        public List<double> AvgAzi { get; set; }
        public List<double> StdAzi { get; set; }
        public List<int> BinCount { get; set; }
        public List<double> MinLength { get; set; }
        public List<double> MaxLength { get; set; }
        public List<double> AvgLength { get; set; }
        public List<double> StdLength { get; set; }
        public List<double> RoseAzimuth { get; set; } //raw input from lines
        public List<double> RoseLength { get; set; }  //raw input from lines
        public List<BinRangeClass> BinRange { get; set; }
        #endregion

        public RoseDiagramParameters()
        {
            CellID = 0;
            MinAzi = new List<double>();
            MaxAzi = new List<double>(); ;
            AvgAzi = new List<double>();
            BinCount = new List<int>();
            MinLength = new List<double>();
            MaxLength = new List<double>(); ;
            AvgLength = new List<double>();
            RoseAzimuth = new List<double>(); //captures all values from input for statistics
            RoseLength = new List<double>(); //captures all values from input for statistics
            BinCount = new List<int>();
            BinRange = new List<BinRangeClass>();
            StdAzi = new List<double>();
            StdLength = new List<double>();
            RoseArray = new List<double>();

        }

        public bool CalculateRoseStatistics(RoseDiagramParameters nRose, int binSize)
        {
            int nElements = 180 / binSize;

            List<FreqLen> freqLength = new List<FreqLen>();

            for (int i = 0; i < nRose.RoseAzimuth.Count; i++)
            {
                freqLength.Add(new FreqLen { Azimuth = nRose.RoseAzimuth[i], Length = nRose.RoseLength[i] });
                freqLength.Add(new FreqLen { Azimuth = nRose.RoseAzimuth[i] + 180, Length = nRose.RoseLength[i] });
            }

            int lowerRange = 0;
            int upperRange = binSize;

            int count = nElements * 2;

            for (int p = 0; p < count; p++)
            {
                var query = freqLength.Where(i => i.Azimuth > lowerRange).Where(b => b.Azimuth <= upperRange);

                nRose.BinRange.Add(new BinRangeClass { LowerRange = lowerRange, UpperRange = upperRange });

                nRose.MaxAzi.Add(freqLength.Where(i => i.Azimuth > lowerRange).Where(b => b.Azimuth <= upperRange).Max(a => a.Azimuth));
                nRose.MinAzi.Add(freqLength.Where(i => i.Azimuth > lowerRange).Where(b => b.Azimuth <= upperRange).Min(a => a.Azimuth));
                double AziAvg = freqLength.Where(i => i.Azimuth > lowerRange).Where(b => b.Azimuth <= upperRange).Average(a => a.Azimuth);
                nRose.AvgAzi.Add(AziAvg);

                int Count = freqLength.Where(i => i.Azimuth > lowerRange).Where(b => b.Azimuth <= upperRange).Count();
                nRose.BinCount.Add(Count);

                nRose.MaxLength.Add(freqLength.Where(i => i.Azimuth > lowerRange).Where(b => b.Azimuth <= upperRange).Max(a => a.Length));
                nRose.MinLength.Add(freqLength.Where(i => i.Azimuth > lowerRange).Where(b => b.Azimuth <= upperRange).Min(a => a.Length));

                double LenAvg = freqLength.Where(i => i.Azimuth > lowerRange).Where(b => b.Azimuth <= upperRange).Average(a => a.Length);
                nRose.AvgLength.Add(LenAvg);

                double sumOfDerivation = 0;
                foreach (FreqLen _freq in query)
                {
                    sumOfDerivation += (_freq.Azimuth) * (_freq.Azimuth);
                }
                double sumOfDerivationAverage = sumOfDerivation / Count;
                nRose.StdAzi.Add(Math.Sqrt(sumOfDerivationAverage - (AziAvg * AziAvg)));

                sumOfDerivation = 0;
                foreach (FreqLen _freq in query)
                {
                    sumOfDerivation += (_freq.Length) * (_freq.Length);
                }
                sumOfDerivationAverage = sumOfDerivation / Count;
                nRose.StdLength.Add(Math.Sqrt(sumOfDerivationAverage - (LenAvg * LenAvg)));

                lowerRange = upperRange;
                upperRange = upperRange + binSize;

            }

            return true;

        }

        public bool CalculateRoseStatistics(List<FreqLen> freqLength, int binSize)
        {
            int nElements = 180 / binSize;
            int lowerRange = 0;
            int upperRange = binSize;

            int count = nElements * 2;

            //Create temp list
            List<FreqLen> tempList = new List<FreqLen>();
            foreach (FreqLen _value in freqLength)
            {
                tempList.Add(new FreqLen { Azimuth = _value.Azimuth + 180, Length = _value.Length });
            }

            freqLength = freqLength.Concat(tempList).ToList();


            for (int p = 0; p < count; p++)
            {
                int Count = 0;
                var query = freqLength.Where(i => i.Azimuth > lowerRange).Where(b => b.Azimuth <= upperRange);

                if (query.Count() == 0)
                {
                    this.BinRange.Add(new BinRangeClass { LowerRange = lowerRange, UpperRange = upperRange });

                    Count = 0;
                    MaxAzi.Add(-1);
                    MinAzi.Add(-1);
                    AvgAzi.Add(-1);
                    BinCount.Add(-1);
                    MaxLength.Add(-1);
                    MinLength.Add(-1);
                    AvgLength.Add(-1);
                    StdAzi.Add(-1);
                    StdLength.Add(-1);
                }
                else
                {
                    this.BinRange.Add(new BinRangeClass { LowerRange = lowerRange, UpperRange = upperRange });
                    this.MaxAzi.Add(freqLength.Where(i => i.Azimuth > lowerRange).Where(b => b.Azimuth <= upperRange).Max(a => a.Azimuth));
                    this.MinAzi.Add(freqLength.Where(i => i.Azimuth > lowerRange).Where(b => b.Azimuth <= upperRange).Min(a => a.Azimuth));
                    double AziAvg = freqLength.Where(i => i.Azimuth > lowerRange).Where(b => b.Azimuth <= upperRange).Average(a => a.Azimuth);
                    this.AvgAzi.Add(AziAvg);

                    Count = freqLength.Where(i => i.Azimuth > lowerRange).Where(b => b.Azimuth <= upperRange).Count();
                    this.BinCount.Add(Count);

                    this.MaxLength.Add(freqLength.Where(i => i.Azimuth > lowerRange).Where(b => b.Azimuth <= upperRange).Max(a => a.Length));
                    this.MinLength.Add(freqLength.Where(i => i.Azimuth > lowerRange).Where(b => b.Azimuth <= upperRange).Min(a => a.Length));

                    double LenAvg = freqLength.Where(i => i.Azimuth > lowerRange).Where(b => b.Azimuth <= upperRange).Average(a => a.Length);
                    this.AvgLength.Add(LenAvg);

                    double sumOfDerivation = 0;
                    foreach (FreqLen _freq in query)
                    {

                        sumOfDerivation += (_freq.Azimuth) * (_freq.Azimuth);

                    }
                    double sumOfDerivationAverage = sumOfDerivation / Count;
                    this.StdAzi.Add(Math.Sqrt(sumOfDerivationAverage - (AziAvg * AziAvg)));

                    sumOfDerivation = 0;
                    foreach (FreqLen _freq in query)
                    {

                        sumOfDerivation += (_freq.Length) * (_freq.Length);

                    }
                    sumOfDerivationAverage = sumOfDerivation / Count;
                    this.StdLength.Add(Math.Sqrt(sumOfDerivationAverage - (LenAvg * LenAvg)));

                }

                lowerRange = upperRange;
                upperRange = upperRange + binSize;
            }

            return true;
        }

        public void RoseArrayValues(int NoOfElements, Enum.RoseType _roseType, int binSize)
        {
            int counter = NoOfElements;

            counter = counter * 2;

            for (int i = 0; i <= counter - 1; i++)
            {
                this.RoseArray.Add(0);
            }

            double dblElement = 0.0;

            for (int i = 0; i < this.RoseAzimuth.Count; i++)
            {
                int nElement = 0;

                dblElement = RoseAzimuth[i] / Convert.ToDouble(binSize);

                nElement = (int)Math.Floor(dblElement);

                if (_roseType == RoseType.Length)
                {
                    RoseArray[nElement] = RoseArray[nElement] + RoseLength[i];
                }
                else
                {
                    RoseArray[nElement] = RoseArray[nElement] + 1;
                }

            }

            int nIncrement = NoOfElements;

            for (int i = 0; i <= NoOfElements - 1; i++)
            {
                RoseArray[nIncrement] = RoseArray[i];

                nIncrement++; ;
            }

        }

        public void RoseArrayValues(int NoOfElements, int binSize, List<FreqLen> freqLen, RoseType roseType)
        {
            int counter = NoOfElements;

            counter = counter * 2;

            for (int i = 0; i <= counter - 1; i++)
            {
                this.RoseArray.Add(0);
            }

            double dblElement = 0.0;

            for (int i = 0; i < freqLen.Count; i++)
            {
                int nElement = 0;

                dblElement = freqLen[i].Azimuth / Convert.ToDouble(binSize);

                nElement = (int)Math.Floor(dblElement);

                if (roseType == RoseType.Length)
                {
                    RoseArray[nElement] = RoseArray[nElement] + freqLen[i].Length;
                }
                else
                {
                    RoseArray[nElement] = RoseArray[nElement] + 1;
                }
            }

            int nIncrement = NoOfElements;

            for (int i = 0; i <= NoOfElements - 1; i++)
            {
                RoseArray[nIncrement] = RoseArray[i];

                nIncrement++; ;
            }
        }

        public void RoseArrayValues(bool bDirection, int NoOfElements, int binSize, List<FreqLen> freqLen, RoseType roseType)
        {
            int counter = NoOfElements;

            counter = counter * 2;

            for (int i = 0; i <= counter - 1; i++)
            {
                this.RoseArray.Add(0);
            }

            double dblElement = 0.0;

            for (int i = 0; i < freqLen.Count; i++)
            {
                int nElement = 0;

                dblElement = freqLen[i].Azimuth / Convert.ToDouble(binSize);

                nElement = (int)Math.Floor(dblElement);

                if (roseType == RoseType.Length)
                {
                    RoseArray[nElement] = RoseArray[nElement] + freqLen[i].Length;
                }
                else
                {
                    RoseArray[nElement] = RoseArray[nElement] + 1;
                }

            }

            if (!bDirection)
            {
                int nIncrement = NoOfElements;

                for (int i = 0; i <= NoOfElements - 1; i++)
                {
                    RoseArray[nIncrement] = RoseArray[i];

                    nIncrement++; ;
                }
            }

        }

        public void CalculateRosePetals(int nInterval, double ExtentWidth, double ExtentHeight)
        {
            int j2 = RoseArray.Count;

            //NEED Extent from envelope or local if not regional rose

            //scale roseArray
            double dblScale = ScalePlots(RoseArray, ExtentWidth, ExtentHeight);
            double ScaleFactor = dblScale;

            for (int i = 0; i < RoseArray.Count; i++)
            {
                RoseArray[i] = (RoseArray[i] * dblScale) * 0.8;
            }

            int start = 180 / nInterval;
            RoseArrayBin = new double[j2, 4];

            for (int j = 0; j != j2; j++)
            {
                //Create rose diagram bins
                int alphaDeg = j * nInterval;
                int betaDeg = (j + 1) * nInterval;

                double dblAlphaRad = (double)alphaDeg * (Math.PI / 180);  //Convert to radians
                double dblBetaRad = (double)betaDeg * (Math.PI / 180);

                RoseArrayBin[j, 0] = Math.Sin(dblAlphaRad) * RoseArray[j];  //x2
                RoseArrayBin[j, 1] = Math.Cos(dblAlphaRad) * RoseArray[j];  //y2
                RoseArrayBin[j, 2] = Math.Sin(dblBetaRad) * RoseArray[j];  //x3
                RoseArrayBin[j, 3] = Math.Cos(dblBetaRad) * RoseArray[j];  //y3
            }

        }

        private double ScalePlots(List<double> RoseArray, double ExtentWidth, double ExtentHeight)
        {
            double dblLength = RoseArray.Max(); //greatest length

            double dblWidth = ExtentWidth / 2;
            double dblHeight = ExtentHeight / 2;

            double cellSize = dblWidth;

            if (cellSize < dblHeight)
                cellSize = dblHeight;

            return cellSize / dblLength;

        }

        public string IntervalErrorChecking(string strInterval)
        {

            if (strInterval == "")
            {
                return "Please enter an integer value";
            }

            if (!Information.IsNumeric(strInterval))
            {
                return "The value must be an integer";
            }

            int interval = Convert.ToInt32(strInterval);

            if (interval <= 4)
            {
                return "The value must be greater than 4";
            }

            if (180 % interval > 0)
            {
                return "The value must be divisible into 180";
            }

            return "";
        }

        public string SubcellErrorChecking(string strInterval)
        {
            if (!Information.IsNumeric(strInterval))
            {
                return "The value must be an integer";
            }

            return "";
        }

        public bool CalculateRoseStatistics(string RoseGeom, bool bDirection, int binSize)
        {
            int nElements = 180 / binSize;

            List<FreqLen> freqLength = new List<FreqLen>();

            if (RoseGeom == "Line")
            {
                for (int i = 0; i < RoseAzimuth.Count; i++)
                {
                    freqLength.Add(new FreqLen { Azimuth = RoseAzimuth[i], Length = RoseLength[i] });
                    freqLength.Add(new FreqLen { Azimuth = RoseAzimuth[i] + 180, Length = RoseLength[i] + 180 });
                }
            }
            else
            {
                for (int i = 0; i < RoseAzimuth.Count; i++)
                {
                    freqLength.Add(new FreqLen { Azimuth = RoseAzimuth[i], Length = 0 });
                    //  freqLength.Add(new FreqLen { Azimuth = nRose.RoseAzimuth[i] + 180, Length = nRose.RoseLength[i] + 180 });
                }
            }

            int lowerRange = 0;
            int upperRange = binSize;

            int count = 0;

            if (!bDirection)
                count = nElements * 2;
            else
                count = nElements;

            for (int p = 0; p < count; p++)
            {
                var query = freqLength.Where(i => i.Azimuth > lowerRange).Where(b => b.Azimuth <= upperRange);

                int Count = 0;
                BinRange.Add(new BinRangeClass { LowerRange = lowerRange, UpperRange = upperRange });

                if (query.Count() == 0)
                {
                    Count = 0;
                    MaxAzi.Add(-1);
                    MinAzi.Add(-1);
                    AvgAzi.Add(-1);
                    BinCount.Add(-1);
                    MaxLength.Add(-1);
                    MinLength.Add(-1);
                    AvgLength.Add(-1);
                    StdAzi.Add(-1);
                    StdLength.Add(-1);
                }
                else
                {
                    MaxAzi.Add(freqLength.Where(i => i.Azimuth > lowerRange).Where(b => b.Azimuth <= upperRange).Max(a => a.Azimuth));
                    MinAzi.Add(freqLength.Where(i => i.Azimuth > lowerRange).Where(b => b.Azimuth <= upperRange).Min(a => a.Azimuth));
                    double AziAvg = freqLength.Where(i => i.Azimuth > lowerRange).Where(b => b.Azimuth <= upperRange).Average(a => a.Azimuth);
                    AvgAzi.Add(AziAvg);

                    Count = freqLength.Where(i => i.Azimuth > lowerRange).Where(b => b.Azimuth <= upperRange).Count();
                    BinCount.Add(Count);

                    MaxLength.Add(freqLength.Where(i => i.Azimuth > lowerRange).Where(b => b.Azimuth <= upperRange).Max(a => a.Length));
                    MinLength.Add(freqLength.Where(i => i.Azimuth > lowerRange).Where(b => b.Azimuth <= upperRange).Min(a => a.Length));

                    double LenAvg = freqLength.Where(i => i.Azimuth > lowerRange).Where(b => b.Azimuth <= upperRange).Average(a => a.Length);
                    AvgLength.Add(LenAvg);

                    double sumOfDerivation = 0;
                    foreach (FreqLen _freq in query)
                    {
                        sumOfDerivation += (_freq.Azimuth) * (_freq.Azimuth);
                    }
                    double sumOfDerivationAverage = sumOfDerivation / Count;
                    StdAzi.Add(Math.Sqrt(sumOfDerivationAverage - (AziAvg * AziAvg)));

                    if (RoseGeom == "Line")
                    {
                        sumOfDerivation = 0;
                        foreach (FreqLen _freq in query)
                        {
                            sumOfDerivation += (_freq.Length) * (_freq.Length);
                        }
                        sumOfDerivationAverage = sumOfDerivation / Count;
                        StdLength.Add(Math.Sqrt(sumOfDerivationAverage - (LenAvg * LenAvg)));
                    }
                }
                lowerRange = upperRange;
                upperRange = upperRange + binSize;
            }

            return true;

        }

    }

    public class FishnetStatistics
    {
        public double AziMin { get; set; }
        public double AziMax { get; set; }
        public double AziAvg { get; set; }
        public double AziStd { get; set; }
        public double Count { get; set; }

        public double LenMin { get; set; }
        public double LenMax { get; set; }
        public double LenAvg { get; set; }
        public double LenStd { get; set; }

        public double TotalLength { get; set; }

        public List<double> RoseAzimuth { get; set; }
        public List<double> RoseLength { get; set; }

        public FishnetStatistics()
        {
            RoseAzimuth = new List<double>();
            RoseLength = new List<double>();
        }

        public bool CalculateFishnetStatistics(List<FreqLen> freqLength)
        {

            Count = freqLength.Count;
            AziAvg = freqLength.Select(a => a.Azimuth).Average();
            AziMin = freqLength.Select(a => a.Azimuth).Min();
            AziMax = freqLength.Select(a => a.Azimuth).Max();

            var queryAzi = freqLength.Select(a => a.Azimuth);

            double sumOfDerivation = 0;
            foreach (double azi in queryAzi)
            {
                sumOfDerivation += (azi) * (azi);
            }

            double sumOfDerivationAverage = sumOfDerivation / Count;
            AziStd = Math.Sqrt(sumOfDerivationAverage - (AziAvg * AziAvg));

            LenMin = freqLength.Select(a => a.Length).Min();
            LenMax = freqLength.Select(a => a.Length).Max();
            LenAvg = freqLength.Select(a => a.Length).Average();
            TotalLength = freqLength.Select(a => a.Length).Sum();

            var queryLen = freqLength.Select(a => a.Length);

            sumOfDerivation = 0;
            foreach (double len in queryLen)
            {
                sumOfDerivation += (len) * (len);
            }
            sumOfDerivationAverage = sumOfDerivation / Count;
            LenStd = Math.Sqrt(sumOfDerivationAverage - (LenAvg * LenAvg));

            return true;

        }
    }

    public class RoseFields
    {
        // public int fieldID { get; set; }
        public string FieldName { get; set; }
        public string FieldAlias { get; set; }
        public string FieldType { get; set; }
        public int FieldLength { get; set; }
        public int FieldPrecision { get; set; }
    }
}
