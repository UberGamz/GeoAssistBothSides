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
using Mastercam.IO.Types;

namespace _GeoAssistBothSides
{
    public class GeoAssistBothSides : Mastercam.App.NetHook3App
    {
        public Mastercam.App.Types.MCamReturn GeoAssistBothSidesRun(Mastercam.App.Types.MCamReturn notused)
        {
            void offsetCutchain()
            {

                var levelTenList1 = new List<int>();
                var level139List1 = new List<int>();
                var levelTenList2 = new List<int>();
                var level139List2 = new List<int>();
                SelectionManager.UnselectAllGeometry();
                LevelsManager.RefreshLevelsManager();
                GraphicsManager.Repaint(true);
                int mainGeo = 10;
                int cleanOut = 12;
                int roughSurf = 138;
                int finishSurf = 139;
                var depth = -0.100;
                var roughAngle = 15.0;
                var finishAngle = 20.0;
                var selectedCutChain = ChainManager.GetMultipleChains("Select Geometry");
                if (selectedCutChain == null)
                {
                    return;
                }
                var depthDialog = DialogManager.AskForNumber("Enter Depth", ref depth);
                if (depthDialog == 0)
                {
                    return;
                }
                    var roughAngleDialog = DialogManager.AskForAngle("Enter Rough Angle", ref roughAngle);
                if (roughAngleDialog == 0)
                {
                    return;
                }
                var finishAngleDialog = DialogManager.AskForAngle("Enter Finish Angle", ref finishAngle);
                if (finishAngleDialog == 0)
                {
                    return;
                }
                SurfaceDraftParams roughSurfaceDraftParams1 = new SurfaceDraftParams
                {
                    draftMethod = SurfaceDraftParams.DraftMethod.Length,
                    geometryType = SurfaceDraftParams.GeometryType.Surface,
                    length = depth,
                    angle = Mastercam.Math.VectorManager.RadiansToDegrees(-roughAngle),
                    draftDirection = SurfaceDraftParams.DraftDirection.Defined
                };
                SurfaceDraftParams finishSurfaceDraftParams1 = new SurfaceDraftParams
                {
                    draftMethod = SurfaceDraftParams.DraftMethod.Length,
                    geometryType = SurfaceDraftParams.GeometryType.Surface,
                    length = depth,
                    angle = Mastercam.Math.VectorManager.RadiansToDegrees(-finishAngle),
                    draftDirection = SurfaceDraftParams.DraftDirection.Defined
                };
                SurfaceDraftParams roughSurfaceDraftParams2 = new SurfaceDraftParams
                {
                    draftMethod = SurfaceDraftParams.DraftMethod.Length,
                    geometryType = SurfaceDraftParams.GeometryType.Surface,
                    length = depth,
                    angle = Mastercam.Math.VectorManager.RadiansToDegrees(roughAngle),
                    draftDirection = SurfaceDraftParams.DraftDirection.Defined
                };
                SurfaceDraftParams finishSurfaceDraftParams2 = new SurfaceDraftParams
                {
                    draftMethod = SurfaceDraftParams.DraftMethod.Length,
                    geometryType = SurfaceDraftParams.GeometryType.Surface,
                    length = depth,
                    angle = Mastercam.Math.VectorManager.RadiansToDegrees(finishAngle),
                    draftDirection = SurfaceDraftParams.DraftDirection.Defined
                };

                foreach (var chain in selectedCutChain)
                {
                    chain.Direction = ChainDirectionType.Clockwise;
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
                    chain.Direction = ChainDirectionType.Clockwise;
                    var mainGeoSide1 = chain.OffsetChain2D(OffsetSideType.Right, .002, OffsetRollCornerType.None, .5, false, .005, false);
                    var mainGeoResult1 = SearchManager.GetResultGeometry();
                    foreach (var entity in mainGeoResult1)
                    {
                        entity.Color = mainGeo;
                        entity.Level = mainGeo;
                        entity.Selected = false;
                        levelTenList1.Add(entity.GetEntityID());
                        entity.Commit();
                    }
                    foreach (var entity1 in mainGeoResult1)
                    {
                        if (entity1 is LineGeometry line1)
                        {
                            foreach (var entity2 in mainGeoResult1)
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
                                            levelTenList1.Add(newFillet.GetEntityID());
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
                    foreach (var entity in levelTenList1)
                    {
                        var entityGeo = Geometry.RetrieveEntity(entity);
                        entityGeo.Selected = true;
                        entityGeo.Commit();
                    }
                    var levelTenGeo = SearchManager.GetSelectedGeometry();
                    var thisChain10 = ChainManager.ChainGeometry(levelTenGeo);
                    foreach (var draftChain10 in thisChain10)
                    {
                        draftChain10.Direction = ChainDirectionType.Clockwise;
                        var draftSurface10 = SurfaceDraftInterop.CreateDrafts(draftChain10, roughSurfaceDraftParams1, false, 1);
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
                    var mainGeoSide2 = chain.OffsetChain2D(OffsetSideType.Left, .002, OffsetRollCornerType.None, .5, false, .005, false);
                    var mainGeoResult2 = SearchManager.GetResultGeometry();
                    foreach (var entity in mainGeoResult2)
                    {
                        entity.Color = mainGeo;
                        entity.Level = mainGeo;
                        entity.Selected = false;
                        levelTenList2.Add(entity.GetEntityID());
                        entity.Commit();
                    }
                    foreach (var entity1 in mainGeoResult2)
                    {
                        if (entity1 is LineGeometry line1)
                        {
                            foreach (var entity2 in mainGeoResult2)
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
                                            levelTenList2.Add(newFillet.GetEntityID());
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
                    foreach (var entity in levelTenList2)
                    {
                        var entityGeo = Geometry.RetrieveEntity(entity);
                        entityGeo.Selected = true;
                        entityGeo.Commit();
                    }
                    var levelTenGeo2 = SearchManager.GetSelectedGeometry();
                    var thisChain102 = ChainManager.ChainGeometry(levelTenGeo2);
                    foreach (var draftChain10 in thisChain102)
                    {
                        draftChain10.Direction = ChainDirectionType.Clockwise;
                        var draftSurface10 = SurfaceDraftInterop.CreateDrafts(draftChain10, roughSurfaceDraftParams2, false, 1);
                        foreach (var surface10 in draftSurface10)
                        {
                            if (Geometry.RetrieveEntity(surface10) is Geometry roughDraftSurface10)
                            {
                                roughDraftSurface10.Level = roughSurf;
                                roughDraftSurface10.Commit();
                            }
                        }
                    }
                    ///////////////////////////////
                    var cleanOutSide1 = chain.OffsetChain2D(OffsetSideType.Right, .0025, OffsetRollCornerType.None, .5, false, .005, false);
                    var cleanOutSide2 = chain.OffsetChain2D(OffsetSideType.Left, .0025, OffsetRollCornerType.None, .5, false, .005, false);
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
                    var finishSurfSide1 = chain.OffsetChain2D(OffsetSideType.Right, .0005, OffsetRollCornerType.None, .5, false, .005, false);
                    var finishSurfResult1 = SearchManager.GetResultGeometry();
                    foreach (var entity in finishSurfResult1)
                    {
                        entity.Color = finishSurf;
                        entity.Level = finishSurf;
                        entity.Selected = false;
                        level139List1.Add(entity.GetEntityID());
                        entity.Commit();
                    }
                    foreach (var entity1 in finishSurfResult1)
                    {
                        if (entity1 is LineGeometry line1)
                        {
                            foreach (var entity2 in finishSurfResult1)
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
                                            level139List1.Add(newFillet.GetEntityID());
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
                    foreach (var entity in level139List1)
                    {
                        var entityGeo = Geometry.RetrieveEntity(entity);
                        entityGeo.Selected = true;
                        entityGeo.Commit();
                    }
                    var level139Geo = SearchManager.GetSelectedGeometry();
                    var thisChain139 = ChainManager.ChainGeometry(level139Geo);
                    foreach (var draftChain139 in thisChain139)

                    {
                        draftChain139.Direction = ChainDirectionType.Clockwise;
                        var draftSurface139 = SurfaceDraftInterop.CreateDrafts(draftChain139, finishSurfaceDraftParams1, false, 1);
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
                    var finishSurfSide2 = chain.OffsetChain2D(OffsetSideType.Left, .0005, OffsetRollCornerType.None, .5, false, .005, false);
                    var finishSurfResult2 = SearchManager.GetResultGeometry();
                    foreach (var entity in finishSurfResult2)
                    {
                        entity.Color = finishSurf;
                        entity.Level = finishSurf;
                        entity.Selected = false;
                        level139List2.Add(entity.GetEntityID());
                        entity.Commit();
                    }
                    foreach (var entity1 in finishSurfResult2)
                    {
                        if (entity1 is LineGeometry line1)
                        {
                            foreach (var entity2 in finishSurfResult2)
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
                                            level139List2.Add(newFillet.GetEntityID());
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
                    foreach (var entity in level139List2)
                    {
                        var entityGeo = Geometry.RetrieveEntity(entity);
                        entityGeo.Selected = true;
                        entityGeo.Commit();
                    }
                    var level139Geo2 = SearchManager.GetSelectedGeometry();
                    var thisChain1392 = ChainManager.ChainGeometry(level139Geo2);
                    foreach (var draftChain139 in thisChain1392)

                    {
                        draftChain139.Direction = ChainDirectionType.Clockwise;
                        var draftSurface139 = SurfaceDraftInterop.CreateDrafts(draftChain139, finishSurfaceDraftParams2, false, 1);
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
            GraphicsManager.Repaint(true);
            return MCamReturn.NoErrors;
        }
    }
}