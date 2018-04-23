using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using Script.FlowTileUtils;
using Script.LPModel;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.WSA;
using Random = System.Random;

namespace Script.GridBuilding
{
    public class GridBuilder
    {

        private int minXFlux;
        private int maxXFlux;
        private int minYFlux;
        private int maxYFlux;
        private List<Vector2> allowedVelocities = new List<Vector2>();

        private TileGrid tileGrid;

        public int gridDimension;

        public int innerTileGridDimension;

        public GridBuilder(int minXFluxIn, int maxXFluxIn, int minYFluxIn, int maxYFluxIn, int gridDimensionIn,
            int innerTileGridDimensionIn, Vector2[] allowedVelocitiesIn)
        {
            minXFlux = minXFluxIn;
            maxXFlux = maxXFluxIn;
            minYFlux = minYFluxIn;
            maxYFlux = maxYFluxIn;
            gridDimension = gridDimensionIn;

            innerTileGridDimension = innerTileGridDimensionIn;
            
            foreach (Vector2 cornerVelocity in allowedVelocitiesIn)
            {
                allowedVelocities.Add(cornerVelocity);
            }

            tileGrid = new TileGrid(gridDimension);
            
            //LPSolve.BuildInitialModel(minXFlux, maxXFlux, minYFlux, maxYFlux, tileGrid);
        }
        
        public GridBuilder(int minXFluxIn, int maxXFluxIn, int minYFluxIn, int maxYFluxIn, int gridDimensionIn,
            int innerTileGridDimensionIn)
        {
            minXFlux = minXFluxIn;
            maxXFlux = maxXFluxIn;
            minYFlux = minYFluxIn;
            maxYFlux = maxYFluxIn;
            gridDimension = gridDimensionIn;

            innerTileGridDimension = innerTileGridDimensionIn;
            
            allowedVelocities.Add(Vector2.zero);
            
            tileGrid = new TileGrid(gridDimension);
            
            //LPSolve.BuildInitialModel(minXFlux, maxXFlux, minYFlux, maxYFlux, tileGrid);
        }

        public void AddTile(int rowIndex, int colIndex, FlowTile flowTile)
        {
            tileGrid.AddTile(rowIndex, colIndex, flowTile);
        }

        public TileGrid GetTileGrid()
        {
            return tileGrid;
        }

        private void WriteTilesToXML(List<FlowTile> tiles, string filename)
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlElement root = xmlDoc.CreateElement("root");
            foreach (var tile in tiles)
            {
                root.AppendChild(tile.ToXmlElement(xmlDoc));
            }
            xmlDoc.Save(filename);
        }

        public FlowTile AskUserForTile(int row, int col)
        {
            LPSolve.BuildInitialModel(minXFlux, maxXFlux, minYFlux, maxYFlux, tileGrid);
            List<FlowTile> validTiles = ValidTiles(row, col);    
            
            string pathToScript = "/home/felix/FlowTiles/ui.py";
            string path = Directory.GetCurrentDirectory();
            string gridPath = path + "grid.xml";
            UnityEngine.Debug.Log(gridPath);
            string validTilesPath = path + "validtiles.xml";
            tileGrid.WriteToXML(gridPath);    
            WriteTilesToXML(validTiles, validTilesPath);
            
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = "/usr/bin/python3";
            start.Arguments = string.Format("{0} {1} {2} {3} {4} {5}",
                pathToScript, gridDimension, row, col, gridPath, validTilesPath);
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            
            
            using (Process process = Process.Start(start))
            {
                using (StreamReader reader = process.StandardOutput)
                {
                    string result = reader.ReadToEnd();
                    //Console.WriteLine(result);
                }
            }
            

            //Process python = Process.Start(start);
            //python.WaitForExit();
            Console.WriteLine("Which tile do you want at row {0}, column {1}. Type a number:", row, col);
            var num = Convert.ToInt32(Console.ReadLine());
            
            UnityEngine.Debug.Log(num);
            
            //python.Close();
            LPSolve.FreeModel();

            return validTiles[num];
            }
        
        public TileGrid BuildRandomTileGrid()
        {
            //Clear the tilegrid.
            tileGrid = new TileGrid(gridDimension);
            
            Random RNG = new Random();
            for (int row = 0; row < gridDimension; row++)
            {
                for (int col = 0; col < gridDimension; col++)
                {
                    LPSolve.BuildInitialModel(minXFlux, maxXFlux, minYFlux, maxYFlux, tileGrid);
                    List<FlowTile> validTiles = ValidTiles(row, col);
                    FlowTile newTile = validTiles[RNG.Next(0, validTiles.Count - 1)];
                    /*
                    Console.WriteLine("(" + row + ", " + col + ")");
                    Console.WriteLine("Top: " + newTile.Flux.topEdge);
                    Console.WriteLine("Right: " + newTile.Flux.rightEdge);
                    Console.WriteLine("Bottom: " + newTile.Flux.bottomEdge);
                    Console.WriteLine("Left: " + newTile.Flux.leftEdge);
                    */
                    tileGrid.AddTile(row, col, newTile);

                    LPSolve.FreeModel();
                }
            }

            return tileGrid;
        }

        private Vector2?[] velocityRestrictions(int rowNumber, int colNumber)
        {
            Vector2? allowedTopLeftVelocity = null;
            Vector2? allowedBottomLeftVelocity = null;
            Vector2? allowedTopRightVelocity = null;
            Vector2? allowedBottomRightVelocity = null;
            
            bool topLeftRestricted = false;
            bool bottomLeftRestricted = false;
            bool topRightRestricted = false;
            bool bottomRightRestricted = false;
            
            //Left tile
            if(tileGrid.HasTile(rowNumber, colNumber - 1))
            {
                allowedTopLeftVelocity = tileGrid.GetFlowTile(rowNumber, colNumber - 1).CornerVelocities.TopRight;
                allowedBottomLeftVelocity =
                    tileGrid.GetFlowTile(rowNumber, colNumber - 1).CornerVelocities.BottomRight; 
                
                topLeftRestricted = true;
                bottomLeftRestricted = true;
            }
            
            //Top left tile
            if (tileGrid.HasTile(rowNumber + 1, colNumber - 1))
            {
                
                //Checks if velocity restriction has been set already, in that case we don't need to set it again, 
                //since it will be the same in a valid tiling.
                if (!topLeftRestricted)
                {
                    allowedTopLeftVelocity =
                        tileGrid.GetFlowTile(rowNumber + 1, colNumber - 1).CornerVelocities.BottomRight;
                    topLeftRestricted = true;
                }
            }
            
            //Bottom left tile
            if (tileGrid.HasTile(rowNumber - 1, colNumber - 1))
            {
                if (!bottomLeftRestricted)
                {
                    allowedBottomLeftVelocity =
                        tileGrid.GetFlowTile(rowNumber - 1, colNumber - 1).CornerVelocities.TopRight;
                    bottomLeftRestricted = true;
                }
            }
            
            //Top tile
            if (tileGrid.HasTile(rowNumber + 1, colNumber))
            {
                if (!topLeftRestricted)
                {
                    allowedTopLeftVelocity = 
                        tileGrid.GetFlowTile(rowNumber + 1, colNumber).CornerVelocities.BottomLeft;
                    topLeftRestricted = true;
                }

                if (!topRightRestricted)
                {
                    allowedTopRightVelocity = 
                        tileGrid.GetFlowTile(rowNumber + 1, colNumber).CornerVelocities.BottomLeft;
                    topRightRestricted = true;
                }
            }
            
            //Bottom Tile
            if (tileGrid.HasTile(rowNumber - 1, colNumber))
            {
                if (!bottomLeftRestricted)
                {
                    allowedBottomLeftVelocity = 
                        tileGrid.GetFlowTile(rowNumber - 1, colNumber).CornerVelocities.TopLeft;
                    bottomLeftRestricted = true;
                }

                if (!bottomRightRestricted)
                {
                    allowedBottomRightVelocity =
                        tileGrid.GetFlowTile(rowNumber - 1, colNumber).CornerVelocities.TopRight;
                    bottomRightRestricted = true;
                }
            }
            
            //Right Tile
            if (tileGrid.HasTile(rowNumber + 1, colNumber))
            {
                if (!bottomRightRestricted)
                {
                    allowedBottomLeftVelocity =
                        tileGrid.GetFlowTile(rowNumber + 1, colNumber).CornerVelocities.BottomLeft;
                    bottomRightRestricted = true;
                }

                if (!topRightRestricted)
                {
                    allowedTopRightVelocity =
                        tileGrid.GetFlowTile(rowNumber + 1, colNumber).CornerVelocities.BottomLeft;
                    topRightRestricted = true;
                }
            }
            
            //Top right tile
            if (tileGrid.HasTile(rowNumber + 1, colNumber + 1))
            {
                if (!topRightRestricted)
                {
                    allowedTopRightVelocity = 
                        tileGrid.GetFlowTile(rowNumber + 1, colNumber + 1).CornerVelocities.TopRight;
                }
            }
            
            //Bottom right tile
            if (tileGrid.HasTile(rowNumber + 1, colNumber - 1))
            {
                if (!bottomRightRestricted)
                {
                    allowedBottomRightVelocity =
                        tileGrid.GetFlowTile(rowNumber + 1, colNumber - 1).CornerVelocities.BottomRight;
                }
            }

            return new Vector2?[]
            {
                allowedTopLeftVelocity, allowedTopRightVelocity,
                allowedBottomRightVelocity, allowedBottomLeftVelocity
            };
        }

        private List<CornerVelocities> cornerVelocityCombinations(Vector2?[] restrictions)
        {
            List<Vector2>[] iteratorVectorList = new List<Vector2>[4];
            
            for (int i = 0; i < 4; i++)
            {
                iteratorVectorList[i] = new List<Vector2>();
                if (!restrictions[i].HasValue)
                {
                    foreach (Vector2 cornerVelocity in allowedVelocities)
                    {
                        iteratorVectorList[i].Add(cornerVelocity);
                    }
                }

                else
                {
                    iteratorVectorList[i].Add(restrictions[i].Value);
                }
            }

            List<CornerVelocities> combinationList = new List<CornerVelocities>();

            foreach (Vector2 v1 in iteratorVectorList[(int) Corner.TopLeft])
            {
                foreach (Vector2 v2 in iteratorVectorList[(int) Corner.TopRight])
                {
                    foreach (Vector2 v3 in iteratorVectorList[(int) Corner.BottomLeft])
                    {
                        foreach (Vector2 v4 in iteratorVectorList[(int) Corner.BottomRight])
                        {
                            combinationList.Add(new CornerVelocities(v1, v2, v3, v4));
                        }
                    }
                }
            }

            return combinationList;
        }
        
        /// <summary>
        /// Finds the valid flow tiles to be put in a position given by row and column indices.
        /// The constraints are that edge fluxes should match and that the field needs to be divergence free.
        /// </summary>
        /// <param name="rowNumber">Row index in grid</param>
        /// <param name="colNumber">Column index in grid</param>
        /// <returns>List of valid tiles to put in that position</returns>
        /// <exception cref="ArgumentException">
        /// If there already is a tile in the given position this exception is thrown.
        /// </exception>
        private List<FlowTile> ValidTiles(int rowNumber, int colNumber)
        {
            if (tileGrid.HasTile(rowNumber, colNumber))
            {
                throw new ArgumentException("Tile already exists on that position");
            }
            
            int[] validTopFluxRange = new int[2];
            int[] validBottomFluxRange = new int[2];
            int[] validLeftFluxRange = new int[2];
            int[] validRightFluxRange = new int[2];
            
            //Finds the restrictions on corner velocities
            Vector2?[] restrictions = velocityRestrictions(rowNumber, colNumber);
            //Finds all valid combinations of the allowed corner velocities.
            List<CornerVelocities> allowedCornerVelocities = cornerVelocityCombinations(restrictions);
                        
            if (rowNumber == 0)
            {
                validTopFluxRange[0] = 0;
                validTopFluxRange[1] = 0;
            }
            else
            {
                LPSolve.SetEdgeToSolve(rowNumber, colNumber, LPSolve.Direction.Top, gridDimension, false);
                validTopFluxRange[0] = LPSolve.SolveModel();
                LPSolve.SetEdgeToSolve(rowNumber, colNumber, LPSolve.Direction.Top, gridDimension, true);
                validTopFluxRange[1] = LPSolve.SolveModel();
            }

            if (rowNumber == gridDimension - 1)
            {
                validBottomFluxRange[0] = 0;
                validBottomFluxRange[1] = 0;
            }

            else
            {
                LPSolve.SetEdgeToSolve(rowNumber, colNumber, LPSolve.Direction.Bottom, gridDimension, false);
                validBottomFluxRange[0] = LPSolve.SolveModel();
                LPSolve.SetEdgeToSolve(rowNumber, colNumber, LPSolve.Direction.Bottom, gridDimension, true);
                validBottomFluxRange[1] = LPSolve.SolveModel();
            }

            if (colNumber == 0)
            {
                validLeftFluxRange[0] = 0;
                validLeftFluxRange[1] = 0;
            }

            else
            {
                LPSolve.SetEdgeToSolve(rowNumber, colNumber, LPSolve.Direction.Left, gridDimension, false);
                validLeftFluxRange[0] = LPSolve.SolveModel();
                LPSolve.SetEdgeToSolve(rowNumber, colNumber, LPSolve.Direction.Left, gridDimension, true);
                validLeftFluxRange[1] = LPSolve.SolveModel();
            }

            if (colNumber == gridDimension - 1)
            {
                validRightFluxRange[0] = 0;
                validRightFluxRange[1] = 0;
            }

            else
            {
                LPSolve.SetEdgeToSolve(rowNumber, colNumber, LPSolve.Direction.Right, gridDimension, false);
                validRightFluxRange[0] = LPSolve.SolveModel();
                LPSolve.SetEdgeToSolve(rowNumber, colNumber, LPSolve.Direction.Right, gridDimension, true);
                validRightFluxRange[1] = LPSolve.SolveModel();
            }

            List<FlowTile> currentValidTiles = new List<FlowTile>();

            //Create all possible FlowTiles given the bounds on flows. This set still needs to be filtered
            for (int i = validTopFluxRange[0]; i <= validTopFluxRange[1]; i++)
            {
                for (int j = validRightFluxRange[0]; j <= validRightFluxRange[1]; j++)
                {
                    for (int k = validBottomFluxRange[0]; k <= validBottomFluxRange[1]; k++)
                    {
                        for (int l = validLeftFluxRange[0]; l <= validLeftFluxRange[1]; l++)
                        {
                            foreach (CornerVelocities cornerVelocities in allowedCornerVelocities)
                            {

                                Flux flux = new Flux();
                                flux.TopEdge = i;
                                flux.RightEdge = j;
                                flux.BottomEdge = k;
                                flux.LeftEdge = l;

                                currentValidTiles.Add(new FlowTile(innerTileGridDimension, flux, cornerVelocities));
                            }
                        }
                    }
                }
            }
            
            /*
            foreach (FlowTile tile in currentValidTiles)
            {
                Console.WriteLine("Top: " + tile.Flux.topEdge);
                Console.WriteLine("Right: " + tile.Flux.rightEdge);
                Console.WriteLine("Bottom: " + tile.Flux.bottomEdge);
                Console.WriteLine("Left: " + tile.Flux.leftEdge + "\n");
            }
            */

            //WriteLine("number of tiles: " + currentValidTiles.Count);

            List<FlowTile> validTiles =
                LPSolve.FilterValidTiles(currentValidTiles, rowNumber, colNumber, gridDimension);

            //Console.WriteLine("final number of tiles: " + newValidTiles.Count);
            
            return validTiles;
        }
        
    }

}