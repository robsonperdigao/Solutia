using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using RevitView = Autodesk.Revit.DB.View;

namespace Solutia.Commands.GEN
{
    [Transaction(TransactionMode.Manual)]
    internal class ExportRVT : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            List<string> logErrors = new List<string>(); // Lista para armazenar os erros

            try
            {
                using (Transaction transaction = new Transaction(doc, "Excluir Folhas e Vistas"))
                {
                    transaction.Start();

                    // Excluir vistas com filtros
                    var viewsToDelete = new FilteredElementCollector(doc)
                        .OfClass(typeof(RevitView))
                        .Cast<RevitView>()
                        .Where(v =>
                        {
                            if (v.ViewType == ViewType.Legend && v.Name.Contains("PRJ VISTA INICIAL"))
                                return false;
                            if (v.ViewType == ViewType.ThreeD && v.Name == "COORDENAÇÃO")
                                return false;

                            return (v.ViewType == ViewType.FloorPlan ||
                                    v.ViewType == ViewType.CeilingPlan ||
                                    v.ViewType == ViewType.Legend ||
                                    v.ViewType == ViewType.Schedule ||
                                    v.ViewType == ViewType.DraftingView ||
                                    v.ViewType == ViewType.Detail ||
                                    v.ViewType == ViewType.Section ||
                                    v.ViewType == ViewType.Elevation ||
                                    v.ViewType == ViewType.ThreeD) && !v.IsTemplate;
                        })
                        .Select(v => v.Id)
                        .ToList();

                    foreach (var viewId in viewsToDelete)
                    {
                        try
                        {
                            doc.Delete(viewId);
                        }
                        catch (Exception ex)
                        {
                            logErrors.Add($"Erro ao excluir vista ID {viewId}: {ex.Message}");
                        }
                    }

                    // Excluir todas as folhas
                    var sheetsToDelete = new FilteredElementCollector(doc)
                        .OfClass(typeof(ViewSheet))
                        .Cast<ViewSheet>()
                        .Select(s => s.Id)
                        .ToList();

                    foreach (var sheetId in sheetsToDelete)
                    {
                        try
                        {
                            doc.Delete(sheetId);
                        }
                        catch (Exception ex)
                        {
                            logErrors.Add($"Erro ao excluir folha ID {sheetId}: {ex.Message}");
                        }
                    }

                    transaction.Commit();
                }
                /*
                // Perguntar ao usuário sobre a gestão de links
                TaskDialogResult result = TaskDialog.Show(
                    "Gerenciar Links",
                    "Você deseja excluir os links carregados?\n" +
                    "Sim: Excluir completamente os links.\n" +
                    "Não: Apenas descarregar os links.",
                    TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No | TaskDialogCommonButtons.Cancel);

                if (result == TaskDialogResult.Yes || result == TaskDialogResult.No)
                {
                    using (Transaction linkTransaction = new Transaction(doc, "Gerenciar Links"))
                    {
                        linkTransaction.Start();

                        var links = new FilteredElementCollector(doc)
                            .OfClass(typeof(RevitLinkInstance))
                            .Cast<RevitLinkInstance>();

                        foreach (var link in links)
                        {
                            try
                            {
                                if (result == TaskDialogResult.Yes)
                                {
                                    // Excluir instâncias do link
                                    doc.Delete(link.Id);

                                    // Remover o tipo do link após a transação
                                    var linkType = doc.GetElement(link.GetTypeId()) as RevitLinkType;
                                    if (linkType != null)
                                    {
                                        doc.Delete(linkType.Id); // Remove completamente o link carregado
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                logErrors.Add($"Erro ao excluir link ID {link.Id}: {ex.Message}");
                            }
                        }

                        linkTransaction.Commit();
                    }

                    // Processar descarregamento de links separadamente
                    if (result == TaskDialogResult.No)
                    {
                        var linksToUnload = new FilteredElementCollector(doc)
                            .OfClass(typeof(RevitLinkInstance))
                            .Cast<RevitLinkInstance>()
                            .Select(link => doc.GetElement(link.GetTypeId()) as RevitLinkType)
                            .Where(linkType => linkType != null)
                            .ToList();

                        foreach (var linkType in linksToUnload)
                        {
                            try
                            {
                                using (Transaction unloadTransaction = new Transaction(doc, $"Descarregar link {linkType.Name}"))
                                {
                                    unloadTransaction.Start();
                                    linkType.Unload(null);
                                    unloadTransaction.Commit();
                                }
                            }
                            catch (Exception ex)
                            {
                                logErrors.Add($"Erro ao descarregar link {linkType.Name}: {ex.Message}");
                            }
                        }
                    }
                
                }
                else if (result == TaskDialogResult.Cancel)
                {
                    TaskDialog.Show("Operação Cancelada", "A operação foi cancelada pelo usuário.");
                    return Result.Cancelled;
                }
                */

                // Salvar o arquivo antes do Salvar Como
                try
                {
                    doc.Save();
                }
                catch (Exception ex)
                {
                    logErrors.Add($"Erro ao salvar o arquivo: {ex.Message}");
                    return Result.Failed;
                }

                // Exibir diálogo para salvar como
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Arquivo Revit (*.rvt)|*.rvt",
                    Title = "Salvar modelo como",
                    DefaultExt = "rvt",
                    AddExtension = true
                };

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedFilePath = saveFileDialog.FileName;

                    SaveAsOptions saveAsOptions = new SaveAsOptions
                    {
                        OverwriteExistingFile = true
                    };

                    if (doc.IsWorkshared)
                    {
                        saveAsOptions.SetWorksharingOptions(new WorksharingSaveAsOptions { SaveAsCentral = true });
                    }

                    doc.SaveAs(ModelPathUtils.ConvertUserVisiblePathToModelPath(selectedFilePath), saveAsOptions);

                    // Salvar log de erros
                    if (logErrors.Any())
                    {
                        string logFilePath = Path.Combine(Path.GetDirectoryName(selectedFilePath), "log.txt");
                        File.WriteAllLines(logFilePath, logErrors);
                    }
                }
                else
                {
                    TaskDialog.Show("Atenção", "Operação cancelada pelo usuário.");
                    return Result.Cancelled;
                }

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }
    }
}