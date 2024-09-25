using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB.Plumbing;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Mechanical;

namespace Solutia.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class TeTub : IExternalCommand
    {
        public Result Execute(
            ExternalCommandData commandData,
            ref string message,
            ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            UIDocument uiDoc = uiApp.ActiveUIDocument;
            Document doc = uiDoc.Document;

            // Coleta todas as tubulações visíveis na vista ativa
            List<MEPCurve> pipes = new FilteredElementCollector(doc, uiDoc.ActiveView.Id)
                                    .OfClass(typeof(Pipe))
                                    .Cast<MEPCurve>()
                                    .ToList();

            // Variável para armazenar as conexões criadas
            List<ElementId> fittings = new List<ElementId>();

            using (Transaction trans = new Transaction(doc, "Create Tee Fittings"))
            {
                trans.Start();

                // Itera sobre cada tubulação para encontrar endpoints que coincidam com outros pipes
                for (int i = 0; i < pipes.Count; i++)
                {
                    MEPCurve primaryPipe = pipes[i];
                    XYZ primaryStart = (primaryPipe.Location as LocationCurve).Curve.GetEndPoint(0);
                    XYZ primaryEnd = (primaryPipe.Location as LocationCurve).Curve.GetEndPoint(1);

                    for (int j = 0; j < pipes.Count; j++)
                    {
                        if (i == j) continue;

                        MEPCurve branchPipe = pipes[j];
                        XYZ branchStart = (branchPipe.Location as LocationCurve).Curve.GetEndPoint(0);
                        XYZ branchEnd = (branchPipe.Location as LocationCurve).Curve.GetEndPoint(1);

                        if (primaryStart.IsAlmostEqualTo(branchEnd) || primaryEnd.IsAlmostEqualTo(branchEnd))
                        {
                            try
                            {
                                double width = GetCurveWidth(branchPipe);

                                Line primaryLine = (primaryPipe.Location as LocationCurve).Curve as Line;
                                Line branchLine = (branchPipe.Location as LocationCurve).Curve as Line;

                                XYZ pointmid = primaryLine.Project(branchEnd).XYZPoint;
                                double len1 = primaryStart.DistanceTo(pointmid);
                                double len2 = len1 - width / 2;
                                double len3 = len1 + width / 2;

                                XYZ midstart = primaryLine.Evaluate(len2 / primaryLine.Length, true);
                                XYZ midend = primaryLine.Evaluate(len3 / primaryLine.Length, true);

                                bool toggle = false;
                                if (primaryStart.DistanceTo(pointmid) < primaryEnd.DistanceTo(pointmid))
                                {
                                    toggle = true;
                                    XYZ temppoint = primaryStart;
                                    primaryStart = primaryEnd;
                                    primaryEnd = temppoint;

                                    XYZ tempmid = midstart;
                                    midstart = midend;
                                    midend = tempmid;
                                }

                                Connector startconn = null, endconn = null;
                                foreach (Connector conn in (primaryPipe.ConnectorManager as ConnectorManager).Connectors)
                                {
                                    if (conn.Origin.IsAlmostEqualTo(primaryStart))
                                        startconn = conn;
                                    else if (conn.Origin.IsAlmostEqualTo(primaryEnd))
                                        endconn = conn;
                                }

                                FamilyInstance otherfitting = null;
                                foreach (Connector conn in endconn.AllRefs)
                                {
                                    if (conn.IsConnectedTo(endconn))
                                    {
                                        if (conn.Owner is FamilyInstance)
                                            otherfitting = conn.Owner as FamilyInstance;
                                        endconn.DisconnectFrom(conn);
                                    }
                                }

                                if (toggle)
                                    (primaryPipe.Location as LocationCurve).Curve = Line.CreateBound(midstart, primaryStart);
                                else
                                    (primaryPipe.Location as LocationCurve).Curve = Line.CreateBound(primaryStart, midstart);
                                doc.Regenerate();

                                XYZ direction = new XYZ(
                                    (midstart.X - primaryEnd.X) * -1,
                                    (midstart.Y - primaryEnd.Y) * -1,
                                    (midstart.Z - primaryEnd.Z) * -1);
                                ElementId newElemId = ElementTransformUtils.CopyElement(doc, primaryPipe.Id, direction).First();
                                MEPCurve secondaryPipe = doc.GetElement(newElemId) as MEPCurve;
                                doc.Regenerate();

                                (secondaryPipe.Location as LocationCurve).Curve = Line.CreateBound(primaryEnd, midend);
                                doc.Regenerate();

                                List<Connector> connectors = ClosestConnectors(primaryPipe, branchPipe, secondaryPipe);
                                if (connectors.Count == 3)
                                {
                                    FamilyInstance fitting = doc.Create.NewTeeFitting(connectors[0], connectors[1], connectors[2]);
                                    fittings.Add(fitting.Id);

                                    if (otherfitting != null)
                                    {
                                        foreach (Connector conn in secondaryPipe.ConnectorManager.Connectors)
                                        {
                                            foreach (Connector conn2 in otherfitting.MEPModel.ConnectorManager.Connectors)
                                            {
                                                if (conn.Origin.IsAlmostEqualTo(conn2.Origin))
                                                {
                                                    conn.ConnectTo(conn2);
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                TaskDialog.Show("Error", ex.Message);
                            }
                        }
                    }
                }

                trans.Commit();
            }

            TaskDialog.Show("Fittings Criados", string.Join("\n", fittings.Select(f => f.ToString())));
            return Result.Succeeded;
        }

        private double GetCurveWidth(MEPCurve curve)
        {
            if (curve is Pipe pipe)
                return pipe.Diameter;
            else if (curve is Duct duct)
                return duct.Width;
            else
                throw new InvalidOperationException("Unsupported MEPCurve type");
        }

        private List<Connector> ClosestConnectors(MEPCurve pipe1, MEPCurve pipe2, MEPCurve pipe3)
        {
            ConnectorSet conns1 = pipe1.ConnectorManager.Connectors;
            ConnectorSet conns2 = pipe2.ConnectorManager.Connectors;
            ConnectorSet conns3 = pipe3.ConnectorManager.Connectors;

            double dist1 = double.MaxValue;
            double dist2 = double.MaxValue;
            Connector c1 = null, d1 = null, e1 = null;

            foreach (Connector c in conns1)
            {
                foreach (Connector d in conns3)
                {
                    double conndist = c.Origin.DistanceTo(d.Origin);
                    if (conndist < dist1)
                    {
                        dist1 = conndist;
                        c1 = c;
                        d1 = d;
                    }
                }
                foreach (Connector e in conns2)
                {
                    double conndist = c.Origin.DistanceTo(e.Origin);
                    if (conndist < dist2)
                    {
                        dist2 = conndist;
                        e1 = e;
                    }
                }
            }

            return new List<Connector> { c1, d1, e1 };
        }
    }
}


