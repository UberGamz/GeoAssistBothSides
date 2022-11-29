using Mastercam.Database;
using Mastercam.IO;
using Mastercam.Database.Types;
using Mastercam.App.Types;
using Mastercam.GeometryUtility;
using Mastercam.Support;

namespace _GeoAssistBothSides
{
    public class GeoAssistBothSides : Mastercam.App.NetHook3App
    {
        public Mastercam.App.Types.MCamReturn GeoAssistBothSidesRun(Mastercam.App.Types.MCamReturn notused)
        {
            void offsetCutchain(){
                SelectionManager.UnselectAllGeometry();
                LevelsManager.RefreshLevelsManager();
                GraphicsManager.Repaint(true);

                var selectedCutChain = ChainManager.GetMultipleChains("Select Geometry");
                int mainGeo = 10;
                int cleanOut = 12;
                int roughSurf = 138;
                int finishSurf = 139;

                foreach (var chain in selectedCutChain){

                    var mainGeoSide1 = chain.OffsetChain2D(OffsetSideType.Left, .002, OffsetRollCornerType.All, .5, false, .005, false);
                    var mainGeoSide2 = chain.OffsetChain2D(OffsetSideType.Right, .002, OffsetRollCornerType.All, .5, false, .005, false);
                    var mainGeoResult = SearchManager.GetResultGeometry();
                    foreach (var entity in mainGeoResult){
                        entity.Color = 10;
                        entity.Selected = true;
                        entity.Commit();
                    }
                    GeometryManipulationManager.MoveSelectedGeometryToLevel(mainGeo, true);
                    GraphicsManager.ClearColors(new GroupSelectionMask(true));

                    var cleanOutSide1 = chain.OffsetChain2D(OffsetSideType.Left, .0025, OffsetRollCornerType.All, .5, false, .005, false);
                    var cleanOutSide2 = chain.OffsetChain2D(OffsetSideType.Right, .0025, OffsetRollCornerType.All, .5, false, .005, false);
                    var cleanOutResult = SearchManager.GetResultGeometry();
                    foreach (var entity in cleanOutResult){
                        entity.Color = 12;
                        entity.Selected = true;
                        entity.Commit();
                    }
                    GeometryManipulationManager.MoveSelectedGeometryToLevel(cleanOut, true);
                    GraphicsManager.ClearColors(new GroupSelectionMask(true));

                    var finishSurfSide1 = chain.OffsetChain2D(OffsetSideType.Left, .0005, OffsetRollCornerType.All, .5, false, .005, false);
                    var finishSurfSide2 = chain.OffsetChain2D(OffsetSideType.Right, .0005, OffsetRollCornerType.All, .5, false, .005, false);
                    var finishSurfResult = SearchManager.GetResultGeometry();
                    foreach (var entity in finishSurfResult){
                        entity.Color = 139;
                        entity.Selected = true;
                        entity.Commit();
                    }
                    GeometryManipulationManager.MoveSelectedGeometryToLevel(finishSurf, true);
                    GraphicsManager.ClearColors(new GroupSelectionMask(true));
                }
            }
            offsetCutchain();
            GraphicsManager.Repaint(true);
            return MCamReturn.NoErrors;
        }
    }
}