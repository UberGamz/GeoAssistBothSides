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
using Mastercam.GeometryUtility.Types;
using System;
using System.Drawing;

namespace _GeoAssistBothSides
{
    public class GeoAssistBothSides : Mastercam.App.NetHook3App
    {
        public Mastercam.App.Types.MCamReturn GeoAssistBothSidesRun(Mastercam.App.Types.MCamReturn notused)
        {
            var depth = -0.100;
            var roughAngle = 15.0;
            var selectedCutChain = ChainManager.GetMultipleChains("Select Geometry");
            if (selectedCutChain == null)
            {
                return MCamReturn.NoErrors;
            }
            var depthDialog = DialogManager.AskForNumber("Enter Depth", ref depth);
            if (depthDialog == 0)
            {
                return MCamReturn.NoErrors;
            }
            var roughAngleDialog = DialogManager.AskForAngle("Enter Rough Angle", ref roughAngle);
            if (roughAngleDialog == 0)
            {
                return MCamReturn.NoErrors;
            }
            void offsetCutchain()
            {
                void deSelect()
                {
                    var selectedGeo = SearchManager.GetGeometry();
                    foreach (var entity in selectedGeo)
                    {
                        entity.Retrieve();
                        entity.Selected = false;
                        entity.Commit();
                    }
                }
                var chainList1 = new List<Chain>();
                SelectionManager.UnselectAllGeometry();
                LevelsManager.RefreshLevelsManager();
                GraphicsManager.Repaint(true);

                SurfaceDraftParams roughSurfaceDraftParamsRight = new SurfaceDraftParams
                {
                    draftMethod = SurfaceDraftParams.DraftMethod.Length,
                    geometryType = SurfaceDraftParams.GeometryType.Surface,
                    length = depth,
                    angle = Mastercam.Math.VectorManager.RadiansToDegrees(-roughAngle),
                    draftDirection = SurfaceDraftParams.DraftDirection.Defined
                };
                SurfaceDraftParams roughSurfaceDraftParamsLeft = new SurfaceDraftParams
                {
                    draftMethod = SurfaceDraftParams.DraftMethod.Length,
                    geometryType = SurfaceDraftParams.GeometryType.Surface,
                    length = depth,
                    angle = Mastercam.Math.VectorManager.RadiansToDegrees(roughAngle),
                    draftDirection = SurfaceDraftParams.DraftDirection.Defined
                };
                ChainManager.ChainTolerance = 0.0005;
                void ProcessChainsWithSurfaces(List<Chain> inputChainList, OffsetSideType offsetSide, double offsetDistance, int geoLevel)
                {
                    OffsetRollCornerType cornerType = OffsetRollCornerType.All;
                    double cornerRadius = offsetDistance;
                    foreach (var chain in inputChainList)
                    {
                        var geoList = new List<Geometry>();
                        chain.OffsetChain2D(offsetSide, offsetDistance, cornerType, cornerRadius, false, offsetDistance/2, false);
                        var resultChainGeo = SearchManager.GetResultGeometry();

                        foreach (var entity in resultChainGeo)
                        {
                            entity.Color = geoLevel;
                            entity.Level = geoLevel;
                            entity.Selected = false;
                            entity.Commit();
                            geoList.Add(entity);
                        }

                        var geoIDs = new List<int>();
                        foreach (var entity in geoList)
                        {
                            var entityID = entity.GetEntityID();
                            geoIDs.Add(entityID);
                        }
                        var thisChainArray = ChainManager.ChainGeometry(geoList.ToArray());
                        var thisChain = thisChainArray[0];
                        SurfaceDraftParams roughDraftParams;

                        //var newFillets = GeometryManipulationManager.FilletChain(thisChain, offsetDistance, geoLevel, geoLevel, FilletDirectionType.Either);
                        //foreach (var entity in newFillets)
                        //{
                        //    geoList.Add(entity);
                        //}
                        //thisChainArray = ChainManager.ChainGeometry(geoList.ToArray());
                        //thisChain = thisChainArray[0];

                        if (offsetSide == OffsetSideType.Right)
                        {
                            roughDraftParams = roughSurfaceDraftParamsLeft;
                        }
                        else
                        {
                            roughDraftParams = roughSurfaceDraftParamsRight;
                        }

                        var draftSurface = SurfaceDraftInterop.CreateDrafts(thisChain, roughDraftParams, false, 1);
                        foreach (var surface in draftSurface)
                        {
                            if (Geometry.RetrieveEntity(surface) is Geometry roughDraftSurface)
                            {
                                roughDraftSurface.Level = 138;
                                roughDraftSurface.Commit();
                            }
                        }
                        GraphicsManager.ClearColors(new GroupSelectionMask(true));
                        SelectionManager.UnselectAllGeometry();
                        GraphicsManager.Repaint(true);
                    }
                }
                void ProcessChainsWithoutSurfaces(List<Chain> inputChainList, OffsetSideType offsetSide, double offsetDistance, int geoLevel)
                {
                    OffsetRollCornerType cornerType = OffsetRollCornerType.All;
                    double cornerRadius = offsetDistance;
                    foreach (var chain in inputChainList)
                    {
                        var geoList = new List<Geometry>();
                        chain.OffsetChain2D(offsetSide, offsetDistance, cornerType, cornerRadius, false, offsetDistance / 2, false);
                        var resultChainGeo = SearchManager.GetResultGeometry();

                        foreach (var entity in resultChainGeo)
                        {
                            entity.Color = geoLevel;
                            entity.Level = geoLevel;
                            entity.Selected = false;
                            entity.Commit();
                            geoList.Add(entity);
                        }

                        var geoIDs = new List<int>();
                        foreach (var entity in geoList)
                        {
                            var entityID = entity.GetEntityID();
                            geoIDs.Add(entityID);
                        }
                        var thisChainArray = ChainManager.ChainGeometry(geoList.ToArray());
                        var thisChain = thisChainArray[0];

                        //var newFillets = GeometryManipulationManager.FilletChain(thisChain, offsetDistance, geoLevel, geoLevel, FilletDirectionType.Either);
                        //foreach (var entity in newFillets)
                        //{
                        //    geoList.Add(entity);
                        //}
                        //thisChainArray = ChainManager.ChainGeometry(geoList.ToArray());
                        //thisChain = thisChainArray[0];

                        GraphicsManager.ClearColors(new GroupSelectionMask(true));
                        SelectionManager.UnselectAllGeometry();
                        GraphicsManager.Repaint(true);
                    }
                }

                foreach (var chain in selectedCutChain)
                {
                    if (chain.IsClosed) { continue; }
                    else
                    {
                        DialogManager.Error("The chain is not closed, try again", "Chain Not Closed Error");
                        return;
                    }
                }
                foreach (var chain in selectedCutChain)
                {
                    var chainGeos = ChainManager.GetGeometryInChain(chain);
                    foreach (var entity in chainGeos)
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
                    chainList1.Add(chain);
                    deSelect();
                }
                ProcessChainsWithSurfaces(chainList1, OffsetSideType.Left, 0.002, 10);
                ProcessChainsWithSurfaces(chainList1, OffsetSideType.Right, 0.002, 10);
                ProcessChainsWithoutSurfaces(chainList1, OffsetSideType.Left, 0.0025, 12);
                ProcessChainsWithoutSurfaces(chainList1, OffsetSideType.Right, 0.0025, 12);
            }
            void boundingBox()
            {
                var sideGeo = new List<Geometry>();
                var surfaceGeo = SearchManager.GetSurfaceGeometry(138);
                BoundingBoxCommonParams CommonData = new BoundingBoxCommonParams {
                    CreateLinesArcs = true
                };
                BoundingBoxRectangularParams RectangularData = new BoundingBoxRectangularParams
                {
                    ExpandXMinus = .25,
                    ExpandXPlus = .25,
                    ExpandYMinus = .25,
                    ExpandYPlus = .25,
                    ExpandZPlus = depth
                };
                var boundingBoxGeo = GeometryCreationManager.RectangularBoundingBox(CommonData, RectangularData, surfaceGeo);
                foreach (var boxGeo in boundingBoxGeo)
                {
                    boxGeo.Level = 140;
                    boxGeo.Color = 140;
                    boxGeo.Commit();
                }
                foreach(var boxGeo in boundingBoxGeo)
                {
                    if (boxGeo is LineGeometry boxLine)
                    {
                        if (boxLine.EndPoint1.x == boxLine.EndPoint2.x) {
                            sideGeo.Add(boxLine);
                        }
                    }
                }
                var chainDetails = new Mastercam.Database.Interop.ChainDetails();// Preps the ChainDetails plugin
                var boxChain = ChainManager.ChainGeometry(sideGeo.ToArray());

                foreach (var sideChain in boxChain)
                {
                    var chainData = chainDetails.GetData(sideChain);
                    var startPoint = chainData.StartPoint;
                    var endPoint = chainData.EndPoint;
                    if (startPoint.y < endPoint.y)
                    {
                        sideChain.Direction = Mastercam.Database.Types.ChainDirectionType.Clockwise;
                    }
                    else
                    {
                        sideChain.Direction = Mastercam.Database.Types.ChainDirectionType.CounterClockwise;
                    }
                }
                var surfaces = Mastercam.IO.Interop.SelectionManager.CreateRuledSurface(boxChain);

                foreach (var surface in surfaces)
                {
                    var surfaceID = Geometry.RetrieveEntity(surface) as SurfaceGeometry;
                    surfaceID.Color = 8;
                    surfaceID.Level = 140;
                    surfaceID.Translate(new Point3D(0, 0, 0), new Point3D(0, 0, depth), new MCView(), new MCView());
                    surfaceID.Commit();
                }
            }
            offsetCutchain();
            boundingBox();
            GraphicsManager.Repaint(true);
            return MCamReturn.NoErrors;
        }
    }
}


