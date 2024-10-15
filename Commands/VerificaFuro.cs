using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Solutia.Commands
{
    public class VerificaFuro
    {
        public static List<string> CheckInterferences(
            Document doc,
            IEnumerable<Element> structuralElements,
            IEnumerable<Element> targetElements,
            string familyName = "Furo Retangular",
            double diameterOffset = 0.05)
        {
            List<string> interferenceResults = new List<string>();

            // Obtenha a família de furo retangular
            FamilySymbol familySymbol = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol))
                .OfCategory(BuiltInCategory.OST_GenericModel)
                .FirstOrDefault(x => x.Name == familyName) as FamilySymbol;

            if (familySymbol == null)
            {
                TaskDialog.Show("Erro", $"A família '{familyName}' não foi encontrada.");
                return interferenceResults;
            }

            if (!familySymbol.IsActive)
            {
                familySymbol.Activate();
            }

            using (Transaction trans = new Transaction(doc, "Inserir Furo Retangular"))
            {
                trans.Start();

                foreach (Element targetElement in targetElements)
                {
                    foreach (Element structuralElement in structuralElements)
                    {
                        ElementIntersectsElementFilter intersectionFilter = new ElementIntersectsElementFilter(structuralElement);

                        if (intersectionFilter.PassesFilter(targetElement))
                        {
                            BoundingBoxXYZ targetBB = targetElement.get_BoundingBox(null);
                            BoundingBoxXYZ structBB = structuralElement.get_BoundingBox(null);

                            if (targetBB != null && structBB != null)
                            {
                                XYZ minPoint = new XYZ(
                                    Math.Max(targetBB.Min.X, structBB.Min.X),
                                    Math.Max(targetBB.Min.Y, structBB.Min.Y),
                                    Math.Max(targetBB.Min.Z, structBB.Min.Z));

                                XYZ maxPoint = new XYZ(
                                    Math.Min(targetBB.Max.X, structBB.Max.X),
                                    Math.Min(targetBB.Max.Y, structBB.Max.Y),
                                    Math.Min(targetBB.Max.Z, structBB.Max.Z));

                                XYZ intersectionPoint = (minPoint + maxPoint) / 2;

                                Parameter diameterParam = targetElement.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM);
                                if (diameterParam != null)
                                {
                                    double targetDiameter = diameterParam.AsDouble() + (diameterOffset * 3.2808398950);
                                    double structuralThickness = Math.Min(structBB.Max.X - structBB.Min.X, structBB.Max.Y - structBB.Min.Y);

                                    // Verifica se já existe um furo no ponto
                                    if (!HoleExistsAtPoint(doc, intersectionPoint, familyName))
                                    {
                                        FamilyInstance instance = doc.Create.NewFamilyInstance(intersectionPoint, familySymbol, StructuralType.NonStructural);
                                        instance.LookupParameter("Largura Viga")?.Set(structuralThickness);
                                        instance.LookupParameter("Largura do Furo")?.Set(targetDiameter);
                                        instance.LookupParameter("Altura do Furo")?.Set(targetDiameter);

                                        double offsetFromHost = Math.Abs(structBB.Max.Z - intersectionPoint.Z);
                                        instance.LookupParameter("H do Furo (Centro)")?.Set(offsetFromHost);

                                        // Verifica se o bounding box do elemento atende ao critério de rotação
                                        if ((targetBB.Max.X - targetBB.Min.X) > (targetBB.Max.Y - targetBB.Min.Y))
                                        {
                                            Line rotationAxis = Line.CreateBound(intersectionPoint, intersectionPoint + XYZ.BasisZ);
                                            ElementTransformUtils.RotateElement(doc, instance.Id, rotationAxis, Math.PI / 2);
                                        }

                                        interferenceResults.Add($"Interferência detectada: Elemento {targetElement.Id} com Elemento Estrutural {structuralElement.Id}");
                                    }
                                }
                            }
                        }
                    }
                }

                trans.Commit();
            }

            return interferenceResults;
        }

        // Função para verificar se já existe um furo no ponto
        private static bool HoleExistsAtPoint(Document doc, XYZ point, string familyName)
        {
            IEnumerable<FamilyInstance> existingHoles = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_GenericModel)
                .WhereElementIsNotElementType()
                .OfClass(typeof(FamilyInstance))
                .Cast<FamilyInstance>()
                .Where(f => f.Symbol.Name == familyName);

            foreach (FamilyInstance hole in existingHoles)
            {
                XYZ holeLocation = (hole.Location as LocationPoint).Point;
                if (holeLocation.IsAlmostEqualTo(point, tolerance: 0.01))
                {
                    return true;
                }
            }

            return false;
        }
    }
}

