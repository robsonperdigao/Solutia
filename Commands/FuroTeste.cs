using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;

namespace Solutia.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class FuroTeste : IExternalCommand
    {
        public Result Execute(
            ExternalCommandData commandData,
            ref string message,
            ElementSet elements)
        {
            // Obter o documento e a interface de usuário do Revit
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            try
            {
                // Selecionar os elementos estruturais (por exemplo, paredes, vigas)
                List<Element> structuralElements = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_StructuralFraming)
                    .WhereElementIsNotElementType()
                    .ToElements() as List<Element>;

                // Selecionar os elementos de conduítes
                List<Element> conduitElements = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_Conduit)
                    .WhereElementIsNotElementType()
                    .ToElements() as List<Element>;

                // Verificar se há elementos estruturais e conduítes no modelo
                if (structuralElements.Count == 0 || conduitElements.Count == 0)
                {
                    TaskDialog.Show("Erro", "Nenhum elemento estrutural ou conduíte foi encontrado no modelo.");
                    return Result.Failed;
                }

                // Chamar a função de verificação de interferências
                List<string> interferenceResults = VerificaFuro.CheckInterferences(doc, structuralElements, conduitElements);

                // Exibir o resultado da verificação
                if (interferenceResults.Count > 0)
                {
                    TaskDialog.Show("Interferências Encontradas", string.Join(Environment.NewLine, interferenceResults));
                }
                else
                {
                    TaskDialog.Show("Interferências", "Nenhuma interferência foi encontrada entre conduítes e elementos estruturais.");
                }

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                // Exibir uma mensagem de erro caso algo dê errado
                TaskDialog.Show("Erro", ex.Message);
                return Result.Failed;
            }
        }
    }



}

