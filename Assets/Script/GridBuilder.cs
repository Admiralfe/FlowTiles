using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridBuilder
{
    private List<int[]> currentValidTiles;
    
    private int minXFlux;
    private int maxXFlux;
    private int minYFlux;
    private int maxYFlux;

    public int gridDimension;
    
    public GridBuilder(int minXFluxIn, int maxXFluxIn, int minYFluxIn, int maxYFluxIn, int gridDimensionIn)
    {
        minXFlux = minXFluxIn;
        maxXFlux = maxXFluxIn;
        minYFlux = minYFluxIn;
        maxYFlux = maxYFluxIn;
        gridDimension = gridDimensionIn;
        
        //LPSolve.BuildInitialModel(minXFlux, maxXFlux, minYFlux, maxYFlux);
    }

    private void SetValidTiles(int rowNumber, int colNumber)
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
        
        
    }
}
