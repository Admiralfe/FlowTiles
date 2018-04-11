using System.Collections.Generic;
using System.Linq;
using System.Text;
using Script.FlowTileUtils;
using Script.LPModel;
using UnityEngine;
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

        private TileGrid tileGrid;

        public int gridDimension;

        public int innerTileGridDimension;

        public GridBuilder(int minXFluxIn, int maxXFluxIn, int minYFluxIn, int maxYFluxIn, int gridDimensionIn)
        {
            minXFlux = minXFluxIn;
            maxXFlux = maxXFluxIn;
            minYFlux = minYFluxIn;
            maxYFlux = maxYFluxIn;
            gridDimension = gridDimensionIn;

            tileGrid = new TileGrid(gridDimension);
            
            LPSolve.BuildInitialModel(minXFlux, maxXFlux, minYFlux, maxYFlux, tileGrid);
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
                    //LPSolve.BuildInitialModel(minXFlux, maxXFlux, minYFlux, maxYFlux, tileGrid);
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
                            flux.topEdge = i;
                            flux.rightEdge = j;
                            flux.bottomEdge = k;
                            flux.leftEdge = l;

                            currentValidTiles.Add(new FlowTile(innerTileGridDimension, flux,
                                new CornerVelocities
                                {
                                    topLeft = Vector2.zero,
                                    bottomLeft = Vector2.zero,
                                    topRight = Vector2.zero,
                                    bottomRight = Vector2.zero,
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

            List<FlowTile> newValidTiles =
                LPSolve.FilterValidTiles(currentValidTiles, rowNumber, colNumber, gridDimension);

            //Console.WriteLine("final number of tiles: " + newValidTiles.Count);
            
            return newValidTiles;
        }
    }

}