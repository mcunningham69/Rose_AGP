using ArcGIS.Core.Data;
using ArcGIS.Desktop.Mapping;
using Rose_AGP.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Core.Geometry;

namespace Rose_AGP
{
    //Helper Class for abstract referen
    //

    public class RoseFactoryPreview
    {
        public TypeOfAnalysis _factoryType;

        public RoseFactoryPreview(RoseLineamentAnalysis value)
        {

            SetAnalysisDataset(value);
        }

        public void SetAnalysisDataset(RoseLineamentAnalysis value)
        {
            _factoryType = TypeOfAnalysis.RoseStatistics(value);
        }

        public async Task<FlapParameters> PrepareInputForProcessing(FeatureLayer InputLayer, Envelope customEnvelope, int subCellsize,
    int nInterval, bool selectedFeatures, RoseGeom roseGeom, bool bRegional, string fieldName)
        {
            return await _factoryType.PrepareInputForProcessing(InputLayer, customEnvelope, subCellsize, nInterval,
                selectedFeatures, roseGeom, bRegional, fieldName);
        }

        public async Task<FeatureClass> CreateFeatureClass(string suffix, string roseName, bool bExists, string databasePath,
    FeatureLayer InputLayer, bool bStatistics, RoseGeom roseGeom, SpatialReference thisSpatRef)
        {
            return await _factoryType.CreateFeatureClass(suffix, roseName, bExists, databasePath, InputLayer,
                bStatistics, roseGeom, thisSpatRef);

        }

        public async Task<string> SaveAsVectorFeatures(FeatureClass OuputFeatureClass, SpatialReference mySpatialRef,
            FlapParameters _parameters, bool bExtentOnly)
        {
            return await _factoryType.SaveAsVectorFeatures(OuputFeatureClass, mySpatialRef, _parameters, bExtentOnly);
        }

        public void CalculateVectorValues(FlapParameters _parameters, bool Statistics, RoseType roseType, bool bDirection)
        {
            _factoryType.CalculateVectorValues(_parameters, Statistics, roseType, bDirection);
        }

    }

    //abstract class 
    public abstract class TypeOfAnalysis
    {
        public static TypeOfAnalysis RoseStatistics(RoseLineamentAnalysis value)
        {
            switch (value)
            {
                case RoseLineamentAnalysis.RoseCells:
                    return new RoseCellPlots();

                case RoseLineamentAnalysis.RoseRegional:
                    return new RoseRegionalPlot();

                case RoseLineamentAnalysis.Fishnet:
                    return new FishnetCoverage();

                default:
                    throw new Exception("Generic error with rose analysis");
            }
        }

        public abstract Task<FlapParameters> PrepareInputForProcessing(FeatureLayer InputLayer, Envelope customEnvelope, int subCellsize,
            int nInterval, bool selectedFeatures, RoseGeom roseGeom, bool bRegional, string fieldName);

        public abstract Task<FeatureClass> CreateFeatureClass(string suffix, string roseName, bool bExists, string databasePath,
    FeatureLayer InputLayer, bool bStatistics, RoseGeom roseGeom, SpatialReference thisSpatRef);

        public abstract Task<string> SaveAsVectorFeatures(FeatureClass OuputFeatureClass, SpatialReference mySpatialRef,
            FlapParameters _parameters, bool bExtentOnly);

        public abstract void CalculateVectorValues(FlapParameters _parameters, bool Statistics, RoseType roseType, bool bDirection);

    }

    public class RoseCellPlots : TypeOfAnalysis
    {
        public override void CalculateVectorValues(FlapParameters _parameters, bool Statistics, RoseType roseType, bool bDirection)
        {
            int NoOfElements = 180 / _parameters.Interval;

            foreach (FlapParameter parameter in _parameters.flapParameters)
            {
                if (parameter.CreateCell)
                {
                    parameter.Rose = new RoseDiagramParameters();
                    parameter.Rose.RoseArrayValues(bDirection, NoOfElements, _parameters.Interval, parameter.LenAzi, roseType);
                    parameter.Rose.CalculateRosePetals(_parameters.Interval, parameter.ExtentWidth, parameter.ExtentHeight);
                    parameter.Rose.CalculateRoseStatistics(parameter.LenAzi, _parameters.Interval);
                }
            }
        }

        public override async Task<FlapParameters> PrepareInputForProcessing(FeatureLayer InputLayer,
            Envelope customEnvelope, int subCellsize, int nInterval, bool selectedFeatures, RoseGeom roseGeom,
            bool bRegional, string fieldName)
        {
            if (selectedFeatures)
                return await VectorFunctions.PrepareInputForProcessing(InputLayer, customEnvelope, subCellsize, nInterval,
               0, 0, roseGeom, fieldName, selectedFeatures);
            else
                return await VectorFunctions.PrepareInputForProcessing(InputLayer, customEnvelope, subCellsize, nInterval,
                    0, 0, roseGeom, fieldName);
        }

        public override async Task<string> SaveAsVectorFeatures(FeatureClass OuputFeatureClass,
            SpatialReference mySpatialRef, FlapParameters _parameters, bool bExtentOnly)
        {
            return await VectorFunctions.SaveRoseToVectorFeatures(OuputFeatureClass, mySpatialRef, _parameters);
        }

        public override async Task<FeatureClass> CreateFeatureClass(string suffix, string roseName, bool bExists,
            string databasePath, FeatureLayer InputLayer, bool bStatistics, RoseGeom roseGeom, SpatialReference thisSpatRef)
        {
            return await VectorFunctions.CreateRoseNetFeatClass(suffix, roseName, bExists, databasePath, InputLayer,
            bStatistics, roseGeom, false, thisSpatRef);
        }
    }

    public class FishnetCoverage : TypeOfAnalysis
    {

        public override void CalculateVectorValues(FlapParameters _parameters, bool Statistics, RoseType roseType, bool bDirection)
        {
            foreach (FlapParameter parameter in _parameters.flapParameters)
            {
                if (parameter.CreateCell)
                {
                    if (parameter.LenAzi.Count > 0)
                    {
                        parameter.FishStats = new FishnetStatistics();

                        parameter.FishStats.CalculateFishnetStatistics(parameter.LenAzi);
                    }
                }
            }
        }

        public override async Task<FlapParameters> PrepareInputForProcessing(FeatureLayer InputLayer,
            Envelope customEnvelope, int subCellsize, int nInterval, bool selectedFeatures, RoseGeom roseGeom,
            bool bRegional, string fieldName)
        {
            if (!bRegional)
            {
                if (!selectedFeatures)
                    return await VectorFunctions.PrepareInputForCellFishnet(InputLayer, customEnvelope, subCellsize,
                        nInterval, 0, 0, roseGeom, fieldName);
                else
                    return await VectorFunctions.PrepareInputForCellFishnet(InputLayer, customEnvelope, subCellsize,
                    nInterval, 0, 0, roseGeom, fieldName, true);
            }
            else
                return await VectorFunctions.PrepareInputForRegionalFishnet(InputLayer, customEnvelope,
                    nInterval, roseGeom, fieldName);

        }

        public override async Task<string> SaveAsVectorFeatures(FeatureClass OuputFeatureClass, SpatialReference mySpatialRef, FlapParameters _parameters, bool bExtentOnly)
        {
            return await VectorFunctions.SaveFishnetVectorFeatures(OuputFeatureClass, mySpatialRef, _parameters, bExtentOnly);
        }

        public override async Task<FeatureClass> CreateFeatureClass(string suffix, string fishnetName, bool bExists,
            string databasePath, FeatureLayer InputLayer, bool bStatistics, RoseGeom roseGeom, SpatialReference thisSpatRef)
        {
            return await VectorFunctions.CreateRoseNetFeatClass(suffix, fishnetName, bExists, databasePath, InputLayer,
             bStatistics, roseGeom, true, thisSpatRef);
        }

    }

    public class RoseRegionalPlot : TypeOfAnalysis
    {
        public override void CalculateVectorValues(FlapParameters _parameters, bool Statistics, RoseType roseType, bool bDirection)
        {
            int NoOfElements = 180 / _parameters.Interval;

            FlapParameter parameter = _parameters.flapParameters[0];

            if (parameter.CreateCell)
            {
                parameter.Rose = new RoseDiagramParameters();

                parameter.Rose.RoseArrayValues(bDirection, NoOfElements, _parameters.Interval, parameter.LenAzi, roseType);

                parameter.Rose.CalculateRosePetals(_parameters.Interval, parameter.ExtentWidth, parameter.ExtentHeight);

                if (Statistics)
                    parameter.Rose.CalculateRoseStatistics(parameter.LenAzi, _parameters.Interval);
            }
        }

        public override async Task<FlapParameters> PrepareInputForProcessing(FeatureLayer InputLayer,
            Envelope customEnvelope, int subCellsize, int nInterval, bool selectedFeatures, RoseGeom roseGeom,
            bool bRegional, string fieldName)
        {
            if (selectedFeatures)
                return await VectorFunctions.PrepareInputForRegionalProcessing(InputLayer, customEnvelope,
                                nInterval, roseGeom, fieldName, selectedFeatures);
            else
                return await VectorFunctions.PrepareInputForRegionalProcessing(InputLayer, customEnvelope,
                    nInterval, roseGeom, fieldName);
        }

        public override async Task<string> SaveAsVectorFeatures(FeatureClass OuputFeatureClass,
            SpatialReference mySpatialRef, FlapParameters _parameters, bool bExtentOnly)
        {
            return await VectorFunctions.SaveRoseToVectorFeatures(OuputFeatureClass, mySpatialRef, _parameters);
        }


        public override async Task<FeatureClass> CreateFeatureClass(string suffix, string roseName, bool bExists,
            string databasePath, FeatureLayer InputLayer, bool bStatistics, RoseGeom roseGeom, SpatialReference thisSpatRef)
        {
            return await VectorFunctions.CreateRoseNetFeatClass(suffix, roseName, bExists, databasePath, InputLayer,
             bStatistics, roseGeom, false, thisSpatRef);
        }
    }
}
