using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using System.Windows.Forms;
using Solutia.Forms;

namespace Solutia.Commands.GEN
{
    [Transaction(TransactionMode.Manual)]
    internal class ExportFamilies : IExternalCommand
    {
        public Result Execute(
            ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Obtém o documento atual
            Document doc = commandData.Application.ActiveUIDocument.Document;

            // Exibe o formulário para selecionar categorias e a pasta
            using (CategoryFamilyExportForm form = new CategoryFamilyExportForm(doc))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    // Obtém as categorias selecionadas
                    List<BuiltInCategory> selectedCategories = form.SelectedCategories;

                    if (selectedCategories == null || !selectedCategories.Any())
                    {
                        TaskDialog.Show("Exportação de Famílias", "Nenhuma categoria foi selecionada.");
                        return Result.Cancelled;
                    }

                    // Obtém o caminho da pasta selecionada no formulário
                    string targetFolder = form.SelectedFolderPath;

                    if (string.IsNullOrEmpty(targetFolder) || !Directory.Exists(targetFolder))
                    {
                        TaskDialog.Show("Erro", "Caminho de pasta inválido ou não selecionado.");
                        return Result.Failed;
                    }

                    // Filtra as famílias no projeto pelas categorias selecionadas
                    FilteredElementCollector collector = new FilteredElementCollector(doc)
                        .OfClass(typeof(Family));

                    int savedCount = 0;
                    List<string> errorLog = new List<string>();

                    foreach (Element element in collector)
                    {
                        Family family = element as Family;

                        // Verifica se a família é válida e pertence às categorias selecionadas
                        if (family == null || !family.IsEditable ||
                            !selectedCategories.Contains((BuiltInCategory)family.FamilyCategory.Id.Value))
                            continue;

                        string familyName = family.Name;
                        string filePath = Path.Combine(targetFolder, $"{familyName}.rfa");

                        try
                        {
                            // Edita a família (abre um novo documento)
                            Document familyDoc = doc.EditFamily(family);

                            // Salva a família no caminho especificado
                            familyDoc.SaveAs(filePath);

                            // Fecha o documento da família
                            familyDoc.Close(false);

                            savedCount++;
                        }
                        catch (Exception ex)
                        {
                            // Armazena mensagens de erro
                            errorLog.Add($"Falha ao salvar a família '{familyName}': {ex.Message}");
                        }
                    }

                    // Salva o log na pasta selecionada
                    if (errorLog.Any())
                    {
                        string logFilePath = Path.Combine(targetFolder, "ExportLog.txt");
                        File.WriteAllLines(logFilePath, errorLog);
                    }

                    // Mensagem de resultado
                    string resultMessage = $"{savedCount} famílias salvas com sucesso.\n";
                    if (errorLog.Any())
                    {
                        resultMessage += $"{errorLog.Count} erros encontrados. Consulte o arquivo 'ExportLog.txt' na pasta:\n{targetFolder}";
                    }
                    TaskDialog.Show("Resultado", resultMessage);

                    return Result.Succeeded;
                }
            }

            message = "Operação cancelada pelo usuário.";
            return Result.Cancelled;
        }
    }
}
