using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using System.IO;
using System.Windows.Forms;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.UI.Selection;
using System.Collections.Generic;
using System;

namespace Solutia.Commands
{
    [Transaction(TransactionMode.ReadOnly)]
    public class ColetaDadosFiac : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            Reference selectedPanelRef = uiDoc.Selection.PickObject(ObjectType.Element, new ElectricalEquipmentSelectionFilter(), "Selecione o painel elétrico");
            if (selectedPanelRef == null)
            {
                message = "Nenhum painel elétrico foi selecionado.";
                return Result.Failed;
            }

            Element selectedPanel = doc.GetElement(selectedPanelRef);
            List<ElementId> connectedElementIds = GetElementsConnectedToPanel(doc, selectedPanel);

            // Mostra a lista de elementos e pergunta se o usuário deseja salvar o arquivo
            TaskDialogResult dialogResult = ShowConnectedElementsAndAskToSave(connectedElementIds, doc);

            if (dialogResult == TaskDialogResult.Yes)
            {
                ExportToCsv(connectedElementIds);
            }

            return Result.Succeeded;
        }

        private List<ElementId> GetElementsConnectedToPanel(Document doc, Element panel)
        {
            List<ElementId> connectedElementIds = new List<ElementId>();
            FamilyInstance panelInstance = panel as FamilyInstance;
            if (panelInstance == null) return connectedElementIds;

            var electricalSystems = panelInstance.MEPModel.GetElectricalSystems();
            if (electricalSystems == null) return connectedElementIds;

            foreach (ElectricalSystem electricalSystem in electricalSystems)
            {
                foreach (ElementId id in electricalSystem.Elements)
                {
                    Element element = doc.GetElement(id);
                    if (element != null && element.Category != null && element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_ElectricalFixtures)
                    {
                        connectedElementIds.Add(element.Id);
                    }
                }
            }

            return connectedElementIds;
        }

        private void ExportToCsv(List<ElementId> elementIds)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                sfd.Title = "Salvar Arquivo CSV";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    string filePath = sfd.FileName;

                    try
                    {
                        using (StreamWriter sw = new StreamWriter(filePath))
                        {
                            sw.WriteLine("ElementId");

                            foreach (var id in elementIds)
                            {
                                sw.WriteLine(id.IntegerValue);
                            }
                        }

                        TaskDialog.Show("Exportação", "Os dados foram exportados com sucesso para " + filePath);
                    }
                    catch (Exception ex)
                    {
                        TaskDialog.Show("Erro", "Ocorreu um erro ao exportar os dados para o arquivo CSV: " + ex.Message);
                    }
                }
            }
        }

        private TaskDialogResult ShowConnectedElementsAndAskToSave(List<ElementId> elementIds, Document doc)
        {
            if (elementIds.Count == 0)
            {
                return TaskDialog.Show("Elementos Conectados", "Nenhum elemento foi encontrado conectado ao painel.", TaskDialogCommonButtons.Close);
            }

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("Os seguintes elementos estão conectados ao painel:");

            foreach (var id in elementIds)
            {
                Element element = doc.GetElement(id);
                sb.AppendLine($"ID: {id.IntegerValue} - Nome: {element.Name}");
            }

            sb.AppendLine("\nDeseja salvar esses elementos em um arquivo CSV?");

            TaskDialog dialog = new TaskDialog("Elementos Conectados")
            {
                MainInstruction = "Elementos Conectados ao Painel",
                MainContent = sb.ToString(),
                CommonButtons = TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No,
                DefaultButton = TaskDialogResult.Yes
            };

            return dialog.Show();
        }

        private class ElectricalEquipmentSelectionFilter : ISelectionFilter
        {
            public bool AllowElement(Element elem)
            {
                return elem.Category != null && elem.Category.Id.IntegerValue == (int)BuiltInCategory.OST_ElectricalEquipment;
            }

            public bool AllowReference(Reference reference, XYZ position)
            {
                return true;
            }
        }
    }
}
