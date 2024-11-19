using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.Attributes;

namespace Solutia.Commands.PLU
{
    [Transaction(TransactionMode.Manual)]
    public class Intersec : IExternalCommand
    {
        public Result Execute(
            ExternalCommandData commandData,
            ref string message,
            ElementSet elements)
        {
            // Obter o documento e a vista ativa
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;
            View activeView = doc.ActiveView;

            try
            {
                // Coletar todas as tubulações na vista ativa
                FilteredElementCollector collector = new FilteredElementCollector(doc, activeView.Id);
                ICollection<Element> pipes = collector
                    .OfCategory(BuiltInCategory.OST_PipeCurves)
                    .WhereElementIsNotElementType()
                    .ToElements();

                // Obter as curvas das tubulações
                List<Curve> curves = new List<Curve>();
                foreach (Element pipe in pipes)
                {
                    Curve curve = (pipe.Location as LocationCurve)?.Curve;
                    if (curve != null)
                    {
                        curves.Add(curve);
                    }
                }

                // Verificar interseção entre todas as tubulações
                List<XYZ> intersectionPoints = new List<XYZ>();
                for (int i = 0; i < curves.Count; i++)
                {
                    for (int j = i + 1; j < curves.Count; j++)
                    {
                        XYZ intersectionPoint = GetIntersectionPoint(curves[i], curves[j]);
                        if (intersectionPoint != null)
                        {
                            intersectionPoints.Add(intersectionPoint);
                        }
                    }
                }

                // Mostrar os pontos de interseção encontrados
                if (intersectionPoints.Count > 0)
                {
                    string pointsMessage = "Pontos de interseção encontrados:\n";
                    foreach (XYZ point in intersectionPoints)
                    {
                        pointsMessage += $"({point.X}, {point.Y}, {point.Z})\n";
                    }
                    TaskDialog.Show("Interseções Encontradas", pointsMessage);
                }
                else
                {
                    TaskDialog.Show("Sem Interseções", "Nenhuma interseção foi encontrada entre as tubulações na vista atual.");
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Erro", $"Ocorreu um erro: {ex.Message}");
                return Result.Failed;
            }

            return Result.Succeeded;
        }

        private XYZ GetIntersectionPoint(Curve curve1, Curve curve2)
        {
            IntersectionResultArray results;
            if (curve1.Intersect(curve2, out results) == SetComparisonResult.Overlap && results.Size > 0)
            {
                return results.get_Item(0).XYZPoint;
            }
            return null;
        }
    }
}
