﻿using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Structure;

namespace Solutia.Commands.SHA
{
    [Transaction(TransactionMode.Manual)]
    public class AutoFuroOrig : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            View activeView = doc.ActiveView;

            // Verificar se a vista ativa é válida para a operação
            if (activeView == null || !(activeView is ViewPlan))
            {
                TaskDialog.Show("Erro", "Apenas vistas do tipo 'ViewPlan' são compatíveis para a detecção de interferências.");
                return Result.Failed;
            }

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

            // Coletar elementos estruturais visíveis na vista ativa
            FilteredElementCollector structuralElements = new FilteredElementCollector(linkDoc, activeView.Id)
                .OfCategory(BuiltInCategory.OST_StructuralFraming).WhereElementIsNotElementType();

            // Coletar todas as tubulações visíveis na vista ativa
            FilteredElementCollector pipeElements = new FilteredElementCollector(doc, activeView.Id)
                .OfCategory(BuiltInCategory.OST_PipeCurves).WhereElementIsNotElementType();

            // Lista para armazenar interferências detectadas
            List<string> interferenceResults = new List<string>();

            Level level = activeView.GenLevel;
            double elevacaoNivel = level.Elevation;

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

                                    // Calcular o diâmetro da tubulação usando o parâmetro embutido
                                    Parameter diameterParam = pipe.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM);
                                    if (diameterParam != null)
                                    {
                                        // Converta o diâmetro de milímetros para metros e adicione 0,05 metros (5 cm)
                                        double pipeDiameter = diameterParam.AsDouble() + (0.05 * 3.2808398950);

                                        // Calcular a espessura do elemento estrutural
                                        double structuralThickness = structBB.Max.X - structBB.Min.X;

                                        // Criar uma instância da família na coordenada de interseção
                                        FamilyInstance instance = doc.Create.NewFamilyInstance(intersectionPoint, familySymbol, level, StructuralType.NonStructural);
                                        
                                        // Definir os parâmetros da família usando valores em metros
                                        instance.LookupParameter("Largura Viga")?.Set(structuralThickness);
                                        instance.LookupParameter("Largura do Furo")?.Set(pipeDiameter);
                                        instance.LookupParameter("Altura do Furo")?.Set(pipeDiameter);

                                        // Inserir o valor do offset no parâmetro "H do Furo (Centro)"
                                        double offsetFromHost = Math.Abs(intersectionPoint.Z - structBB.Max.Z);
                                        instance.LookupParameter("H do Furo (Centro)")?.Set(offsetFromHost);

                                        // Definir o offset from host como 0
                                        if (structBB.Max.Z > elevacaoNivel)
                                        {
                                            instance.get_Parameter(BuiltInParameter.INSTANCE_FREE_HOST_OFFSET_PARAM)?.Set(structBB.Max.Z - elevacaoNivel);
                                        }
                                        else if (structBB.Max.Z < elevacaoNivel)
                                        {
                                            instance.get_Parameter(BuiltInParameter.INSTANCE_FREE_HOST_OFFSET_PARAM)?.Set(elevacaoNivel - structBB.Max.Z);
                                        }
                                        else { instance.get_Parameter(BuiltInParameter.INSTANCE_FREE_HOST_OFFSET_PARAM)?.Set(0); }
                                        

                                        string result = $"Interferência detectada: Tubulação {pipe.Id} com Elemento Estrutural {structuralElement.Id}\n" +
                                                        $"Ponto de Interseção: X = {intersectionPoint.X:2}, Y = {intersectionPoint.Y:2}, Z = {intersectionPoint.Z:2}\n" +
                                                        $"Furo Retangular inserido com ID: {instance.Id}";
                                        interferenceResults.Add(result);
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
    }

}
