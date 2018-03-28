﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FlowTilesUtils;

public class GridBuilder
{

    private int minXFlux;
    private int maxXFlux;
    private int minYFlux;
    private int maxYFlux;

    public int gridDimension;

    public int innerTileGridDimension;

    public GridBuilder(int minXFluxIn, int maxXFluxIn, int minYFluxIn, int maxYFluxIn, int gridDimensionIn)
    {
        minXFlux = minXFluxIn;
        maxXFlux = maxXFluxIn;
        minYFlux = minYFluxIn;
        maxYFlux = maxYFluxIn;
        gridDimension = gridDimensionIn;

        //LPSolve.BuildInitialModel(minXFlux, maxXFlux, minYFlux, maxYFlux);
    }

    public TileGrid BuildRandomTileGrid()
    {
        Random RNG = new Random();
        TileGrid currentTileGrid = new TileGrid(gridDimension);
        for (int row = 0; row < gridDimension; row++)
        {
            for (int col = 0; col < gridDimension; col++)
            {
                LPSolve.BuildInitialModel(-1, 1, -1, 1, currentTileGrid);
                List<FlowTile> validTiles = ValidTiles(row, col);
                currentTileGrid.AddTile(row, col, validTiles[RNG.Next(0, validTiles.Count - 1)]);
                LPSolve.FreeModel();
            }
        }

        return currentTileGrid;
    }

    //Finds valid tiles in position rowNumber, colNumber and returns a list
    private List<FlowTile> ValidTiles(int rowNumber, int colNumber)
    {
        int[] validTopFluxRange = new int[2];
        int[] validBottomFluxRange = new int[2];
        int[] validLeftFluxRange = new int[2];
        int[] validRightFluxRange = new int[2];

        int sourceCell = rowNumber * gridDimension + colNumber;

        if (rowNumber == 0)
        {
            validTopFluxRange[0] = 0;
            validTopFluxRange[1] = 0;
        }
        else
        {
            LPSolve.SetEdgeToSolve(sourceCell, sourceCell - gridDimension, gridDimension, false, false);
            validTopFluxRange[0] = LPSolve.SolveModel();
            LPSolve.SetEdgeToSolve(sourceCell, sourceCell - gridDimension, gridDimension, false, true);
            validTopFluxRange[1] = LPSolve.SolveModel();
        }

        if (rowNumber == gridDimension - 1)
        {
            validBottomFluxRange[0] = 0;
            validBottomFluxRange[1] = 0;
        }

        else
        {
            LPSolve.SetEdgeToSolve(sourceCell, sourceCell + gridDimension, gridDimension, false, false);
            validBottomFluxRange[0] = LPSolve.SolveModel();
            LPSolve.SetEdgeToSolve(sourceCell, sourceCell + gridDimension, gridDimension, false, true);
            validBottomFluxRange[1] = LPSolve.SolveModel();
        }

        if (colNumber == 0)
        {
            validLeftFluxRange[0] = 0;
            validLeftFluxRange[1] = 0;
        }

        else
        {
            LPSolve.SetEdgeToSolve(sourceCell, sourceCell - 1, gridDimension, true, false);
            validLeftFluxRange[0] = LPSolve.SolveModel();
            LPSolve.SetEdgeToSolve(sourceCell, sourceCell - 1, gridDimension, true, true);
            validLeftFluxRange[1] = LPSolve.SolveModel();
        }

        if (colNumber == gridDimension - 1)
        {
            validRightFluxRange[0] = 0;
            validRightFluxRange[1] = 0;
        }

        else
        {
            LPSolve.SetEdgeToSolve(sourceCell, sourceCell + 1, gridDimension, true, false);
            validRightFluxRange[0] = LPSolve.SolveModel();
            LPSolve.SetEdgeToSolve(sourceCell, sourceCell + 1, gridDimension, true, true);
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
                        
                        currentValidTiles.Add(new FlowTile(innerTileGridDimension, flux, new CornerVelocities()));
                    }
                }
            }
        }

        Console.WriteLine("number of tiles: " + currentValidTiles.Count);

        List<FlowTile> newValidTiles = LPSolve.FilterValidTiles(currentValidTiles, rowNumber, colNumber, gridDimension);

        Console.WriteLine("final number of tiles: " + newValidTiles.Count);
        return newValidTiles;
    }
}


