using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.DB.IFC;

namespace Solutia.Commands.SHA
{
    [Transaction(TransactionMode.Manual)]
    public class FuroELE : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            // Permitir que o usuário selecione um vínculo de Revit
            Reference r = uidoc.Selection.PickObject(ObjectType.Element, "Selecione um vínculo de Revit.");
            Element selectedElement = doc.GetElement(r);

            if (!(selectedElement is RevitLinkInstance))
            {
                TaskDialog.Show("Erro", "Por favor, selecione um vínculo de Revit.");
                return Result.Failed;
            }

            // Obtenha o documento do vínculo
            RevitLinkInstance revitLink = selectedElement as RevitLinkInstance;
            Document linkDoc = revitLink.GetLinkDocument();

            if (linkDoc == null)
            {
                TaskDialog.Show("Erro", "Não foi possível acessar o documento do vínculo.");
                return Result.Failed;
            }

            // Coletar elementos estruturais no vínculo
            FilteredElementCollector structuralElements = new FilteredElementCollector(linkDoc)
                .OfCategory(BuiltInCategory.OST_StructuralFraming)
                .WhereElementIsNotElementType();

            // Coletar todas as tubulações no modelo principal
            FilteredElementCollector pipeElements = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_PipeCurves)
                .WhereElementIsNotElementType();

            // Coletar todos os eletrodutos no modelo principal
            FilteredElementCollector conduitElements = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Conduit)
                .WhereElementIsNotElementType();

            // Coletar todas as eletrocalhas no modelo principal
            FilteredElementCollector cabletrayElements = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_CableTray)
                .WhereElementIsNotElementType();

            // Lista para armazenar interferências detectadas
            List<string> interferenceResults = new List<string>();

            // Iniciar uma transação para inserir a família no modelo
            using (Transaction trans = new Transaction(doc, "Inserir Furo Retangular"))
            {
                trans.Start();

                foreach (Element pipe in pipeElements)
                {
                    foreach (Element structuralElement in structuralElements)
                    {
                        ElementIntersectsElementFilter intersectionFilter = new ElementIntersectsElementFilter(structuralElement);

                        if (intersectionFilter.PassesFilter(pipe))
                        {
                            // Obter a interseção usando BoundingBox
                            BoundingBoxXYZ pipeBB = pipe.get_BoundingBox(null);
                            BoundingBoxXYZ structBB = structuralElement.get_BoundingBox(null);

                            if (pipeBB != null && structBB != null)
                            {
                                XYZ minPoint = new XYZ(
                                    Math.Max(pipeBB.Min.X, structBB.Min.X),
                                    Math.Max(pipeBB.Min.Y, structBB.Min.Y),
                                    Math.Max(pipeBB.Min.Z, structBB.Min.Z));

                                XYZ maxPoint = new XYZ(
                                    Math.Min(pipeBB.Max.X, structBB.Max.X),
                                    Math.Min(pipeBB.Max.Y, structBB.Max.Y),
                                    Math.Min(pipeBB.Max.Z, structBB.Max.Z));

                                // Calcular o ponto médio da interseção
                                XYZ intersectionPoint = (minPoint + maxPoint) / 2;

                                // Inserir a família "Furo Retangular" no ponto de interseção
                                FamilySymbol familySymbol = new FilteredElementCollector(doc)
                                    .OfClass(typeof(FamilySymbol))
                                    .OfCategory(BuiltInCategory.OST_GenericModel)
                                    .FirstOrDefault(x => x.Name == "Furo Retangular") as FamilySymbol;

                                if (familySymbol != null)
                                {
                                    if (!familySymbol.IsActive)
                                    {
                                        familySymbol.Activate();
                                    }

                                    // Calcular o diâmetro da tubulação
                                    Parameter diameterParam = pipe.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM);
                                    if (diameterParam != null)
                                    {
                                        // Converta o diâmetro de milímetros para metros e adicione 0,05 metros (5 cm)
                                        double pipeDiameter = diameterParam.AsDouble() + (0.05 * 3.2808398950);

                                        // Calcular a espessura do elemento estrutural
                                        double structBBX = structBB.Max.X - structBB.Min.X;
                                        double structBBY = structBB.Max.Y - structBB.Min.Y;
                                        double structBBZ = structBB.Max.Z - structBB.Min.Z;
                                        double structuralThickness = Math.Min(structBBX, structBBY);

                                        IEnumerable<FamilyInstance> existingHoles = new FilteredElementCollector(doc)
                                            .OfCategory(BuiltInCategory.OST_GenericModel)
                                            .WhereElementIsNotElementType()
                                            .OfClass(typeof(FamilyInstance))
                                            .Cast<FamilyInstance>()
                                            .Where(f => f.Symbol.Name == "Furo Retangular");

                                        bool holeExists = existingHoles.Cast<FamilyInstance>().Any(f =>
                                        {
                                            XYZ furoLocation = (f.Location as LocationPoint).Point;
                                            return furoLocation.IsAlmostEqualTo(intersectionPoint, tolerance: 0.01); // Pode ajustar a tolerância se necessário
                                        });

                                        if (!holeExists)
                                        {
                                            // Criar uma instância da família na coordenada de interseção
                                            FamilyInstance instance = doc.Create.NewFamilyInstance(intersectionPoint, familySymbol, StructuralType.NonStructural);

                                            // Definir os parâmetros da família
                                            instance.LookupParameter("Largura Viga")?.Set(structuralThickness);
                                            instance.LookupParameter("Largura do Furo")?.Set(pipeDiameter);
                                            instance.LookupParameter("Altura do Furo")?.Set(pipeDiameter);

                                            double structuralHeight = GetElementHeight(structuralElement);
                                            // Inserir o valor do offset no parâmetro "H do Furo (Centro)"
                                            double offsetFromHost = Math.Abs(structBB.Max.Z - intersectionPoint.Z);
                                            instance.LookupParameter("H do Furo (Centro)")?.Set(offsetFromHost);

                                            // Verificar se o bounding box da tubulação atende ao critério para rotação
                                            if ((pipeBB.Max.X - pipeBB.Min.X) > (pipeBB.Max.Y - pipeBB.Min.Y))
                                            {
                                                Line rotationAxis = Line.CreateBound(intersectionPoint, intersectionPoint + XYZ.BasisZ);
                                                ElementTransformUtils.RotateElement(doc, instance.Id, rotationAxis, Math.PI / 2); // Rotação de 90 graus (π/2 radianos)
                                            }

                                            string result = $"Interferência detectada: Tubulação {pipe.Id} com Elemento Estrutural {structuralElement.Id}\n" +
                                                            $"Ponto de Interseção: Z = {intersectionPoint.Z / 3.2808398950}\n" +
                                                            $"Elevação da Viga: {structBB.Max.Z / 3.2808398950}\n" +
                                                            $"Altura da Viga FORMULA: {Math.Abs(structuralHeight) / 3.2808398950}\n" +
                                                            $"Altura da Viga structBBZ: {structBBZ / 3.2808398950}\n" +
                                                            $"H do Furo (Centro): {offsetFromHost / 3.2808398950}";
                                            interferenceResults.Add(result);
                                        }
                                    }
                                }
                                else
                                {
                                    TaskDialog.Show("Erro", "A família 'Furo Retangular' não foi encontrada.");
                                    trans.RollBack();
                                    return Result.Failed;
                                }
                            }
                        }
                    }
                }

                trans.Commit();
            }

            // Exibir resultados
            if (interferenceResults.Count > 0)
            {
                TaskDialog.Show("Interferências Encontradas", string.Join("\n\n", interferenceResults));
            }
            else
            {
                TaskDialog.Show("Nenhuma Interferência", "Nenhuma interferência foi encontrada entre tubulações e elementos estruturais.");
            }

            return Result.Succeeded;
        }
        public static double GetElementHeight(Element element)
        {
            // Inicializar valores para altura mínima e máxima
            double minZ = double.MaxValue;
            double maxZ = double.MinValue;

            // Obter a geometria do elemento
            Options geomOptions = new Options();
            geomOptions.ComputeReferences = true;
            geomOptions.IncludeNonVisibleObjects = true;
            geomOptions.DetailLevel = ViewDetailLevel.Fine;

            GeometryElement geomElement = element.get_Geometry(geomOptions);

            // Iterar através dos objetos na geometria
            foreach (GeometryObject geomObj in geomElement)
            {
                if (geomObj is Solid solid)
                {
                    // Iterar através das faces do Solid
                    foreach (Face face in solid.Faces)
                    {
                        Mesh mesh = face.Triangulate();

                        // Iterar através dos vértices da malha para encontrar Z mínimo e máximo
                        foreach (XYZ vertex in mesh.Vertices)
                        {
                            if (vertex.Z < minZ)
                            {
                                minZ = vertex.Z;
                            }
                            if (vertex.Z > maxZ)
                            {
                                maxZ = vertex.Z;
                            }
                        }
                    }
                }
            }

            // Calcular a altura como a diferença entre maxZ e minZ
            double height = maxZ - minZ;

            return height;
        }
    }
}

