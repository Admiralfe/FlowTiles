using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using lpsolve55;


public class LPSolve
{
    public static System.IntPtr LpModel;

    public static void BuildInitialModel(int minXFlux, int maxXFlux, int minYFlux, int maxYFlux, 
        int gridDimension, bool maximize)
    {
        double[] objectFnVector = new double[2 * gridDimension * (gridDimension + 1)];

        //We always have four variables in the constraint
        int[] constraintVector = new int[4];

        //Number of variables is 2*n*(n+1) for n x n - grid
        LpModel = lpsolve.make_lp(0, 2 * gridDimension * (gridDimension + 1));
        
        //Improves performance when adding rows to model
        lpsolve.set_add_rowmode(LpModel, 1);
        
        //Set flows on the top and bottom boundary of the grid to 0
        for (int n = 0; n < gridDimension; n++)
        {
            lpsolve.set_bounds(LpModel, n, 0, 0);
            lpsolve.set_bounds(LpModel, n + gridDimension * (2 * gridDimension + 1), 0, 0);
        }
        
        //Set flows on the left and right boundary of the grid to 0
        for (int n = gridDimension; n < gridDimension * (2 * gridDimension + 1); n += (2 * gridDimension + 1))
        {
            lpsolve.set_bounds(LpModel, n, 0, 0);
            lpsolve.set_bounds(LpModel, n + gridDimension, 0, 0);
        }
        
        //Set constraints on individual INTERNAL flows
        for (int sourceCell = 0; sourceCell < gridDimension * gridDimension; sourceCell++)
        {    
            //Exclude the boundary flows which are already set to zero
            if (sourceCell % gridDimension != gridDimension - 1)
            {
                //x direction since source and destination cells are adjacent
                lpsolve.set_bounds(LpModel, GridToVariableIndex(sourceCell, sourceCell + 1, gridDimension, true),
                    minXFlux, maxXFlux);
            }
            
            //Exclude the boundary flows which are already set to zero
            if (!(sourceCell >= gridDimension * gridDimension - gridDimension)) {
                //y direction
                lpsolve.set_bounds(LpModel,
                    GridToVariableIndex(sourceCell, sourceCell + gridDimension, gridDimension, false), minYFlux,
                    minXFlux);
            }
        }

        //Set all constraints
        for (int currentCell = 0; currentCell < gridDimension * gridDimension; currentCell++)
        {
            double[] rowVector = { -1, -1, 1, 1 };
            //Check if current cell is on the top row or not
            if (currentCell < gridDimension)
                constraintVector[0] = GridToVariableIndex(-1, currentCell, gridDimension, false);
            else
                constraintVector[0] = GridToVariableIndex(currentCell - gridDimension, currentCell, gridDimension, false);

            //Check if current cell is in the leftmost column
            if ((currentCell % gridDimension) == 0)
                constraintVector[1] = GridToVariableIndex(-1, currentCell, gridDimension, true);
            else
                constraintVector[1] = GridToVariableIndex(currentCell - 1, currentCell, gridDimension, true);

            //Check if current cell is in the rightmost column
            if ((currentCell % gridDimension) == gridDimension - 1)
                constraintVector[2] = GridToVariableIndex(currentCell, -1, gridDimension, true);
            else
                constraintVector[2] = GridToVariableIndex(currentCell, currentCell + 1, gridDimension, true);

            //Check if current cell is in the bottom row
            if (currentCell >= gridDimension * gridDimension - gridDimension)
                constraintVector[3] = GridToVariableIndex(currentCell, -1, gridDimension, false);
            else
                constraintVector[3] = GridToVariableIndex(currentCell, currentCell + gridDimension, gridDimension, false);

            lpsolve.add_constraintex(LpModel, 4, rowVector, constraintVector, lpsolve.lpsolve_constr_types.EQ, 0);
        }

        return;
    }

    public static void SetEdgeToSolve(int sourceCell, int destCell, int gridDimension, bool isXDirection, bool maximize)
    {
        double[] rowVector = { -1 };
        int[] constraintVector = { GridToVariableIndex(sourceCell, destCell, gridDimension, isXDirection};

        //Sets the variable, that is edge, to solve for
        lpsolve.set_obj_fnex(LpModel, 1, rowVector, constraintVector);

        //Sets if we want to maximize or minimize the flux
        if (maximize)
            lpsolve.set_maxim(LpModel);
        else
            lpsolve.set_minim(LpModel);

        //Sets the solution algorithm to be a simplex solver
        lpsolve.set_simplextype(LpModel, lpsolve.lpsolve_simplextypes.SIMPLEX_PRIMAL_PRIMAL;
    }

    //Computes the 1d arrayindex for edge from sourceCell to destCell. Indexes of variables in lpsolve start at 1.
    private static int GridToVariableIndex(int sourceCell, int destCell, int gridDimension, bool isXDirection)
    {    
        if (isXDirection)
        {   
            //this is the leftmost index on the row
            int leftMostIndex = (destCell / gridDimension) * (2 * gridDimension + 1) + gridDimension + 1;
            
            //Corresponds to a flow coming from outside the grid from the left, that is leftmost index
            if (sourceCell == -1)
                return leftMostIndex;
            
            //Corresponds to a flow flowing out of the grid towards the right 
            if (destCell == -1)
                return leftMostIndex + gridDimension;

            return leftMostIndex + destCell % gridDimension;
        }
        
        //Else y-direction
        else
        {    
            //Topindex in the column
            int topIndex = sourceCell % gridDimension + 1;
            
            //Corresponds to a flow coming from outside the grid from above
            if (sourceCell == -1)
                return topIndex;
            
            //Corresponds to a flow flowing out of the grid downwards
            if (destCell == -1)
                return topIndex + gridDimension * (2 * gridDimension + 1);

            return topIndex + (destCell / gridDimension) * (2 * gridDimension + 1);
        }
    
    }
}