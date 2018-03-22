using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using lpsolve55;


public class LPSolve
{
    public static System.IntPtr LpModel;

    public static bool BuildInitialModel(int minXFlux, int maxXFlux, int minYFlux, int maxYFlux, 
        int gridDimension, int objectIndex, bool maximize)
    {
        double[] objectFnVector = new double[2 * gridDimension * (gridDimension + 1)];
        
        //We always have four variables in the constraint
        int[] constraintVector = new int[4];

        objectFnVector[objectIndex] = 1;

        //Number of variables is 2*n*(n+1) for n x n - grid
        LpModel = lpsolve.make_lp(0, 2 * gridDimension * (gridDimension + 1));
        
        if (LpModel.ToInt32() == 0)
            return false;
        
        //Sets which edges flow to maximize
        lpsolve.set_obj_fn(LpModel, objectFnVector);
        
        //Sets maximization or minimization of flow
        if (maximize)
            lpsolve.set_maxim(LpModel);
        else
            lpsolve.set_minim(LpModel);
        

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

        for (int currentCell = 0; currentCell < gridDimension * gridDimension; currentCell++)
        {
            //lpsolve.add_constraint(LpModel, constraintVector)
        }

        return true;
    }

    private static int GridToVariableIndex(int sourceCell, int destCell, int gridDimension, bool isXDirection)
    {    
        if (isXDirection)
        {   
            //this is the leftmost index on the row
            int leftMostIndex = (destCell / gridDimension) * (2 * gridDimension + 1) + gridDimension;
            
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
            int topIndex = sourceCell % gridDimension;
            
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