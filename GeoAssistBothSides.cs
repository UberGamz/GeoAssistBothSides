using Mastercam.Database;
using Mastercam.IO;
using Mastercam.Database.Types;
using Mastercam.App.Types;
using Mastercam.GeometryUtility;
using Mastercam.Support;
using Mastercam.Database.Interop;
using Mastercam.Curves;
using Mastercam.Math;
using System.Collections.Generic;

namespace _GeoAssistBothSides
{
    public class GeoAssistBothSides : Mastercam.App.NetHook3App
    {
        public Mastercam.App.Types.MCamReturn GeoAssistBothSidesRun(Mastercam.App.Types.MCamReturn notused)
        {
            void offsetCutchain()
            {
                var depth = -0.100;
                var roughAngle = 15.0;
                var finishAngle = 20.0;
                var levelTenList = new List<int>();
                var level139List = new List<int>();
                SelectionManager.UnselectAllGeometry();
                LevelsManager.RefreshLevelsManager();
                GraphicsManager.Repaint(true);
                int mainGeo = 10;
                int cleanOut = 12;
                int roughSurf = 138;
                int finishSurf = 139;
                SurfaceDraftParams roughSurfaceDraftParams = new SurfaceDraftParams
                {
                    draftMethod = SurfaceDraftParams.DraftMethod.Length,
                    geometryType = SurfaceDraftParams.GeometryType.Surface,
                    length = depth,
                    angle = Mastercam.Math.VectorManager.RadiansToDegrees(roughAngle),
                    draftDirection = SurfaceDraftParams.DraftDirection.Defined
                };
                SurfaceDraftParams finishSurfaceDraftParams = new SurfaceDraftParams
                {
                    draftMethod = SurfaceDraftParams.DraftMethod.Length,
                    geometryType = SurfaceDraftParams.GeometryType.Surface,
                    length = depth,
                    angle = Mastercam.Math.VectorManager.RadiansToDegrees(finishAngle),
                    draftDirection = SurfaceDraftParams.DraftDirection.Defined
                };
                var selectedCutChain = ChainManager.GetMultipleChains("Select Geometry");
                DialogManager.AskForNumber("Enter Depth", ref depth);
                DialogManager.AskForAngle("Enter Rough Angle", ref roughAngle);
                DialogManager.AskForAngle("Enter Finish Angle", ref finishAngle);
                foreach (var chain in selectedCutChain)
                {
                    var chainGeo = ChainManager.GetGeometryInChain(chain);
                    foreach (var entity in chainGeo)
                    {
                        if (entity is SplineGeometry || entity is NURBSCurveGeometry)
                        {
                            Mastercam.IO.DialogManager.Error("Spline Found", "Fix Splines and try again");
                            return;
                        }
                    }
                }
                foreach (var chain in selectedCutChain)
                {
                    var mainGeoSide1 = chain.OffsetChain2D(OffsetSideType.Left, .002, OffsetRollCornerType.None, .5, false, .005, false);
                    var mainGeoSide2 = chain.OffsetChain2D(OffsetSideType.Right, .002, OffsetRollCornerType.None, .5, false, .005, false);
                    var mainGeoResult = SearchManager.GetResultGeometry();
                    foreach (var entity in mainGeoResult)
                    {
                        entity.Color = mainGeo;
                        entity.Level = mainGeo;
                        entity.Selected = false;
                        levelTenList.Add(entity.GetEntityID());
                        entity.Commit();
                    }
                    foreach (var entity1 in mainGeoResult)
                    {
                        if (entity1 is LineGeometry line1)
                        {
                            foreach (var entity2 in mainGeoResult)
                            {
                                if (entity2 is LineGeometry line2 && entity1.GetEntityID() != entity2.GetEntityID())
                                {
                                    if ((VectorManager.Distance(line1.EndPoint1, line2.EndPoint1) <= 0.001)
                                        ||
                                        (VectorManager.Distance(line1.EndPoint1, line2.EndPoint2) <= 0.001)
                                        ||
                                        (VectorManager.Distance(line1.EndPoint2, line2.EndPoint2) <= 0.001)
                                        ||
                                        (VectorManager.Distance(line1.EndPoint2, line2.EndPoint1) <= 0.001))
                                    {
                                        var newFillet = GeometryManipulationManager.FilletTwoCurves(line1, line2, 0.0020, mainGeo, mainGeo, true);
                                        if (newFillet != null)
                                        {
                                            newFillet.Retrieve();
                                            newFillet.Commit();
                                            levelTenList.Add(newFillet.GetEntityID());
                                        }
                                        line1.Retrieve();
                                        line1.Commit();
                                        line2.Retrieve();
                                        line2.Commit();
                                    }
                                }
                            }
                        }
                    }
                    foreach (var entity in levelTenList)
                    {
                        var entityGeo = Geometry.RetrieveEntity(entity);
                        entityGeo.Selected = true;
                        entityGeo.Commit();
                    }
                    var levelTenGeo = SearchManager.GetSelectedGeometry();
                    var thisChain10 = ChainManager.ChainGeometry(levelTenGeo);
                    foreach (var draftChain10 in thisChain10)
                    {
                        var draftSurface10 = SurfaceDraftInterop.CreateDrafts(draftChain10, roughSurfaceDraftParams, false, 1);
                        foreach (var surface10 in draftSurface10)
                        {
                            if (Geometry.RetrieveEntity(surface10) is Geometry roughDraftSurface10)
                            {
                                roughDraftSurface10.Level = roughSurf;
                                roughDraftSurface10.Commit();
                            }
                        }
                    }
                    GraphicsManager.ClearColors(new GroupSelectionMask(true));
                    SelectionManager.UnselectAllGeometry();
                    ///////////////////////////////
                    var cleanOutSide1 = chain.OffsetChain2D(OffsetSideType.Left, .0025, OffsetRollCornerType.None, .5, false, .005, false);
                    var cleanOutSide2 = chain.OffsetChain2D(OffsetSideType.Right, .0025, OffsetRollCornerType.None, .5, false, .005, false);
                    var cleanOutResult = SearchManager.GetResultGeometry();
                    foreach (var entity in cleanOutResult)
                    {
                        entity.Level = cleanOut;
                        entity.Color = cleanOut;
                        entity.Selected = false;
                        entity.Commit();
                    }
                    foreach (var entity1 in cleanOutResult)
                    {
                        if (entity1 is LineGeometry line1)
                        {
                            foreach (var entity2 in cleanOutResult)
                            {
                                if (entity2 is LineGeometry line2 && entity1.GetEntityID() != entity2.GetEntityID())
                                {
                                    if ((VectorManager.Distance(line1.EndPoint1, line2.EndPoint1) <= 0.001)
                                        ||
                                        (VectorManager.Distance(line1.EndPoint1, line2.EndPoint2) <= 0.001)
                                        ||
                                        (VectorManager.Distance(line1.EndPoint2, line2.EndPoint2) <= 0.001)
                                        ||
                                        (VectorManager.Distance(line1.EndPoint2, line2.EndPoint1) <= 0.001))
                                    {
                                        var newFillet = GeometryManipulationManager.FilletTwoCurves(line1, line2, 0.0025, cleanOut, cleanOut, true);
                                        if (newFillet != null)
                                        {
                                            newFillet.Retrieve();
                                            newFillet.Commit();
                                            levelTenList.Add(newFillet.GetEntityID());
                                        }
                                        line1.Retrieve();
                                        line1.Commit();
                                        line2.Retrieve();
                                        line2.Commit();
                                    }
                                }
                            }
                        }
                    }
                    GraphicsManager.ClearColors(new GroupSelectionMask(true));
                    SelectionManager.UnselectAllGeometry();
                    ////////////////////////////////
                    var finishSurfSide1 = chain.OffsetChain2D(OffsetSideType.Left, .0005, OffsetRollCornerType.None, .5, false, .005, false);
                    var finishSurfSide2 = chain.OffsetChain2D(OffsetSideType.Right, .0005, OffsetRollCornerType.None, .5, false, .005, false);
                    var finishSurfResult = SearchManager.GetResultGeometry();
                    foreach (var entity in finishSurfResult)
                    {
                        entity.Color = finishSurf;
                        entity.Level = finishSurf;
                        entity.Selected = false;
                        level139List.Add(entity.GetEntityID());
                        entity.Commit();
                    }
                    foreach (var entity1 in finishSurfResult)
                    {
                        if (entity1 is LineGeometry line1)
                        {
                            foreach (var entity2 in finishSurfResult)
                            {
                                if (entity2 is LineGeometry line2 && entity1.GetEntityID() != entity2.GetEntityID())
                                {
                                    if ((VectorManager.Distance(line1.EndPoint1, line2.EndPoint1) <= 0.001)
                                        ||
                                        (VectorManager.Distance(line1.EndPoint1, line2.EndPoint2) <= 0.001)
                                        ||
                                        (VectorManager.Distance(line1.EndPoint2, line2.EndPoint2) <= 0.001)
                                        ||
                                        (VectorManager.Distance(line1.EndPoint2, line2.EndPoint1) <= 0.001))
                                    {
                                        var newFillet = GeometryManipulationManager.FilletTwoCurves(line1, line2, 0.0005, finishSurf, finishSurf, true);
                                        if (newFillet != null)
                                        {
                                            newFillet.Retrieve();
                                            newFillet.Commit();
                                            level139List.Add(newFillet.GetEntityID());
                                        }
                                        line1.Retrieve();
                                        line1.Commit();
                                        line2.Retrieve();
                                        line2.Commit();
                                    }
                                }
                            }
                        }
                    }
                    foreach (var entity in level139List)
                    {
                        var entityGeo = Geometry.RetrieveEntity(entity);
                        entityGeo.Selected = true;
                        entityGeo.Commit();
                    }
                    var level139Geo = SearchManager.GetSelectedGeometry();
                    var thisChain139 = ChainManager.ChainGeometry(level139Geo);
                    foreach (var draftChain139 in thisChain139)
                    {
                        var draftSurface139 = SurfaceDraftInterop.CreateDrafts(draftChain139, finishSurfaceDraftParams, false, 1);
                        foreach (var surface139 in draftSurface139)
                        {
                            if (Geometry.RetrieveEntity(surface139) is Geometry finishDraftSurface139)
                            {
                                finishDraftSurface139.Level = finishSurf;
                                finishDraftSurface139.Commit();
                            }
                        }
                    }
                    GraphicsManager.ClearColors(new GroupSelectionMask(true));
                    SelectionManager.UnselectAllGeometry();
                    ////////////////////////////////
                }
            }
            offsetCutchain();


            return MCamReturn.NoErrors;
        }
    }
}