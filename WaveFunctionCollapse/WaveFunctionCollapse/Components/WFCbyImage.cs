﻿using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace WaveFunctionCollapse
{
    public class WFCbyImage : GH_Component
    {
        public WFCbyImage()
          : base("WFCbyImage", "Nickname", "Description",
              "WFC", "Data Analysis")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            // Wave function collapse inputs. 
            pManager.AddParameter(new PatternFromSampleParam());
            pManager.AddPointParameter("Wave", "", "", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Backtrack", "", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Iterations", "", "", GH_ParamAccess.item);

            // Image. 
            pManager.AddParameter(new InputImageParam());

            // New weights
            pManager.AddNumberParameter("Weights", "", "", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Apply weights", "", "", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new PatternHistoryParam());
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Get wave function collapse data
            GH_PatternsFromSample gh_patterns = new GH_PatternsFromSample();
            DA.GetData<GH_PatternsFromSample>(0, ref gh_patterns);

            List<Point3d> wavePoints = new List<Point3d>();
            DA.GetDataList<Point3d>(1, wavePoints);

            bool backtrack = false;
            DA.GetData<bool>(2, ref backtrack);

            double iterations = 0;
            DA.GetData<double>(3, ref iterations);

            // Get image data.
            GH_Image inputImage = new GH_Image();
            DA.GetData(4, ref inputImage);

            List<double> newWeights = new List<double>();
            DA.GetDataList(5, newWeights);

            bool applyWeights = false;
            DA.GetData(6, ref applyWeights);
            
            // Extract parameters to run Wave Function Collapse.
            var patterns = gh_patterns.Value.Patterns;
            var weights = gh_patterns.Value.TilesWeights;
            var N = gh_patterns.Value.N;

            int width = Utils.GetNumberofPointsInOneDimension(wavePoints[0].X, wavePoints[wavePoints.Count - 1].X);
            int height = Utils.GetNumberofPointsInOneDimension(wavePoints[0].Y, wavePoints[wavePoints.Count - 1].Y);

            // Prepare image data. 
            //var image = convertImageListToArray(rawImage, width, height);
            //var image = Utils.generateRandomImage(width, height);
            var image = inputImage.Value.Brightness;

            // Run Wave Function Collapse.
            var wfc = new WaveFunctionCollapseRunner();
            var history = new WaveCollapseHistory();

            if (applyWeights)
            {
                history = wfc.Run(patterns, N, width, height, weights, (int)iterations, backtrack, image, newWeights);

            }
            else
            {
                history = wfc.Run(patterns, N, width, height, weights, (int)iterations, backtrack, image);

            }
            var return_value = new GH_WaveCollapseHistory(history);

            DA.SetData(0, return_value);
        }




        double[,] convertImageListToArray(List<double> image, int width, int height)
        {
            double[,] convertedImage = new double[width, height];

            for (int i = 0; i < width; i ++)
            {  
                for (int j = 0; j < height; j ++)
                {
                    convertedImage[i, j] = image[i*j + j];
                }
            }

            return convertedImage;
        }


        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("d0778493-3bba-401d-baae-edb26b684c1b"); }
        }
    }
}