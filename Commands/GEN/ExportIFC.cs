using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.IO;
using System.Windows.Forms;

namespace Solutia.Commands.GEN
{
    [Transaction(TransactionMode.Manual)]
    internal class ExportIFC : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            try
            {
                /*
                // Salvar o arquivo antes de exportar
                try
                {
                    doc.Save();
                }
                catch (Exception ex)
                {
                    TaskDialog.Show("Erro", $"Erro ao salvar o arquivo: {ex.Message}");
                    return Result.Failed;
                }
                */
                // Abrir diálogo para selecionar a pasta de exportação
                using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
                {
                    folderDialog.Description = "Selecione a pasta para exportar o arquivo IFC";
                    folderDialog.ShowNewFolderButton = true;

                    if (folderDialog.ShowDialog() == DialogResult.OK)
                    {
                        string selectedFolder = folderDialog.SelectedPath;

                        // Definir o nome do arquivo IFC baseado no nome do projeto
                        string fileName = $"{Path.GetFileNameWithoutExtension(doc.Title)}.ifc";
                        string exportPath = Path.Combine(selectedFolder, fileName);

                        // Iniciar uma transação
                        using (Transaction transaction = new Transaction(doc, "Exportar para IFC"))
                        {
                            transaction.Start();

                            // Configuração de exportação IFC
                            IFCExportOptions ifcOptions = new IFCExportOptions();

                            // Exportar o modelo para IFC
                            bool exportResult = doc.Export(selectedFolder, fileName, ifcOptions);

                            transaction.Commit();

                            if (exportResult)
                            {
                                TaskDialog.Show("Exportação IFC", $"Exportação concluída com sucesso!\nArquivo salvo em: {exportPath}");
                                return Result.Succeeded;
                            }
                            else
                            {
                                TaskDialog.Show("Erro", "A exportação para IFC falhou. Verifique as configurações e tente novamente.");
                                return Result.Failed;
                            }
                        }
                    }
                    else
                    {
                        TaskDialog.Show("Operação Cancelada", "Nenhuma pasta foi selecionada.");
                        return Result.Cancelled;
                    }
                }
            }
            catch (Exception ex)
            {
                message = $"Erro inesperado: {ex.Message}";
                return Result.Failed;
            }
        }
    }
}
