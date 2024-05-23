using ArcGIS.Core.Data.Raster;
using ArcGIS.Core.Data;
//using ArcGIS.Core.Internal.CIM;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Core.Geometry;
using Rose_AGP.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rose_AGP
{
    public class RasterFactoryPreview
    {
        public TypeOfRasterAnalysis _rasterFactoryType;

        public RasterFactoryPreview(RasterLineamentAnalysis value)
        {

            SetRasterAnalysisDataset(value);
        }

        public void SetRasterAnalysisDataset(RasterLineamentAnalysis value)
        {
            _rasterFactoryType = TypeOfRasterAnalysis.RasterStatistics(value);
        }

        public async Task<FlapParameters> PrepareInputForProcessing(FeatureLayer InputLayer, Envelope customEnvelope, int subCellsize,
    int nInterval, bool selectedFeatures, int rangeFrom, int rangeTo, RoseGeom roseGeom, bool bRegional, string fieldName)
        {
            return await _rasterFactoryType.PrepareInputForProcessing(InputLayer, customEnvelope, subCellsize, nInterval,
                selectedFeatures, rangeFrom, rangeTo, roseGeom, bRegional, fieldName);
        }

        public async Task<FeatureClass> CreatePolyFeatClass(string suffix, string rasterName, bool bExists, string databasePath,
            FeatureLayer InputLayer, SpatialReference thisSpatRef, string FClassType, bool bMovStats)
        {
            return await _rasterFactoryType.CreatePolyFeatClass(suffix, rasterName, bExists, databasePath, InputLayer, thisSpatRef,
                FClassType, bMovStats);
        }

        public async Task<string> SaveAsVectorFeatures(FeatureClass OuputFeatureClass, SpatialReference mySpatialRef,
            FlapParameters _parameters, string FCType)
        {
            return await _rasterFactoryType.SaveAsVectorFeatures(OuputFeatureClass, mySpatialRef, _parameters, FCType);
        }

        public void WriteRasterValuesToGrid(RasterDataset rasterDS, FlapParameters _parameters, bool bInt)
        {
            _rasterFactoryType.WriteRasterValuesToGrid(rasterDS, _parameters, bInt);
        }

        public void CalculateGridValues(FlapParameters _parameters)
        {
            _rasterFactoryType.CalculateGridValues(_parameters);
        }

        public async Task<bool> CreateRasterDataset(string pixel_type, string rasterName, string cellsize, SpatialReference spat_reference, Envelope myExtent)
        {
            return await _rasterFactoryType.CreateRasterDataset(pixel_type, rasterName, cellsize, spat_reference, myExtent);
        }

        public async Task<RasterDataset> OpenRasterDataset(string rasterPath, string rasterName, bool bFile, FeatureLayer featLayer)
        {
            return await _rasterFactoryType.OpenRasterDataset(rasterPath, rasterName, bFile, featLayer);
        }

        public async Task<bool> SaveToNewRaster(RasterDataset rasterDS, string name, int Cellsize,
            SpatialReference spat_reference, Envelope myExtent, FeatureLayer featLayer)
        {
            return await _rasterFactoryType.SaveToNewRaster(rasterDS, name, Cellsize, spat_reference, myExtent, featLayer);
        }

        public async Task<FlapParameters> PrepareMovingStatisticsProcessing(FeatureLayer InputLayer, Envelope customEnvelope, int subCellsize,
    int nInterval, bool selectedFeatures, int rangeFrom, int rangeTo, RoseGeom roseGeom, bool bRegional, string fieldName, int XBlocks, int YBlocks, int TotalBlocks)
        {
            return await _rasterFactoryType.PrepareMovingStatisticsProcessing(InputLayer, customEnvelope, subCellsize, nInterval,
                selectedFeatures, rangeFrom, rangeTo, roseGeom, bRegional, fieldName, XBlocks, YBlocks, TotalBlocks);
        }
    }

    //abstract class 
    public abstract class TypeOfRasterAnalysis
    {
        public static TypeOfRasterAnalysis RasterStatistics(RasterLineamentAnalysis value)
        {
            switch (value)
            {
                case RasterLineamentAnalysis.DensityFrequency:
                    return new RasterDensityFrequency();

                case RasterLineamentAnalysis.DensityLength:
                    return new RasterDensityLength();

                case RasterLineamentAnalysis.GroupDominanceFrequency:
                    return new RasterGroupDomFreq();

                case RasterLineamentAnalysis.GroupDominanceLength:
                    return new RasterGroupDomLen();

                case RasterLineamentAnalysis.GroupMeansFrequency:
                    return new RasterGroupMeanFreq();

                case RasterLineamentAnalysis.GroupMeansLength:
                    return new RasterGroupMeanLen();

                case RasterLineamentAnalysis.RelativeEntropy:
                    return new RasterRelativeEntropy();

                default:
                    throw new Exception("Generic error with Raster analysis");
            }
        }

        public abstract Task<FlapParameters> PrepareInputForProcessing(FeatureLayer InputLayer, Envelope customEnvelope, int subCellsize,
            int nInterval, bool selectedFeatures, int rangeFrom, int rangeTo, RoseGeom roseGeom, bool bRegional, string fieldName);

        public abstract Task<FlapParameters> PrepareMovingStatisticsProcessing(FeatureLayer InputLayer, Envelope customEnvelope, int subCellsize,
    int nInterval, bool selectedFeatures, int rangeFrom, int rangeTo, RoseGeom roseGeom, bool bRegional, string fieldName, int XBlocks, int YBlocks, int TotalBlocks);

        public abstract Task<FeatureClass> CreatePolyFeatClass(string suffix, string rasterName, bool bExists, string databasePath,
            FeatureLayer InputLayer, SpatialReference thisSpatRef, string FClassType, bool bMovStats);

        public abstract Task<string> SaveAsVectorFeatures(FeatureClass OuputFeatureClass, SpatialReference mySpatialRef,
            FlapParameters _parameters, string FCType);

        public abstract void WriteRasterValuesToGrid(RasterDataset rasterDS, FlapParameters _parameters, bool bInt);

        public abstract void CalculateGridValues(FlapParameters _parameters);

        public abstract Task<bool> CreateRasterDataset(string pixel_type, string rasterName, string cellsize, SpatialReference spat_reference, Envelope myExtent);

        public abstract Task<bool> SaveToNewRaster(RasterDataset rasterDS, string name, int Cellsize,
            SpatialReference spat_reference, Envelope myExtent, FeatureLayer featLayer);

        public abstract Task<RasterDataset> OpenRasterDataset(string rasterPath, string rasterName, bool bFile, FeatureLayer featLayer);


    }

    public class RasterGroupDomFreq : TypeOfRasterAnalysis
    {
        public override async Task<FlapParameters> PrepareInputForProcessing(FeatureLayer InputLayer, Envelope customEnvelope, int subCellsize,
            int nInterval, bool selectedFeatures, int rangeFrom, int rangeTo, RoseGeom roseGeom, bool bRegional, string fieldName)
        {
            if (!selectedFeatures)
                return await VectorFunctions.PrepareInputForProcessing(InputLayer, customEnvelope, subCellsize, nInterval,
                rangeFrom, rangeTo, roseGeom, fieldName);
            else
                return await VectorFunctions.PrepareInputForProcessing(InputLayer, customEnvelope, subCellsize, nInterval,
                rangeFrom, rangeTo, roseGeom, fieldName, selectedFeatures);
        }

        public override async void CalculateGridValues(FlapParameters _parameters)
        {
            VectorFunctions.ProgressMessage = "Calculating Group Dominance Frequency...";
            await VectorFunctions.Progressor_NonCancelable();

            int totalLinesInCell = 0;
            int totalLinesInRange = 0;
            double rangeFrom = _parameters.RangeFrom;
            double rangeTo = _parameters.RangeTo;
            double gridVal = 0.0;

            List<double> rawValues = new List<double>();

            foreach (FlapParameter parameter in _parameters.flapParameters)
            {
                if (parameter.CreateCell)
                {
                    gridVal = 0.0;
                    totalLinesInCell = 0;

                    totalLinesInCell = parameter.LenAzi.Count;

                    totalLinesInRange = parameter.LenAzi.Where(f => f.Azimuth >= rangeFrom).Where(t => t.Azimuth <= rangeTo).Count();

                    parameter.Count = totalLinesInRange;

                    double division = Convert.ToDouble(totalLinesInRange) / Convert.ToDouble(totalLinesInCell);
                    double mul100 = division * 100;
                    gridVal = mul100;

                    parameter.GridValue = gridVal;
                    rawValues.Add(gridVal);
                }
                else
                {
                    parameter.GridValue = -99;
                    rawValues.Add(-99);
                }
            }

            for (int i = 0; i < rawValues.Count - 1; i++)
            {
                if (rawValues[i] > -99)
                {
                    _parameters.flapParameters[i].AdjustedValue = (255 / rawValues.Max()) * rawValues[i];
                }
                else
                    _parameters.flapParameters[i].AdjustedValue = -99;
            }
        }

        public override async Task<FeatureClass> CreatePolyFeatClass(string suffix, string rasterName, bool bExists, string databasePath,
            FeatureLayer InputLayer, SpatialReference thisSpatRef, string FClassType, bool bMovStats)
        {
            return await VectorFunctions.CreatePolyFeatClass(suffix, rasterName, bExists, databasePath, InputLayer,
                RasterLineamentAnalysis.GroupDominanceFrequency, thisSpatRef, FClassType, bMovStats);
        }

        public override async Task<string> SaveAsVectorFeatures(FeatureClass OuputFeatureClass, SpatialReference mySpatialRef,
            FlapParameters _parameters, string FCType)
        {
            if (FCType == "POLYGON")
                return await VectorFunctions.SaveRasterToPolygon(OuputFeatureClass, mySpatialRef, _parameters,
                    RasterLineamentAnalysis.GroupDominanceFrequency);
            else
                return await VectorFunctions.SaveRasterToPoint(OuputFeatureClass, mySpatialRef, _parameters,
                RasterLineamentAnalysis.GroupDominanceFrequency);
        }

        public override async void WriteRasterValuesToGrid(RasterDataset rasterDS, FlapParameters _parameters, bool bInt)
        {
            await RasterFunctions.WriteRasterValuesToGrid(rasterDS, _parameters, bInt);

        }

        public override async Task<bool> CreateRasterDataset(string pixel_type, string rasterName, string cellsize, SpatialReference spat_reference, Envelope myExtent)
        {
            await RasterFunctions.CreateRasterDataset(pixel_type, rasterName, cellsize, spat_reference, myExtent);

            return true;
        }

        public override async Task<RasterDataset> OpenRasterDataset(string rasterPath, string rasterName, bool bFile, FeatureLayer featLayer)
        {
            if (!bFile)
                return await RasterFunctions.OpenRasterDataset(rasterName, featLayer);
            else
                return await RasterFunctions.OpenTempRasterDataset(rasterPath, rasterName);

        }

        public override async Task<bool> SaveToNewRaster(RasterDataset rasterDS, string name, int Cellsize,
            SpatialReference spat_reference, Envelope myExtent, FeatureLayer featLayer)
        {
            await RasterFunctions.SaveToNewRaster(rasterDS, name, Cellsize, spat_reference, myExtent, featLayer);

            return true;
        }

        public override async Task<FlapParameters> PrepareMovingStatisticsProcessing(FeatureLayer InputLayer, Envelope customEnvelope, int subCellsize, int nInterval, bool selectedFeatures,
            int rangeFrom, int rangeTo, RoseGeom roseGeom, bool bRegional, string fieldName, int XBlocks, int YBlocks, int TotalBlocks)
        {
            if (!selectedFeatures)
                return await VectorFunctions.PrepareMovingStatisticsProcessing(InputLayer, customEnvelope, subCellsize, nInterval,
                rangeFrom, rangeTo, roseGeom, fieldName, XBlocks, YBlocks, TotalBlocks);
            else
                return await VectorFunctions.PrepareMovingStatisticsProcessing(InputLayer, customEnvelope, subCellsize, nInterval,
                rangeFrom, rangeTo, roseGeom, fieldName, selectedFeatures, XBlocks, YBlocks, TotalBlocks);
        }
    }

    public class RasterGroupDomLen : TypeOfRasterAnalysis
    {
        public override async Task<FlapParameters> PrepareInputForProcessing(FeatureLayer InputLayer, Envelope customEnvelope, int subCellsize,
                  int nInterval, bool selectedFeatures, int rangeFrom, int rangeTo, RoseGeom roseGeom, bool bRegional, string fieldName)
        {
            if (!selectedFeatures)
                return await VectorFunctions.PrepareInputForProcessing(InputLayer, customEnvelope, subCellsize, nInterval,
                rangeFrom, rangeTo, roseGeom, fieldName);
            else
                return await VectorFunctions.PrepareInputForProcessing(InputLayer, customEnvelope, subCellsize, nInterval,
                rangeFrom, rangeTo, roseGeom, fieldName, selectedFeatures);

        }

        public override async void CalculateGridValues(FlapParameters _parameters)
        {
            VectorFunctions.ProgressMessage = "Calculating Group Dominance Length...";
            await VectorFunctions.Progressor_NonCancelable();

            double totalLengthInCell = 0.0;
            double totalLengthInRange = 0.0;
            double rangeFrom = _parameters.RangeFrom;
            double rangeTo = _parameters.RangeTo;
            double gridVal = 0.0;

            List<double> rawValues = new List<double>();

            foreach (FlapParameter parameter in _parameters.flapParameters)
            {
                if (parameter.CreateCell)
                {
                    gridVal = 0.0;
                    totalLengthInCell = 0.0;
                    totalLengthInRange = 0.0;

                    totalLengthInCell = parameter.LenAzi.Select(l => l.Length).Sum();

                    totalLengthInRange = parameter.LenAzi.Where(f => f.Azimuth >= rangeFrom).Where(t => t.Azimuth <= rangeTo).Select(l => l.Length).Sum();

                    parameter.Count = parameter.LenAzi.Where(f => f.Azimuth >= rangeFrom).Where(t => t.Azimuth <= rangeTo).Count();

                    gridVal = (totalLengthInRange / totalLengthInCell) * 100;
                    parameter.GridValue = gridVal;
                    rawValues.Add(gridVal);
                }
                else
                {
                    parameter.GridValue = -99;
                    rawValues.Add(-99);
                }
            }

            for (int i = 0; i < rawValues.Count - 1; i++)
            {
                if (rawValues[i] > -99)
                {
                    _parameters.flapParameters[i].AdjustedValue = (255 / rawValues.Max()) * rawValues[i];
                }
                else
                    _parameters.flapParameters[i].AdjustedValue = -99;
            }
        }

        public override async Task<FeatureClass> CreatePolyFeatClass(string suffix, string rasterName, bool bExists, string databasePath,
            FeatureLayer InputLayer, SpatialReference thisSpatRef, string FClassType, bool bMovStats)
        {
            return await VectorFunctions.CreatePolyFeatClass(suffix, rasterName, bExists, databasePath, InputLayer,
                RasterLineamentAnalysis.GroupDominanceLength, thisSpatRef, FClassType, bMovStats);
        }

        public override async Task<string> SaveAsVectorFeatures(FeatureClass OuputFeatureClass, SpatialReference mySpatialRef,
            FlapParameters _parameters, string FCType)
        {
            if (FCType == "POLYGON")
                return await VectorFunctions.SaveRasterToPolygon(OuputFeatureClass, mySpatialRef, _parameters,
                    RasterLineamentAnalysis.GroupDominanceLength);
            else
                return await VectorFunctions.SaveRasterToPoint(OuputFeatureClass, mySpatialRef, _parameters,
                RasterLineamentAnalysis.GroupDominanceLength);
        }

        public override async void WriteRasterValuesToGrid(RasterDataset rasterDS, FlapParameters _parameters, bool bInt)
        {
            await RasterFunctions.WriteRasterValuesToGrid(rasterDS, _parameters, bInt);

        }

        public override async Task<bool> CreateRasterDataset(string pixel_type, string rasterName, string cellsize, SpatialReference spat_reference, Envelope myExtent)
        {
            await RasterFunctions.CreateRasterDataset(pixel_type, rasterName, cellsize, spat_reference, myExtent);

            return true;
        }

        public override async Task<RasterDataset> OpenRasterDataset(string rasterPath, string rasterName, bool bFile, FeatureLayer featLayer)
        {
            if (!bFile)
                return await RasterFunctions.OpenRasterDataset(rasterName, featLayer);
            else
                return await RasterFunctions.OpenTempRasterDataset(rasterPath, rasterName);
        }

        public override async Task<bool> SaveToNewRaster(RasterDataset rasterDS, string name, int Cellsize,
            SpatialReference spat_reference, Envelope myExtent, FeatureLayer featLayer)
        {
            await RasterFunctions.SaveToNewRaster(rasterDS, name, Cellsize, spat_reference, myExtent, featLayer);

            return true;
        }

        public override async Task<FlapParameters> PrepareMovingStatisticsProcessing(FeatureLayer InputLayer, Envelope customEnvelope, int subCellsize, int nInterval, bool selectedFeatures,
            int rangeFrom, int rangeTo, RoseGeom roseGeom, bool bRegional, string fieldName, int XBlocks, int YBlocks, int TotalBlocks)
        {
            if (!selectedFeatures)
                return await VectorFunctions.PrepareMovingStatisticsProcessing(InputLayer, customEnvelope, subCellsize, nInterval,
                rangeFrom, rangeTo, roseGeom, fieldName, XBlocks, YBlocks, TotalBlocks);
            else
                return await VectorFunctions.PrepareMovingStatisticsProcessing(InputLayer, customEnvelope, subCellsize, nInterval,
                rangeFrom, rangeTo, roseGeom, fieldName, selectedFeatures, XBlocks, YBlocks, TotalBlocks);
        }
    }

    public class RasterDensityLength : TypeOfRasterAnalysis
    {
        public override async Task<FlapParameters> PrepareInputForProcessing(FeatureLayer InputLayer, Envelope customEnvelope, int subCellsize,
           int nInterval, bool selectedFeatures, int rangeFrom, int rangeTo, RoseGeom roseGeom, bool bRegional, string fieldName)
        {
            if (!selectedFeatures)
                return await VectorFunctions.PrepareInputForProcessing(InputLayer, customEnvelope, subCellsize, nInterval,
                rangeFrom, rangeTo, roseGeom, fieldName);
            else
                return await VectorFunctions.PrepareInputForProcessing(InputLayer, customEnvelope, subCellsize, nInterval,
                rangeFrom, rangeTo, roseGeom, fieldName, selectedFeatures);

        }

        public override async void CalculateGridValues(FlapParameters _parameters)
        {
            VectorFunctions.ProgressMessage = "Calculating Density Length...";
            await VectorFunctions.Progressor_NonCancelable();

            double totalLength = 0.0;
            int totalCount = 0;
            double rangeFrom = _parameters.RangeFrom;
            double rangeTo = _parameters.RangeTo;
            double gridVal = 0.0;

            List<double> rawValues = new List<double>();

            foreach (FlapParameter parameter in _parameters.flapParameters)
            {
                if (parameter.CreateCell)
                {
                    gridVal = 0.0;
                    //parameter.GridValue = 0.0;
                    totalLength = 0;
                    totalCount = 0;

                    totalLength = parameter.LenAzi.Where(f => f.Azimuth >= rangeFrom).Where(t => t.Azimuth <= rangeTo).Select(l => l.Length).Sum();
                    totalCount = parameter.LenAzi.Where(f => f.Azimuth >= rangeFrom).Where(t => t.Azimuth <= rangeTo).Count();
                    parameter.GridValue = 0.0;
                    parameter.Count = totalCount;

                    //calculate density frequency for area
                    gridVal = (totalLength / parameter.ExtentArea);

                    rawValues.Add(gridVal);
                    parameter.GridValue = gridVal;
                }
                else
                {
                    parameter.GridValue = -99;
                    rawValues.Add(-99);
                }
            }

            for (int i = 0; i < rawValues.Count - 1; i++)
            {
                if (rawValues[i] > -99)
                {
                    _parameters.flapParameters[i].AdjustedValue = (255 / rawValues.Max()) * rawValues[i];
                }
                else
                    _parameters.flapParameters[i].AdjustedValue = -99;
            }

        }

        public override async Task<FeatureClass> CreatePolyFeatClass(string suffix, string rasterName, bool bExists, string databasePath,
            FeatureLayer InputLayer, SpatialReference thisSpatRef, string FClassType, bool bMovStats)
        {

            return await VectorFunctions.CreatePolyFeatClass(suffix, rasterName, bExists, databasePath, InputLayer,
                RasterLineamentAnalysis.DensityLength, thisSpatRef, FClassType, bMovStats);

        }

        public override async Task<string> SaveAsVectorFeatures(FeatureClass OuputFeatureClass, SpatialReference mySpatialRef,
            FlapParameters _parameters, string FCType)
        {
            if (FCType == "POLYGON")
                return await VectorFunctions.SaveRasterToPolygon(OuputFeatureClass, mySpatialRef, _parameters,
                    RasterLineamentAnalysis.DensityLength);
            else
                return await VectorFunctions.SaveRasterToPoint(OuputFeatureClass, mySpatialRef, _parameters,
                RasterLineamentAnalysis.DensityLength);
        }

        public override async void WriteRasterValuesToGrid(RasterDataset rasterDS, FlapParameters _parameters, bool bInt)
        {
            await RasterFunctions.WriteRasterValuesToGrid(rasterDS, _parameters, bInt);

        }

        public override async Task<bool> CreateRasterDataset(string pixel_type, string rasterName, string cellsize, SpatialReference spat_reference, Envelope myExtent)
        {
            await RasterFunctions.CreateRasterDataset(pixel_type, rasterName, cellsize, spat_reference, myExtent);

            return true;
        }

        public override async Task<RasterDataset> OpenRasterDataset(string rasterPath, string rasterName, bool bFile, FeatureLayer featLayer)
        {
            if (!bFile)
                return await RasterFunctions.OpenRasterDataset(rasterName, featLayer);
            else
                return await RasterFunctions.OpenTempRasterDataset(rasterPath, rasterName);
        }

        public override async Task<bool> SaveToNewRaster(RasterDataset rasterDS, string name, int Cellsize,
            SpatialReference spat_reference, Envelope myExtent, FeatureLayer featLayer)
        {
            await RasterFunctions.SaveToNewRaster(rasterDS, name, Cellsize, spat_reference, myExtent, featLayer);

            return true;
        }

        public override async Task<FlapParameters> PrepareMovingStatisticsProcessing(FeatureLayer InputLayer, Envelope customEnvelope, int subCellsize, int nInterval, bool selectedFeatures,
            int rangeFrom, int rangeTo, RoseGeom roseGeom, bool bRegional, string fieldName, int XBlocks, int YBlocks, int TotalBlocks)
        {
            if (!selectedFeatures)
                return await VectorFunctions.PrepareMovingStatisticsProcessing(InputLayer, customEnvelope, subCellsize, nInterval,
                rangeFrom, rangeTo, roseGeom, fieldName, XBlocks, YBlocks, TotalBlocks);
            else
                return await VectorFunctions.PrepareMovingStatisticsProcessing(InputLayer, customEnvelope, subCellsize, nInterval,
                rangeFrom, rangeTo, roseGeom, fieldName, selectedFeatures, XBlocks, YBlocks, TotalBlocks);
        }
    }

    public class RasterDensityFrequency : TypeOfRasterAnalysis
    {
        public override async Task<FlapParameters> PrepareInputForProcessing(FeatureLayer InputLayer, Envelope customEnvelope, int subCellsize,
            int nInterval, bool selectedFeatures, int rangeFrom, int rangeTo, RoseGeom roseGeom, bool bRegional, string fieldName)
        {
            if (!selectedFeatures)
                return await VectorFunctions.PrepareInputForProcessing(InputLayer, customEnvelope, subCellsize, nInterval,
                rangeFrom, rangeTo, roseGeom, fieldName);
            else
                return await VectorFunctions.PrepareInputForProcessing(InputLayer, customEnvelope, subCellsize, nInterval,
                rangeFrom, rangeTo, roseGeom, fieldName, selectedFeatures);

        }

        public override async void CalculateGridValues(FlapParameters _parameters)
        {
            VectorFunctions.ProgressMessage = "Calculating Density Frequency...";
            await VectorFunctions.Progressor_NonCancelable();

            int totalLines = 0;
            double rangeFrom = _parameters.RangeFrom;
            double rangeTo = _parameters.RangeTo;
            double gridVal = 0.0;

            List<double> rawValues = new List<double>();

            foreach (FlapParameter parameter in _parameters.flapParameters)
            {
                if (parameter.CreateCell)
                {
                    gridVal = 0.0;
                    totalLines = 0;

                    totalLines = parameter.LenAzi.Where(f => f.Azimuth >= rangeFrom).Where(t => t.Azimuth <= rangeTo).Count();

                    parameter.Count = totalLines;

                    //calculate density frequency for area
                    gridVal = (totalLines / parameter.ExtentArea) * 1000;
                    parameter.GridValue = gridVal;
                    rawValues.Add(gridVal);
                }
                else
                {
                    parameter.GridValue = -99;
                    rawValues.Add(-99);
                }
            }

            for (int i = 0; i < rawValues.Count - 1; i++)
            {
                if (rawValues[i] > -99)
                {
                    _parameters.flapParameters[i].AdjustedValue = ((255 / rawValues.Max()) * rawValues[i]);
                }
                else
                    _parameters.flapParameters[i].AdjustedValue = -99;
            }
        }

        public override async Task<FeatureClass> CreatePolyFeatClass(string suffix, string rasterName, bool bExists, string databasePath,
            FeatureLayer InputLayer, SpatialReference thisSpatRef, string FClassType, bool bMovStats)
        {

            return await VectorFunctions.CreatePolyFeatClass(suffix, rasterName, bExists, databasePath, InputLayer,
                RasterLineamentAnalysis.DensityFrequency, thisSpatRef, FClassType, bMovStats);

        }

        public override async Task<string> SaveAsVectorFeatures(FeatureClass OuputFeatureClass, SpatialReference mySpatialRef,
            FlapParameters _parameters, string FCType)
        {
            if (FCType == "POLYGON")
                return await VectorFunctions.SaveRasterToPolygon(OuputFeatureClass, mySpatialRef, _parameters,
                    RasterLineamentAnalysis.DensityFrequency);
            else
                return await VectorFunctions.SaveRasterToPoint(OuputFeatureClass, mySpatialRef, _parameters,
                RasterLineamentAnalysis.DensityFrequency);
        }

        public override async void WriteRasterValuesToGrid(RasterDataset rasterDS, FlapParameters _parameters, bool bInt)
        {
            await RasterFunctions.WriteRasterValuesToGrid(rasterDS, _parameters, bInt);

        }

        public override async Task<bool> CreateRasterDataset(string pixel_type, string rasterName, string cellsize, SpatialReference spat_reference, Envelope myExtent)
        {
            await RasterFunctions.CreateRasterDataset(pixel_type, rasterName, cellsize, spat_reference, myExtent);

            return true;
        }

        public override async Task<RasterDataset> OpenRasterDataset(string rasterPath, string rasterName, bool bFile, FeatureLayer featLayer)
        {
            if (!bFile)
                return await RasterFunctions.OpenRasterDataset(rasterName, featLayer);
            else
                return await RasterFunctions.OpenTempRasterDataset(rasterPath, rasterName);
        }

        public override async Task<bool> SaveToNewRaster(RasterDataset rasterDS, string name, int Cellsize,
            SpatialReference spat_reference, Envelope myExtent, FeatureLayer featLayer)
        {
            await RasterFunctions.SaveToNewRaster(rasterDS, name, Cellsize, spat_reference, myExtent, featLayer);

            return true;
        }

        public override async Task<FlapParameters> PrepareMovingStatisticsProcessing(FeatureLayer InputLayer, Envelope customEnvelope, int subCellsize, int nInterval, bool selectedFeatures,
            int rangeFrom, int rangeTo, RoseGeom roseGeom, bool bRegional, string fieldName, int XBlocks, int YBlocks, int TotalBlocks)
        {
            if (!selectedFeatures)
                return await VectorFunctions.PrepareMovingStatisticsProcessing(InputLayer, customEnvelope, subCellsize, nInterval,
                rangeFrom, rangeTo, roseGeom, fieldName, XBlocks, YBlocks, TotalBlocks);
            else
                return await VectorFunctions.PrepareMovingStatisticsProcessing(InputLayer, customEnvelope, subCellsize, nInterval,
                rangeFrom, rangeTo, roseGeom, fieldName, selectedFeatures, XBlocks, YBlocks, TotalBlocks);
        }
    }

    public class RasterGroupMeanFreq : TypeOfRasterAnalysis
    {
        public override async Task<FlapParameters> PrepareInputForProcessing(FeatureLayer InputLayer, Envelope customEnvelope, int subCellsize,
           int nInterval, bool selectedFeatures, int rangeFrom, int rangeTo, RoseGeom roseGeom, bool bRegional, string fieldName)
        {
            if (!selectedFeatures)
                return await VectorFunctions.PrepareInputForProcessing(InputLayer, customEnvelope, subCellsize, nInterval,
                rangeFrom, rangeTo, roseGeom, fieldName);
            else
                return await VectorFunctions.PrepareInputForProcessing(InputLayer, customEnvelope, subCellsize, nInterval,
                rangeFrom, rangeTo, roseGeom, fieldName, selectedFeatures);

        }

        public override async void CalculateGridValues(FlapParameters _parameters)
        {
            VectorFunctions.ProgressMessage = "Calculating Group Mean Frequency...";
            await VectorFunctions.Progressor_NonCancelable();

            double sumLines = 0.0;
            double sumAngles = 0.0;
            double rangeFrom = _parameters.RangeFrom;
            double rangeTo = _parameters.RangeTo;
            double gridVal = 0.0;

            List<double> rawValues = new List<double>();

            foreach (FlapParameter parameter in _parameters.flapParameters)
            {
                if (parameter.CreateCell)
                {
                    gridVal = 0.0;
                    sumLines = 0;
                    sumAngles = 0;

                    sumLines = parameter.LenAzi.Where(f => f.Azimuth >= rangeFrom).Where(t => t.Azimuth <= rangeTo).Select(a => a.Length).Sum();
                    sumAngles = parameter.LenAzi.Where(f => f.Azimuth >= rangeFrom).Where(t => t.Azimuth <= rangeTo).Select(a => a.Azimuth).Sum();

                    parameter.Count = parameter.LenAzi.Where(f => f.Azimuth >= rangeFrom).Where(t => t.Azimuth <= rangeTo).Count();

                    //calculate density frequency for area
                    gridVal = (sumAngles / sumLines);
                    parameter.GridValue = gridVal;
                    rawValues.Add(gridVal);
                }
                else
                {
                    parameter.GridValue = -99;
                    rawValues.Add(-99);
                }
            }

            for (int i = 0; i < rawValues.Count - 1; i++)
            {
                if (rawValues[i] > -99)
                {
                    _parameters.flapParameters[i].AdjustedValue = (255 / rawValues.Max()) * rawValues[i];
                }
                else
                    _parameters.flapParameters[i].AdjustedValue = -99;
            }
        }

        public override async Task<FeatureClass> CreatePolyFeatClass(string suffix, string rasterName, bool bExists, string databasePath,
            FeatureLayer InputLayer, SpatialReference thisSpatRef, string FClassType, bool bMovStats)
        {

            return await VectorFunctions.CreatePolyFeatClass(suffix, rasterName, bExists, databasePath, InputLayer,
                RasterLineamentAnalysis.GroupMeansFrequency, thisSpatRef, FClassType, bMovStats);

        }

        public override async Task<string> SaveAsVectorFeatures(FeatureClass OuputFeatureClass, SpatialReference mySpatialRef,
            FlapParameters _parameters, string FCType)
        {
            if (FCType == "POLYGON")
                return await VectorFunctions.SaveRasterToPolygon(OuputFeatureClass, mySpatialRef, _parameters,
                    RasterLineamentAnalysis.GroupMeansFrequency);
            else
                return await VectorFunctions.SaveRasterToPoint(OuputFeatureClass, mySpatialRef, _parameters,
                RasterLineamentAnalysis.GroupMeansFrequency);

        }

        public override async void WriteRasterValuesToGrid(RasterDataset rasterDS, FlapParameters _parameters, bool bInt)
        {
            await RasterFunctions.WriteRasterValuesToGrid(rasterDS, _parameters, bInt);

        }

        public override async Task<bool> CreateRasterDataset(string pixel_type, string rasterName, string cellsize, SpatialReference spat_reference, Envelope myExtent)
        {
            await RasterFunctions.CreateRasterDataset(pixel_type, rasterName, cellsize, spat_reference, myExtent);

            return true;
        }

        public override async Task<RasterDataset> OpenRasterDataset(string rasterPath, string rasterName, bool bFile, FeatureLayer featLayer)
        {
            if (!bFile)
                return await RasterFunctions.OpenRasterDataset(rasterName, featLayer);
            else
                return await RasterFunctions.OpenTempRasterDataset(rasterPath, rasterName);
        }

        public override async Task<bool> SaveToNewRaster(RasterDataset rasterDS, string name, int Cellsize,
            SpatialReference spat_reference, Envelope myExtent, FeatureLayer featLayer)
        {
            await RasterFunctions.SaveToNewRaster(rasterDS, name, Cellsize, spat_reference, myExtent, featLayer);

            return true;
        }


        public override async Task<FlapParameters> PrepareMovingStatisticsProcessing(FeatureLayer InputLayer, Envelope customEnvelope, int subCellsize, int nInterval, bool selectedFeatures,
            int rangeFrom, int rangeTo, RoseGeom roseGeom, bool bRegional, string fieldName, int XBlocks, int YBlocks, int TotalBlocks)
        {
            if (!selectedFeatures)
                return await VectorFunctions.PrepareMovingStatisticsProcessing(InputLayer, customEnvelope, subCellsize, nInterval,
                rangeFrom, rangeTo, roseGeom, fieldName, XBlocks, YBlocks, TotalBlocks);
            else
                return await VectorFunctions.PrepareMovingStatisticsProcessing(InputLayer, customEnvelope, subCellsize, nInterval,
                rangeFrom, rangeTo, roseGeom, fieldName, selectedFeatures, XBlocks, YBlocks, TotalBlocks);
        }
    }

    public class RasterGroupMeanLen : TypeOfRasterAnalysis
    {

        public override async Task<FlapParameters> PrepareInputForProcessing(FeatureLayer InputLayer, Envelope customEnvelope, int subCellsize,
             int nInterval, bool selectedFeatures, int rangeFrom, int rangeTo, RoseGeom roseGeom, bool bRegional, string fieldName)
        {
            if (!selectedFeatures)
                return await VectorFunctions.PrepareInputForProcessing(InputLayer, customEnvelope, subCellsize, nInterval,
                rangeFrom, rangeTo, roseGeom, fieldName);
            else
                return await VectorFunctions.PrepareInputForProcessing(InputLayer, customEnvelope, subCellsize, nInterval,
                rangeFrom, rangeTo, roseGeom, fieldName, selectedFeatures);

        }

        public override async void CalculateGridValues(FlapParameters _parameters)
        {
            VectorFunctions.ProgressMessage = "Calculating Group Mean Length...";
            await VectorFunctions.Progressor_NonCancelable();

            double sumAngles = 0.0;
            double sumOfLines = 0.0;
            double sumOfAngles = 0.0;

            double rangeFrom = _parameters.RangeFrom;
            double rangeTo = _parameters.RangeTo;

            double gridVal = 0.0;

            List<double> rawValues = new List<double>();

            foreach (FlapParameter parameter in _parameters.flapParameters)
            {
                if (parameter.CreateCell)
                {
                    gridVal = 0.0;

                    sumAngles = 0;
                    sumOfLines = 0.0;
                    sumOfAngles = 0.0;

                    sumOfLines = parameter.LenAzi.Where(f => f.Azimuth >= rangeFrom).Where(t => t.Azimuth <= rangeTo).Select(a => a.Length).Sum();
                    sumAngles = parameter.LenAzi.Where(f => f.Azimuth >= rangeFrom).Where(t => t.Azimuth <= rangeTo).Select(a => a.Azimuth).Sum();
                    sumOfAngles = sumOfLines * sumAngles;

                    parameter.GridValue = 0.0;
                    parameter.Count = parameter.LenAzi.Where(f => f.Azimuth >= rangeFrom).Where(t => t.Azimuth <= rangeTo).Count();

                    //calculate density frequency for area
                    gridVal = (sumOfAngles / sumOfLines);
                    parameter.GridValue = gridVal;
                    rawValues.Add(gridVal);
                }
                else
                {
                    parameter.GridValue = -99;
                    rawValues.Add(-99);
                }
            }

            for (int i = 0; i < rawValues.Count - 1; i++)
            {
                if (rawValues[i] > -99)
                {
                    _parameters.flapParameters[i].AdjustedValue = (255 / rawValues.Max()) * rawValues[i];
                }
                else
                    _parameters.flapParameters[i].AdjustedValue = -99;
            }
        }

        public override async Task<FeatureClass> CreatePolyFeatClass(string suffix, string rasterName, bool bExists, string databasePath,
            FeatureLayer InputLayer, SpatialReference thisSpatRef, string FClassType, bool bMovStats)
        {

            return await VectorFunctions.CreatePolyFeatClass(suffix, rasterName, bExists, databasePath, InputLayer,
                RasterLineamentAnalysis.GroupMeansLength, thisSpatRef, FClassType, bMovStats);

        }

        public override async Task<string> SaveAsVectorFeatures(FeatureClass OuputFeatureClass, SpatialReference mySpatialRef,
            FlapParameters _parameters, string FCType)
        {
            if (FCType == "POLYGON")
                return await VectorFunctions.SaveRasterToPolygon(OuputFeatureClass, mySpatialRef, _parameters,
                    RasterLineamentAnalysis.GroupMeansLength);
            else
                return await VectorFunctions.SaveRasterToPoint(OuputFeatureClass, mySpatialRef, _parameters,
                RasterLineamentAnalysis.GroupMeansLength);

        }

        public override async void WriteRasterValuesToGrid(RasterDataset rasterDS, FlapParameters _parameters, bool bInt)
        {
            await RasterFunctions.WriteRasterValuesToGrid(rasterDS, _parameters, bInt);

        }

        public override async Task<bool> CreateRasterDataset(string pixel_type, string rasterName, string cellsize, SpatialReference spat_reference, Envelope myExtent)
        {
            await RasterFunctions.CreateRasterDataset(pixel_type, rasterName, cellsize, spat_reference, myExtent);

            return true;
        }

        public override async Task<RasterDataset> OpenRasterDataset(string rasterPath, string rasterName, bool bFile, FeatureLayer featLayer)
        {
            if (!bFile)
                return await RasterFunctions.OpenRasterDataset(rasterName, featLayer);
            else
                return await RasterFunctions.OpenTempRasterDataset(rasterPath, rasterName);
        }

        public override async Task<bool> SaveToNewRaster(RasterDataset rasterDS, string name, int Cellsize,
            SpatialReference spat_reference, Envelope myExtent, FeatureLayer featLayer)
        {
            await RasterFunctions.SaveToNewRaster(rasterDS, name, Cellsize, spat_reference, myExtent, featLayer);

            return true;
        }


        public override async Task<FlapParameters> PrepareMovingStatisticsProcessing(FeatureLayer InputLayer, Envelope customEnvelope, int subCellsize, int nInterval, bool selectedFeatures,
            int rangeFrom, int rangeTo, RoseGeom roseGeom, bool bRegional, string fieldName, int XBlocks, int YBlocks, int TotalBlocks)
        {
            if (!selectedFeatures)
                return await VectorFunctions.PrepareMovingStatisticsProcessing(InputLayer, customEnvelope, subCellsize, nInterval,
                rangeFrom, rangeTo, roseGeom, fieldName, XBlocks, YBlocks, TotalBlocks);
            else
                return await VectorFunctions.PrepareMovingStatisticsProcessing(InputLayer, customEnvelope, subCellsize, nInterval,
                rangeFrom, rangeTo, roseGeom, fieldName, selectedFeatures, XBlocks, YBlocks, TotalBlocks);
        }
    }

    public class RasterRelativeEntropy : TypeOfRasterAnalysis
    {
        public override async Task<FlapParameters> PrepareInputForProcessing(FeatureLayer InputLayer, Envelope customEnvelope, int subCellsize,
            int nInterval, bool selectedFeatures, int rangeFrom, int rangeTo, RoseGeom roseGeom, bool bRegional, string fieldName)
        {
            if (!selectedFeatures)
                return await VectorFunctions.PrepareInputForProcessing(InputLayer, customEnvelope, subCellsize, nInterval,
                rangeFrom, rangeTo, roseGeom, fieldName);
            else
                return await VectorFunctions.PrepareInputForProcessing(InputLayer, customEnvelope, subCellsize, nInterval,
                rangeFrom, rangeTo, roseGeom, fieldName, selectedFeatures);
        }

        public override async void CalculateGridValues(FlapParameters _parameters)
        {
            VectorFunctions.ProgressMessage = "Calculating Relative Entropy...";
            await VectorFunctions.Progressor_NonCancelable();

            int Interval = _parameters.Interval;
            int noOfSectors = 180 / Interval;
            int cellsize = _parameters.SubCellsize;

            int totalLines = 0;
            double rFrequency = 0.0;
            double entropy = 0.0;
            double rentropy = 0.0;

            List<double> rawValues = new List<double>();

            foreach (FlapParameter parameter in _parameters.flapParameters)
            {
                if (parameter.CreateCell)
                {
                    totalLines = 0;
                    rFrequency = 0;
                    entropy = 0.0;
                    rentropy = 0.0;

                    totalLines = parameter.LenAzi.Count;

                    //calculate frequency
                    parameter.CalcArrayValues(false, noOfSectors, RoseType.Frequency, Interval);

                    for (int i = 0; i < (noOfSectors) - 1; i++)
                    {
                        rFrequency = parameter.ArrayValues[i] / totalLines;

                        if (rFrequency != 0)
                        {
                            entropy = entropy + (rFrequency * Math.Log(rFrequency));
                        }
                    }

                    rentropy = -100 * (entropy / Math.Log(noOfSectors));

                    parameter.GridValue = rentropy;
                    rawValues.Add(rentropy);
                }
                else
                {
                    parameter.GridValue = -99;
                    rawValues.Add(-99);
                }
            }

            for (int i = 0; i < rawValues.Count - 1; i++)
            {
                if (rawValues[i] > -99)
                {
                    _parameters.flapParameters[i].AdjustedValue = (255 / rawValues.Max()) * rawValues[i];
                }
                else
                    _parameters.flapParameters[i].AdjustedValue = -99;
            }
        }

        public override async Task<FeatureClass> CreatePolyFeatClass(string suffix, string rasterName, bool bExists, string databasePath,
            FeatureLayer InputLayer, SpatialReference thisSpatRef, string FClassType, bool bMovStats)
        {

            return await VectorFunctions.CreatePolyFeatClass(suffix, rasterName, bExists, databasePath, InputLayer,
                RasterLineamentAnalysis.RelativeEntropy, thisSpatRef, FClassType, bMovStats);

        }

        public override async Task<string> SaveAsVectorFeatures(FeatureClass OuputFeatureClass, SpatialReference mySpatialRef,
            FlapParameters _parameters, string FCType)
        {
            if (FCType == "POLYGON")
                return await VectorFunctions.SaveRasterToPolygon(OuputFeatureClass, mySpatialRef, _parameters,
                    RasterLineamentAnalysis.RelativeEntropy);
            else
                return await VectorFunctions.SaveRasterToPoint(OuputFeatureClass, mySpatialRef, _parameters,
                RasterLineamentAnalysis.RelativeEntropy);

        }

        public override async void WriteRasterValuesToGrid(RasterDataset rasterDS, FlapParameters _parameters, bool bInt)
        {
            await RasterFunctions.WriteRasterValuesToGrid(rasterDS, _parameters, bInt);
        }

        public override async Task<bool> CreateRasterDataset(string pixel_type, string rasterName, string cellsize,
            SpatialReference spat_reference, Envelope myExtent)
        {
            await RasterFunctions.CreateRasterDataset(pixel_type, rasterName, cellsize, spat_reference, myExtent);

            return true;
        }

        public override async Task<RasterDataset> OpenRasterDataset(string rasterPath, string rasterName, bool bFile, FeatureLayer featLayer)
        {
            if (!bFile)
                return await RasterFunctions.OpenRasterDataset(rasterName, featLayer);
            else
                return await RasterFunctions.OpenTempRasterDataset(rasterPath, rasterName);
        }

        public override async Task<bool> SaveToNewRaster(RasterDataset rasterDS, string name, int Cellsize,
            SpatialReference spat_reference, Envelope myExtent, FeatureLayer featLayer)
        {
            await RasterFunctions.SaveToNewRaster(rasterDS, name, Cellsize, spat_reference, myExtent, featLayer);

            return true;
        }

        public override async Task<FlapParameters> PrepareMovingStatisticsProcessing(FeatureLayer InputLayer, Envelope customEnvelope, int subCellsize, int nInterval, bool selectedFeatures,
            int rangeFrom, int rangeTo, RoseGeom roseGeom, bool bRegional, string fieldName, int XBlocks, int YBlocks, int TotalBlocks)
        {
            if (!selectedFeatures)
                return await VectorFunctions.PrepareMovingStatisticsProcessing(InputLayer, customEnvelope, subCellsize, nInterval,
                rangeFrom, rangeTo, roseGeom, fieldName, XBlocks, YBlocks, TotalBlocks);
            else
                return await VectorFunctions.PrepareMovingStatisticsProcessing(InputLayer, customEnvelope, subCellsize, nInterval,
                rangeFrom, rangeTo, roseGeom, fieldName, selectedFeatures, XBlocks, YBlocks, TotalBlocks);
        }
    }

}
