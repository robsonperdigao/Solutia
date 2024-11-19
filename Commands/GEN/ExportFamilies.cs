using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows.Forms;
using Autodesk.Revit.Attributes;

namespace Solutia.Commands.GEN
{
    [Transaction(TransactionMode.Manual)]
    internal class ExportFamilies : IExternalCommand
    {
        public Result Execute(
            ExternalCommandData commandData,
            ref string message,
            ElementSet elements)
        {
            // Obtém o documento atual
            Document doc = commandData.Application.ActiveUIDocument.Document;

            // Cria um diálogo para seleção de pasta
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Selecione a pasta para salvar as famílias";
                folderDialog.ShowNewFolderButton = true;

                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    string targetFolder = folderDialog.SelectedPath;

                    // Filtrar todas as famílias no projeto
                    FilteredElementCollector collector = new FilteredElementCollector(doc)
                        .OfClass(typeof(Family));

                    int savedCount = 0;
                    List<string> errorLog = new List<string>(); // Lista para armazenar mensagens de erro

                    foreach (Element element in collector)
                    {
                        Family family = element as Family;
                        if (family == null || !family.IsEditable) continue;

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
                            // Armazena a mensagem de erro na lista de log
                            errorLog.Add($"Falha ao salvar a família '{familyName}': {ex.Message}");
                        }
                    }

                    // Salvar o arquivo de log caso existam erros
                    if (errorLog.Count > 0)
                    {
                        string logFilePath = Path.Combine(targetFolder, "Log.txt");
                        File.WriteAllLines(logFilePath, errorLog);
                    }

                    // Mostra mensagem final
                    string resultMessage = $"{savedCount} famílias salvas com sucesso.\n";
                    if (errorLog.Count > 0)
                    {
                        resultMessage += $"{errorLog.Count} erros encontrados. Consulte o arquivo 'Log.txt' na pasta:\n{targetFolder}";
                    }
                    TaskDialog.Show("Resultado", resultMessage);

                    return Result.Succeeded;
                }
                else
                {
                    message = "Operação cancelada pelo usuário.";
                    return Result.Cancelled;
                }
            }
        }
    }
}
