﻿using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveFunctionCollapse
{
    public class WaveFunctionCollapseRunner
    {
        Wave wave;
        private static Random random = new Random();

        int iterations = 200;
        int averageCollapseStep = 0;

        public WaveCollapseHistory Run(List<Pattern> patterns, int N, List<Point3d> wavePoints, float[] weights, bool backtrack)
        {
            WaveCollapseHistory timelapse = new WaveCollapseHistory();

            int width = GetNumberofPointsInOneDimension(wavePoints[0].X, wavePoints[wavePoints.Count - 1].X);
            int height = GetNumberofPointsInOneDimension(wavePoints[0].Y, wavePoints[wavePoints.Count - 1].Y);

            var counter = 0;
            var sumOfCollapsedSteps = 0;
            //var averageCollapseStep = 0; 
            
            while (counter < iterations)
            {
                counter++;
                int steps = 0;

                wave = new Wave(width, height, patterns, N);

                // Start with random seed
                SeedRandom(width, height, patterns);
                AddCurrentFrameToHistory(timelapse);

                // Break if contracition, otherwise run observations until it is not completaly observed
                while (!wave.IsCollapsed())
                {
                    if (wave.Contradiction()) { break; }

                    // Backtracking: repeat the step if contradiction
                    var observed = wave.Observe();

                    if (backtrack)
                    {
                        var canPropagate = wave.CheckIfPropagateWithoutContradiction(observed.Item1, observed.Item2, observed.Item3);
                        int repeatedObservations = 0;

                        while (!canPropagate)
                        {
                            observed = wave.Observe();
                            canPropagate = wave.CheckIfPropagateWithoutContradiction(observed.Item1, observed.Item2, observed.Item3);
                            repeatedObservations++;

                            if (repeatedObservations > (int)(patterns.Count / 4))
                            {
                                break;
                            }
                        }

                        wave.PropagateByUpdatingSuperposition(observed.Item1, observed.Item2, observed.Item3);
                    }
                    else
                    {
                        // WORKING VERSION WITHOUT BACKTRACKING:
                        observed = wave.Observe();
                        try
                        {
                            wave.PropagateByUpdatingSuperposition(observed.Item1, observed.Item2, observed.Item3);
                        }
                        catch (DataMisalignedException ex)
                        {
                            break;
                        }
                    }

                    AddCurrentFrameToHistory(timelapse);
                    steps++;
                }

                if (wave.Contradiction())
                {
                    double percentage = steps / (width * height * 1.0);
                    System.Diagnostics.Debug.WriteLine("Contradiction on " + steps + " th step of " + width * height + 
                        ", which is " + percentage + " of the whole wave");

                    sumOfCollapsedSteps += steps;

                    timelapse.Clear();
                    continue;
                }
                else
                {
                    averageCollapseStep = sumOfCollapsedSteps / counter;
                    System.Diagnostics.Debug.WriteLine("Average collapse step is: " + averageCollapseStep + " for " + width*height + " steps");
                    break;
                }
            }

            averageCollapseStep = sumOfCollapsedSteps / counter;
            System.Diagnostics.Debug.WriteLine("Average collapse step is: " + averageCollapseStep + " for " + width * height + " steps");
            return timelapse;
        }


        public void SeedRandom(int width, int height, List<Pattern> patterns)
        {
            var x = random.Next(width);
            var y = random.Next(height);

            var seedPattern = GetSeedPattern(x, y, patterns);
            wave.PropagateByUpdatingSuperposition(x, y, seedPattern);
        }

        public int GetAverageCollapseStep()
        {
            return averageCollapseStep;
        }

        public int[] GetPatternCounts()
        {
            return wave.GetCollapsedPatternsCounts();
        }

        // For time visualisation - each step is added to the timelapse
        void AddCurrentFrameToHistory(WaveCollapseHistory timelapse)
        {
            var timeFrameToPoints = OutputObservations();
            var timeFrameUncollapsed =  wave.GetPossibleTileTypes();
            var patternOccurence = wave.GetCollapsedPatternsCounts();

            var timeFrameElement = new WaveCollapseHistoryElement(timeFrameToPoints.Item1, 
                timeFrameToPoints.Item2, timeFrameToPoints.Item3, wave.superpositions, timeFrameUncollapsed, patternOccurence);
            timelapse.AddTimeFrame(timeFrameElement);
        }

        // Get 3d points of new tiles 
        public Tuple<List<Point3d>, List<Point3d>, List<Point3d>> OutputObservations()
        {
            var k = wave.Visualise();
            return Tuple.Create(k.Item1, k.Item2, k.Item3);
        }

        // Find width and height of surface
        public int GetNumberofPointsInOneDimension(double firstPointCoordinate, double secondPointCoordinate)
        {
            return Math.Abs((int)(0.5 * (firstPointCoordinate - secondPointCoordinate) - 1));
        }

        // Return first pattern that will be a seed for surface
        public Pattern GetSeedPattern(int x, int y, List<Pattern> patternsFromSample)
        {
            var patternToSeed = highestWeightPattern(patternsFromSample);

            var index = FindPatternIndex(patternToSeed, patternsFromSample);

            var outputSeed = patternToSeed.Instantiate(x, y, 0);

            return patternToSeed;
        }

        // Get pattern with highest weight, if there is few of them - pick randomly
        public Pattern highestWeightPattern(List<Pattern> patterns)
        {
            List<Pattern> patternsWithHeighestWeight = new List<Pattern>();
            float highestWeight = 0;

            for (int i = 0; i < patterns.Count; i++)
            {
                if (patterns[i].Weight > highestWeight) highestWeight = patterns[i].Weight;
            }

            for (int i = 0; i < patterns.Count; i++)
            {
                if (patterns[i].Weight == highestWeight) patternsWithHeighestWeight.Add(patterns[i]);
            }

            if (patternsWithHeighestWeight.Count > 1)
            {
                int randomNumber = random.Next(patternsWithHeighestWeight.Count - 1);

                return patternsWithHeighestWeight[randomNumber];
            }
            else
            {
                return patternsWithHeighestWeight[0];
            }
        }

        // Check which index has provided pattern in a pattern library
        int FindPatternIndex(Pattern patternToFind, List<Pattern> listOfPatterns)
        {
            int index = 0;
            for (int i = 0; i < listOfPatterns.Count; i++)
            {
                if (Pattern.ReferenceEquals(patternToFind, listOfPatterns[i])) index = i;
            }

            return index;
        }
     
    }
}
