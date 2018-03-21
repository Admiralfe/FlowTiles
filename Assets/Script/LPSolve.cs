using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using lpsolve55;

public class LPSolve {

    public static System.IntPtr lpModel;

    public static bool BuildInitialModel(int minXFlux, int maxXFlux, int minYFlux, int maxYFlux, int gridDimension, int objectElement)
    {

        //Number of variables is 2*n*(n+1) for n x n - grid.
        lpModel = lpsolve.make_lp(0, 2 * gridDimension * (gridDimension + 1));
        
        if (lpModel.ToInt32() == 0)
        {
            return false;
        }



        return true;
    }
}