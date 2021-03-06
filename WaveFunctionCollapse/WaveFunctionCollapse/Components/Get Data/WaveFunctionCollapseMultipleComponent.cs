﻿using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace WaveFunctionCollapse
{
    public class MyComponent3 : GH_Component
    {
        public MyComponent3()
          : base("WFCx", "WFCx", "Run WFC multiple times and collects data", "WFC", "Data Analysis")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new PatternFromSampleParam());
            pManager.AddPointParameter("Wave", "", "", GH_ParamAccess.list);
            pManager.AddNumberParameter("Dataset Size", "", "", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Backtrack", "", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Iterations", "", "", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new PatternResultsParam());
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_PatternsFromSample gh_patterns = new GH_PatternsFromSample();
            DA.GetData<GH_PatternsFromSample>(0, ref gh_patterns);

            List<Point3d> wavePoints = new List<Point3d>();
            DA.GetDataList<Point3d>(1, wavePoints);

            double dataSize = 0;
            DA.GetData<double>(2, ref dataSize);

            bool backtrack = false;
            DA.GetData<bool>(3, ref backtrack);

            double iterations = 0;
            DA.GetData<double>(4, ref iterations);

            var patterns = gh_patterns.Value.Patterns;
            var weights = gh_patterns.Value.TilesWeights;
            var N = gh_patterns.Value.N;

            // Get width and height based on 2d array of points
            int width = Utils.GetNumberofPointsInOneDimension(wavePoints[0].X, wavePoints[wavePoints.Count - 1].X);
            int height = Utils.GetNumberofPointsInOneDimension(wavePoints[0].Y, wavePoints[wavePoints.Count - 1].Y);

            List<WaveCollapseHistoryElement> outputs = new List<WaveCollapseHistoryElement>();

            var return_value = new GH_WaveCollapseResults();

            for (int i = 0; i < (int)dataSize; i++)
            {
                // RUN WAVEFUNCION COLLAPSE
                var wfc = new WaveFunctionCollapseRunner();
                var history = wfc.Run(patterns, N, width, height, weights, backtrack, (int)iterations);

                if (history.Elements.Count == 0) continue;

                var historyEndElement = history.Elements[history.Elements.Count - 1];

                outputs.Add(historyEndElement);
                return_value.Value.AddToList(historyEndElement);
            }

            if (true)
            {
                DA.SetData(0, return_value);
            }
            else
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Check the inputs you idiot!");
            }
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
            get { return new Guid("5dbb0585-4297-4dbe-9b11-76d7c75ba5ac"); }
        }
    }
}