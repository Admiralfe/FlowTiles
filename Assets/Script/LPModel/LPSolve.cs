using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using lpsolve55;
using Script.FlowTileUtils;
using Script.GridBuilding;

namespace Script.LPModel
{
    public class LPSolve
    {
        private static System.IntPtr LpModel;

        public enum Direction
        {
            Top,
            Right,
            Bottom,
            Left
        }

        /// <summary>
        /// Creates a Linear Programming problem instance for creating divergence free flows using a tiling method.
        /// The instance is stored in <see cref="LpModel"/>.
        /// </summary>
        /// <param name="minXFlux">Minimum flux in X direction as an integer</param>
        /// <param name="maxXFlux">Maximum flux in X direction as an integer</param>
        /// <param name="minYFlux">Minimum flux in Y direction as an integer</param>
        /// <param name="maxYFlux">Maximum flux in Y direction as an integer</param>
        /// <param name="currentTileGrid"></param>
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
                        lpsolve.add_constraintex(LpModel, 4, new double[] {1, -1, -1, 1}, tileEdges,
                            lpsolve.lpsolve_constr_types.EQ, 0);
                    }
                }
            }

            //Add row mode has to be turned off before continuing with other operations, if not then program crashes.
            lpsolve.set_add_rowmode(LpModel, 0);
            bool[] edgeSetFlag = new bool[2 * gridDimension * (gridDimension + 1) + 1];

            //Iterate over all rows and columns in the grid and set bounds on variables.
            for (int row = 0; row < gridDimension; row++)
            {
                for (int column = 0; column < gridDimension; column++)
                {
                    tileEdges = TileEdgeIndices(row, column, gridDimension);

                    if (!currentTileGrid.HasTile(row, column))
                    {
                        //Set the bounds for each edge flow. Zero if it is a boundary edge.
                        if (!edgeSetFlag[tileEdges[(int) Direction.Top]])
                        {
                            if (row == 0)
                                lpsolve.set_bounds(LpModel, tileEdges[(int) Direction.Top], 0, 0);
                            else
                                lpsolve.set_bounds(LpModel, tileEdges[(int) Direction.Top], minYFlux, maxYFlux);
                        }

                        if (!edgeSetFlag[tileEdges[(int) Direction.Right]])
                        {
                            if (column == 0)
                                lpsolve.set_bounds(LpModel, tileEdges[(int) Direction.Left], 0, 0);
                            else
                                lpsolve.set_bounds(LpModel, tileEdges[(int) Direction.Left], minXFlux, maxXFlux);
                        }

                        if (!edgeSetFlag[tileEdges[(int) Direction.Bottom]])
                        {
                            if (row == gridDimension - 1)
                                lpsolve.set_bounds(LpModel, tileEdges[(int) Direction.Bottom], 0, 0);
                            else
                                lpsolve.set_bounds(LpModel, tileEdges[(int) Direction.Bottom], minYFlux, maxYFlux);
                        }

                        if (!edgeSetFlag[tileEdges[(int) Direction.Left]])
                        {
                            if (column == gridDimension - 1)
                                lpsolve.set_bounds(LpModel, tileEdges[(int) Direction.Right], 0, 0);
                            else
                                lpsolve.set_bounds(LpModel, tileEdges[(int) Direction.Right], minXFlux, maxXFlux);
                        }
                    }
                    //Else there is a tile on the slot and the values are bounded by the flows from that tile
                    else
                    {
                        lpsolve.set_bounds(LpModel, tileEdges[(int) Direction.Top],
                            currentTileGrid.GetFlowTile(row, column).Flux.TopEdge,
                            currentTileGrid.GetFlowTile(row, column).Flux.TopEdge);
                        
                        //Sets a flag that the edge flux has been set so that the value isn't overridden later.
                        edgeSetFlag[tileEdges[(int) Direction.Top]] = true;

                        lpsolve.set_bounds(LpModel, tileEdges[(int) Direction.Right],
                            currentTileGrid.GetFlowTile(row, column).Flux.RightEdge,
                            currentTileGrid.GetFlowTile(row, column).Flux.RightEdge);
                        
                        //Sets a flag that the edge flux has been set so that the value isn't overridden later.
                        edgeSetFlag[tileEdges[(int) Direction.Right]] = true;

                        lpsolve.set_bounds(LpModel, tileEdges[(int) Direction.Bottom],
                            currentTileGrid.GetFlowTile(row, column).Flux.BottomEdge,
                            currentTileGrid.GetFlowTile(row, column).Flux.BottomEdge);
                        
                        //Sets a flag that the edge flux has been set so that the value isn't overridden later.
                        edgeSetFlag[tileEdges[(int) Direction.Bottom]] = true;

                        lpsolve.set_bounds(LpModel, tileEdges[(int) Direction.Left],
                            currentTileGrid.GetFlowTile(row, column).Flux.LeftEdge,
                            currentTileGrid.GetFlowTile(row, column).Flux.LeftEdge);
                        
                        //Sets a flag that the edge flux has been set so that the value isn't overridden later.
                        edgeSetFlag[tileEdges[(int) Direction.Left]] = true;
                    }
                }
            }

            lpsolve.set_verbose(LpModel, 0);
        }
        
        /// <summary>
        /// Sets which edge in the tiling to maximize or minimize the flux for, given the LP-constraints specified
        /// when creating the LP-model using BuildInitialModel. The LPmodel referred to is <see cref="LpModel"/>.
        /// </summary>
        /// <param name="row">Row index of the tile</param>
        /// <param name="col">Column index of the tile</param>
        /// <param name="direction">Direction the edge is facing in the tile</param>
        /// <param name="gridDimension">Number of tiles in width and height the grid is.</param>
        /// <param name="maximize">Flag for minimizing or maximizing</param>
        public static void SetEdgeToSolve(int row, int col, Direction direction, int gridDimension, bool maximize)
        {
            double[] rowVector = {1};
            int[] constraintVector = {TileEdgeIndices(row, col, gridDimension)[(int) direction]};

            //Sets the variable, that is edge, to solve for
            if (lpsolve.set_obj_fnex(LpModel, 1, rowVector, constraintVector) == 0)
            {
                System.Console.WriteLine("Set object function error");
            }

            //Sets if we want to maximize or minimize the flux
            if (maximize)
                lpsolve.set_maxim(LpModel);
            else
                lpsolve.set_minim(LpModel);

            //lpsolve.set_simplextype(LpModel, lpsolve.lpsolve_simplextypes.SIMPLEX_PRIMAL_PRIMAL);
        }
        
        /// <summary>
        /// Solves the LP-model in <see cref="LpModel"/>.
        /// </summary>
        /// <returns>The value of the solution</returns>
        public static int SolveModel()
        {
            lpsolve.solve(LpModel);

            return (int) lpsolve.get_objective(LpModel);
        }
        
        /// <summary>
        /// Use this to free the memory allocated to the LP-problem instance when done with it.
        /// </summary>
        public static void FreeModel()
        {
            lpsolve.delete_lp(LpModel);
        }

        ///<summary>
        ///Computes the 1d array index (counted from top left to bottom right)
        ///for edge corresponding to flux from sourceCell to destCell in a square grid.
        /// Indexes of variables in lpsolve start at 1, so the return is indexed starting from 1.
        /// </summary>
        /// <param name="sourceCell">
        /// The cell from which the flux originates, -1 if flux comes from outside the grid
        /// </param>
        /// <param name="destCell">The cell to which the flux flows, -1 if flux flows to outside the grid</param>
        /// <param name="gridDimension">The dimension of the grid</param>
        /// <<param name="isXDirection">Flag for if the flow is in the x-direction (left-right direction).</param>
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
        
        /// <summary>
        /// Gives all the 1d edge indexes in a square grid for a tile in a given row and column
        /// </summary>
        /// <param name="rowNumber">Row number of tile</param>
        /// <param name="colNumber">Column number of tile</param>
        /// <param name="gridDimension">The dimension of the grid</param>
        /// <returns></returns>
        private static int[] TileEdgeIndices(int rowNumber, int colNumber, int gridDimension)
        {
            //1d index of current cell
            int cellIndex = rowNumber * gridDimension + colNumber;

            int[] result = new int[4];

            //Check if cell is on the boundary
            if (rowNumber == 0)
                result[(int) Direction.Top] = GridToEdgeIndex(-1, cellIndex, gridDimension, false);
            else
                result[(int) Direction.Top] =
                    GridToEdgeIndex(cellIndex - gridDimension, cellIndex, gridDimension, false);

            if (colNumber == 0)
                result[(int) Direction.Left] = GridToEdgeIndex(-1, cellIndex, gridDimension, true);
            else
                result[(int) Direction.Left] = GridToEdgeIndex(cellIndex - 1, cellIndex, gridDimension, true);

            if (colNumber == gridDimension - 1)
                result[(int) Direction.Right] = GridToEdgeIndex(cellIndex, -1, gridDimension, true);
            else
                result[(int) Direction.Right] = GridToEdgeIndex(cellIndex, cellIndex + 1, gridDimension, true);

            if (rowNumber == gridDimension - 1)
                result[(int) Direction.Bottom] = GridToEdgeIndex(cellIndex, -1, gridDimension, false);
            else
                result[(int) Direction.Bottom] =
                    GridToEdgeIndex(cellIndex, cellIndex + gridDimension, gridDimension, false);

            return result;
        }
        
        /// <summary>
        /// Filters valid tiles from the list by adding them one by one as constraints to the LPModel and checking
        /// if a solution exists.
        /// </summary>
        /// <param name="currentValidTiles">Set of candidate valid tiles</param>
        /// <param name="rowNumber">Which row the candidate tiles should be placed in</param>
        /// <param name="colNumber">Which column the candidate tiles should be placed in</param>
        /// <param name="gridDimension">Dimension of the grid tiling</param>
        /// <returns></returns>
        public static List<FlowTile> FilterValidTiles(List<FlowTile> currentValidTiles,
            int rowNumber, int colNumber, int gridDimension)
        {
            int[] variableIndex = new int[1];
            List<FlowTile> returnList = new List<FlowTile>();

            foreach (FlowTile tile in currentValidTiles)
            {
                //Add constraints corresponding to the tile edge flows
                variableIndex[0] = TileEdgeIndices(rowNumber, colNumber, gridDimension)[(int) Direction.Top];
                lpsolve.add_constraintex(LpModel, 1, new double[] {1},
                    variableIndex, lpsolve.lpsolve_constr_types.EQ, tile.Flux.TopEdge);

                variableIndex[0] = TileEdgeIndices(rowNumber, colNumber, gridDimension)[(int) Direction.Right];
                lpsolve.add_constraintex(LpModel, 1, new double[] {1},
                    variableIndex, lpsolve.lpsolve_constr_types.EQ, tile.Flux.RightEdge);

                variableIndex[0] = TileEdgeIndices(rowNumber, colNumber, gridDimension)[(int) Direction.Bottom];
                lpsolve.add_constraintex(LpModel, 1, new double[] {1},
                    variableIndex, lpsolve.lpsolve_constr_types.EQ, tile.Flux.BottomEdge);

                variableIndex[0] = TileEdgeIndices(rowNumber, colNumber, gridDimension)[(int) Direction.Left];
                lpsolve.add_constraintex(LpModel, 1, new double[] {1},
                    variableIndex, lpsolve.lpsolve_constr_types.EQ, tile.Flux.LeftEdge);

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
    }

}