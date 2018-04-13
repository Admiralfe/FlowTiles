﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using Script.FlowTileUtils;
using Script.LPModel;
using UnityEngine;
using UnityEngine.WSA;
using Random = System.Random;

namespace Script.GridBuilding
{
    enum Corners
    {
        TopLeft,
        TopRight,
        BottomRight,
        BottomLeft
    }
    
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

        private Vector2[] velocityRestrictions(int rowNumber, int colNumber)
        {
            Vector2 allowedTopLeftVelocity;
            Vector2 allowedBottomLeftVelocity;
            Vector2 allowedTopRightVelocity;
            Vector2 allowedBottomRightVelocity;
            
            bool topLeftRestricted = false;
            bool bottomLeftRestricted = false;
            bool topRightRestricted = false;
            bool bottomRightRestricted = false;
            
            //Left tile
            if(tileGrid.HasTile(rowNumber - 1, colNumber))
            {
                allowedTopLeftVelocity = tileGrid.GetFlowTile(rowNumber - 1, colNumber).CornerVelocities.TopRight;
                allowedBottomLeftVelocity =
                    tileGrid.GetFlowTile(rowNumber - 1, colNumber).CornerVelocities.BottomRight; 
                
                topLeftRestricted = true;
                bottomLeftRestricted = true;
            }
            
            //Top left tile
            if (tileGrid.HasTile(rowNumber - 1, colNumber + 1))
            {
                
                //Checks if velocity restriction has been set already, in that case we don't need to set it again, 
                //since it will be the same in a valid tiling.
                if (!topLeftRestricted)
                {
                    allowedTopLeftVelocity =
                        tileGrid.GetFlowTile(rowNumber - 1, colNumber + 1).CornerVelocities.BottomRight;
                    topLeftRestricted = true;
                }
            }
            
            //Bottom left tile
            if (tileGrid.HasTile(rowNumber - 1, colNumber - 1))
            {
                if (!bottomLeftRestricted)
                {
                    allowedBottomLeftVelocity =
                        tileGrid.GetFlowTile(rowNumber - 1, colNumber + 1).CornerVelocities.TopRight;
                    bottomLeftRestricted = true;
                }
            }
            
            //Top tile
            if (tileGrid.HasTile(rowNumber, colNumber + 1))
            {
                if (!topLeftRestricted)
                {
                    allowedTopLeftVelocity = 
                        tileGrid.GetFlowTile(rowNumber, colNumber + 1).CornerVelocities.BottomLeft;
                    topLeftRestricted = true;
                }

                if (!topRightRestricted)
                {
                    allowedTopRightVelocity = 
                        tileGrid.GetFlowTile(rowNumber, colNumber + 1).CornerVelocities.BottomLeft;
                    topRightRestricted = true;
                }
            }
            
            //Bottom Tile
            if (tileGrid.HasTile(rowNumber, colNumber - 1))
            {
                if (!bottomLeftRestricted)
                {
                    allowedBottomLeftVelocity = 
                        tileGrid.GetFlowTile(rowNumber, colNumber - 1).CornerVelocities.TopLeft;
                    bottomLeftRestricted = true;
                }

                if (!bottomRightRestricted)
                {
                    allowedBottomRightVelocity =
                        tileGrid.GetFlowTile(rowNumber, colNumber - 1).CornerVelocities.TopRight;
                    bottomRightRestricted = true;
                }
            }
            
            //Left Tile
            if (tileGrid.HasTile(rowNumber + 1, colNumber))
            {
                
            }
        }

        //Finds valid tiles in position rowNumber, colNumber and returns a list
        private List<FlowTile> ValidTiles(int rowNumber, int colNumber)
        {
            int[] validTopFluxRange = new int[2];
            int[] validBottomFluxRange = new int[2];
            int[] validLeftFluxRange = new int[2];
            int[] validRightFluxRange = new int[2];
                        
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
                            Flux flux = new Flux();
                            flux.TopEdge = i;
                            flux.RightEdge = j;
                            flux.BottomEdge = k;
                            flux.LeftEdge = l;

                            currentValidTiles.Add(new FlowTile(innerTileGridDimension, flux,
                                new CornerVelocities
                                {
                                    TopLeft = Vector2.zero,
                                    BottomLeft = Vector2.zero,
                                    TopRight = Vector2.zero,
                                    BottomRight = Vector2.zero,
                                }));
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