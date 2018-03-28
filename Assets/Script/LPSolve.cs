using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using lpsolve55;
using FlowTilesUtils;


public class LPSolve
{
    private static System.IntPtr LpModel;

    private enum Direction { Top, Right, Bottom, Left }

    /* Creates a linear programming model for solving a specific network flow problem.
     */
    public static void BuildInitialModel(int minXFlux, int maxXFlux, int minYFlux, int maxYFlux,
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
                if (!currentTileGrid.HasTile(row, column))
                {
                    tileEdges = TileEdgeIndices(row, column, gridDimension);
                    lpsolve.add_constraintex(LpModel, 4, new double[] { 1, -1, -1, 1 }, tileEdges, lpsolve.lpsolve_constr_types.EQ, 0);
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

                if (!currentTileGrid.HasTile(row, column))
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
                        currentTileGrid.GetFlowTile(row, column).Flux.topEdge, 
                        currentTileGrid.GetFlowTile(row, column).Flux.topEdge);

                    lpsolve.set_bounds(LpModel, tileEdges[(int)Direction.Right],
                        currentTileGrid.GetFlowTile(row, column).Flux.rightEdge, 
                        currentTileGrid.GetFlowTile(row, column).Flux.rightEdge);

                    lpsolve.set_bounds(LpModel, tileEdges[(int)Direction.Bottom],
                        currentTileGrid.GetFlowTile(row, column).Flux.bottomEdge, 
                        currentTileGrid.GetFlowTile(row, column).Flux.bottomEdge);

                    lpsolve.set_bounds(LpModel, tileEdges[(int)Direction.Left], 
                        currentTileGrid.GetFlowTile(row, column).Flux.leftEdge, 
                        currentTileGrid.GetFlowTile(row, column).Flux.leftEdge);
                }
            }
        }

        lpsolve.set_verbose(LpModel, 0);     
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

    public static void FreeModel()
    {
        lpsolve.delete_lp(LpModel);
    }

    //Computes the 1d arrayindex for edge from sourceCell to destCell. Indexes of variables in lpsolve start at 1.
    private static int GridToEdgeIndex(int sourceCell, int destCell, int gridDimension, bool isXDirection)
    {
        if (isXDirection)
        {
            int tempIndex = (destCell == -1 ? sourceCell : destCell);
            //this is the leftmost index on the row
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

    public static List<FlowTile> FilterValidTiles(List<FlowTile> currentValidTiles, 
        int rowNumber, int colNumber, int gridDimension)
    {
        int[] variableIndex = new int[1];
        List<FlowTile> returnList = new List<FlowTile>();

        foreach (FlowTile tile in currentValidTiles)
        {
            //Add constraints corresponding to the tile edge flows
            variableIndex[0] = TileEdgeIndices(rowNumber, colNumber, gridDimension)[(int)Direction.Top];
            lpsolve.add_constraintex(LpModel, 1, new double[] { 1 }, 
                variableIndex, lpsolve.lpsolve_constr_types.EQ, tile.Flux.topEdge);

            variableIndex[0] = TileEdgeIndices(rowNumber, colNumber, gridDimension)[(int)Direction.Right];
            lpsolve.add_constraintex(LpModel, 1, new double[] { 1 }, 
                variableIndex, lpsolve.lpsolve_constr_types.EQ, tile.Flux.rightEdge);

            variableIndex[0] = TileEdgeIndices(rowNumber, colNumber, gridDimension)[(int)Direction.Bottom];
            lpsolve.add_constraintex(LpModel, 1, new double[] { 1 }, 
                variableIndex, lpsolve.lpsolve_constr_types.EQ, tile.Flux.bottomEdge);

            variableIndex[0] = TileEdgeIndices(rowNumber, colNumber, gridDimension)[(int)Direction.Left];
            lpsolve.add_constraintex(LpModel, 1, new double[] { 1 }, 
                variableIndex, lpsolve.lpsolve_constr_types.EQ, tile.Flux.leftEdge);

            //If a feasible solution is found, tile is a possibility for the location
            if (lpsolve.solve(LpModel) == lpsolve.lpsolve_return.OPTIMAL)
            {
                returnList.Add(tile);
            }

            //Delete the candidate tile restrictions from the model.
            int numberOfRows = lpsolve.get_Nrows(LpModel);
            for (int row = numberOfRows; row >= numberOfRows - 3; row--)
            {
                lpsolve.del_constraint(LpModel, row);
            }
        }

        return returnList;
    }

    public static void Main(string[] args)
    {
        /*   System.Console.WriteLine(lpsolve.get_Nrows(LpModel));
        */
        int dimension = 3;
        GridBuilder gridBuilder = new GridBuilder(-1, 1, -1, 1, dimension);
        TileGrid tileGrid = gridBuilder.BuildRandomTileGrid();
        for (int row = 0; row < dimension; row++)
        {
            for (int col = 0; col < dimension; col++)
            {
                FlowTile currentTile = tileGrid.GetFlowTile(row, col);
                System.Console.WriteLine("Position " + "(" + row + "," + col + ")" + " : top = " + 
                    currentTile.Flux.topEdge + ", right = " + currentTile.Flux.rightEdge +
                    ", bottom = " + currentTile.Flux.bottomEdge + ", left = " + currentTile.Flux.leftEdge);
            }
        }

    }
}