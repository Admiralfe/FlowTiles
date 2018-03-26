using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using lpsolve55;


public class LPSolve
{
    private static System.IntPtr LpModel;

    private enum Direction { Top, Right, Bottom, Left }

    /* Creates a linear programming model for solving a specific network flow problem.
     */
    public static System.IntPtr BuildInitialModel(int minXFlux, int maxXFlux, int minYFlux, int maxYFlux,
        TileGrid currentTileGrid)
    {
        int gridDimension = currentTileGrid.Dimension;

        //Number of variables is 2*n*(n+1) for n x n - grid
        LpModel = lpsolve.make_lp(0, 2 * gridDimension * (gridDimension + 1));

        //We always have four variables in the constraint
        int[] tileEdges = new int[4];

        //Improves performance when adding rows to model
        lpsolve.set_add_rowmode(LpModel, 1);
        lpsolve.set_obj_fn(LpModel, new double[2 * gridDimension * (gridDimension + 1)]);

        //Add incompressibility constraint to all cells which don't already have a flowtile on them.
        for (int row = 0; row < gridDimension; row++)
        {
            for (int column = 0; column < gridDimension; column++)
            {
                if (!currentTileGrid.hasTile(row, column))
                {
                    tileEdges = TileEdgeIndices(row, column, gridDimension);
                    lpsolve.add_constraintex(LpModel, 4, new double[] { -1, -1, 1, 1 }, tileEdges, lpsolve.lpsolve_constr_types.EQ, 0);
                }
            }
        }
        //Add row mode has to be turned off before continuing with other operations, if not then program crashes.
        lpsolve.set_add_rowmode(LpModel, 0);

        //Iterate over all rows and columns in the grid and set bounds on variables.
        for (int row = 0; row < gridDimension; row++)
        {
            for (int column = 0; column < gridDimension; column++)
            {
                tileEdges = TileEdgeIndices(row, column, gridDimension);

                if (!currentTileGrid.hasTile(row, column))
                {
                    //Set the bounds for each edge flow. Zero if it is a boundary edge.
                    if (row == 0)
                    {
                        if (lpsolve.set_bounds(LpModel, tileEdges[(int)Direction.Top], 0, 0) == 0)
                        {
                            System.Console.WriteLine("Set bounds error");
                        }
                    }
                    else
                    {
                        if (lpsolve.set_bounds(LpModel, tileEdges[(int)Direction.Top], minYFlux, maxYFlux) == 0)
                        {
                            System.Console.WriteLine("Set bounds error");
                        }
                    }

                    if (column == 0)
                    {
                        if (lpsolve.set_bounds(LpModel, tileEdges[(int)Direction.Left], 0, 0) == 0)
                        {
                            System.Console.WriteLine("Set bounds error");
                        }
                    }
                    else
                    {
                        if (lpsolve.set_bounds(LpModel, tileEdges[(int)Direction.Left], minXFlux, maxXFlux) == 0)
                        {
                            System.Console.WriteLine("Set bounds error");
                        }
                    }

                    if (row == gridDimension - 1)
                    {
                        if (lpsolve.set_bounds(LpModel, tileEdges[(int)Direction.Bottom], 0, 0) == 0)
                        {
                            System.Console.WriteLine("Set bounds error");
                        }
                    }
                    else
                    {
                        if (lpsolve.set_bounds(LpModel, tileEdges[(int)Direction.Bottom], minYFlux, maxYFlux) == 0)
                        {
                            System.Console.WriteLine("Set bounds error");
                        }
                    }

                    if (column == gridDimension - 1)
                    {
                        if (lpsolve.set_bounds(LpModel, tileEdges[(int)Direction.Right], 0, 0) == 0)
                        {
                            System.Console.WriteLine("Set bounds error");
                        }
                    }
                    else
                    {
                        if (lpsolve.set_bounds(LpModel, tileEdges[(int)Direction.Right], minXFlux, maxXFlux) == 0)
                        {
                            System.Console.WriteLine("Set bounds error");
                        }
                    }
                }
                //Else there is a tile on the slot and the values are bounded by the flows from that tile
                else
                {
                    lpsolve.set_bounds(LpModel, tileEdges[(int)Direction.Top],
                        currentTileGrid.TileSet[row, column][(int)Direction.Top], currentTileGrid.TileSet[row, column][(int)Direction.Top]);

                    lpsolve.set_bounds(LpModel, tileEdges[(int)Direction.Right],
                        currentTileGrid.TileSet[row, column][(int)Direction.Right], currentTileGrid.TileSet[row, column][(int)Direction.Right]);

                    lpsolve.set_bounds(LpModel, tileEdges[(int)Direction.Bottom],
                        currentTileGrid.TileSet[row, column][(int)Direction.Bottom], currentTileGrid.TileSet[row, column][(int)Direction.Bottom]);

                    lpsolve.set_bounds(LpModel, tileEdges[(int)Direction.Left],
                        currentTileGrid.TileSet[row, column][(int)Direction.Left], currentTileGrid.TileSet[row, column][(int)Direction.Left]);
                }
            }
        }

        return LpModel;


    }

    public static void SetEdgeToSolve(int sourceCell, int destCell, int gridDimension, bool isXDirection, bool maximize)
    {
        double[] rowVector = { 1 };
        int[] constraintVector = { GridToEdgeIndex(sourceCell, destCell, gridDimension, isXDirection) };

        //Sets the variable, that is edge, to solve for
        if (lpsolve.set_obj_fnex(LpModel, 1, rowVector, constraintVector) == 0)
        {
            System.Console.WriteLine("Set object function error");
        }

        //Sets if we want to maximize or minimize the flux
        if (maximize)
        {
            lpsolve.set_maxim(LpModel);
        }
        else
        {
            lpsolve.set_minim(LpModel);
        }

        //lpsolve.set_simplextype(LpModel, lpsolve.lpsolve_simplextypes.SIMPLEX_PRIMAL_PRIMAL);
    }

    public static int SolveModel()
    {
        lpsolve.solve(LpModel);

        return (int)lpsolve.get_objective(LpModel);
    }

    //Computes the 1d arrayindex for edge from sourceCell to destCell. Indexes of variables in lpsolve start at 1.
    private static int GridToEdgeIndex(int sourceCell, int destCell, int gridDimension, bool isXDirection)
    {
        if (isXDirection)
        {
            //this is the leftmost index on the row
            int tempIndex = (destCell == -1 ? sourceCell : destCell);
            int leftMostIndex = (tempIndex / gridDimension) * (2 * gridDimension + 1) + gridDimension + 1;

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
            int tempIndex = (sourceCell == -1 ? destCell : sourceCell);
            int topIndex = tempIndex % gridDimension + 1;

            //Corresponds to a flow coming from outside the grid from above
            if (sourceCell == -1)
                return topIndex;

            //Corresponds to a flow flowing out of the grid downwards
            if (destCell == -1)
                return topIndex + gridDimension * (2 * gridDimension + 1);

            return topIndex + (destCell / gridDimension) * (2 * gridDimension + 1);
        }

    }

    private static int[] TileEdgeIndices(int rowNumber, int colNumber, int gridDimension)
    {
        //1d index of current cell
        int cellIndex = rowNumber * gridDimension + colNumber;

        int[] result = new int[4];

        //Check if cell is on the boundary
        if (rowNumber == 0)
            result[(int)Direction.Top] = GridToEdgeIndex(-1, cellIndex, gridDimension, false);
        else
            result[(int)Direction.Top] = GridToEdgeIndex(cellIndex - gridDimension, cellIndex, gridDimension, false);

        if (colNumber == 0)
            result[(int)Direction.Left] = GridToEdgeIndex(-1, cellIndex, gridDimension, true);
        else
            result[(int)Direction.Left] = GridToEdgeIndex(cellIndex - 1, cellIndex, gridDimension, true);

        if (colNumber == gridDimension - 1)
            result[(int)Direction.Right] = GridToEdgeIndex(cellIndex, -1, gridDimension, true);
        else
            result[(int)Direction.Right] = GridToEdgeIndex(cellIndex, cellIndex + 1, gridDimension, true);

        if (rowNumber == gridDimension - 1)
            result[(int)Direction.Bottom] = GridToEdgeIndex(cellIndex, -1, gridDimension, false);
        else
            result[(int)Direction.Bottom] = GridToEdgeIndex(cellIndex, cellIndex + gridDimension, gridDimension, false);

        return result;
    }

    public static void FilterValidTiles(ref List<GridBuilder.FlowTile> currentValidTiles, int rowNumber, int colNumber, int gridDimension)
    {
        int[] variableIndex = new int[1];
        foreach (GridBuilder.FlowTile Tile in currentValidTiles)
        {   
            //Add constraints corresponding to the tile edge flows
            variableIndex[0] = TileEdgeIndices(rowNumber, colNumber, gridDimension)[(int)Direction.Top];
            lpsolve.add_constraintex(LpModel, 1, new double[] { 1 }, variableIndex, lpsolve.lpsolve_constr_types.EQ, Tile.topFlux);

            variableIndex[0] = TileEdgeIndices(rowNumber, colNumber, gridDimension)[(int)Direction.Right];
            lpsolve.add_constraintex(LpModel, 1, new double[] { 1 }, variableIndex, lpsolve.lpsolve_constr_types.EQ, Tile.rightFlux);

            variableIndex[0] = TileEdgeIndices(rowNumber, colNumber, gridDimension)[(int)Direction.Bottom];
            lpsolve.add_constraintex(LpModel, 1, new double[] { 1 }, variableIndex, lpsolve.lpsolve_constr_types.EQ, Tile.bottomFlux);

            variableIndex[0] = TileEdgeIndices(rowNumber, colNumber, gridDimension)[(int)Direction.Left];
            lpsolve.add_constraintex(LpModel, 1, new double[] { 1 }, variableIndex, lpsolve.lpsolve_constr_types.EQ, Tile.leftFlux);

            //If no feasible solution is found then tile is not a possibility for the position
            if (lpsolve.solve(LpModel) == lpsolve.lpsolve_return.INFEASIBLE)
            {
                currentValidTiles.Remove(Tile);
            }

            //Delete the candidate tile restrictions from the model.
            for (int row = lpsolve.get_Nrows(LpModel) - 3; row <= lpsolve.get_Nrows(LpModel); row++)
            {
                lpsolve.del_constraint(LpModel, row);
            }
        }
    }

    public static void Main(string[] args)
    {
        BuildInitialModel(-1, 1, -1, 1, new TileGrid(4));
        
        lpsolve.print_lp(LpModel);
    }
}