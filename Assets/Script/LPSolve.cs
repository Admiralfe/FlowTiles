using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using lpsolve55;

public class LPSolve
{
    public static int buildInitialModel(int minXFlux, int maxXFlux, int minYFlux, int maxYFlux, int noOfNeighbors)
    {
        int lpModel;
        int noOfColumns;

        lpsolve.make_lp(0,0);
    }