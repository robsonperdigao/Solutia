using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;

// Furo com coordenadas TXT
namespace Solutia.Commands.SHA
{
    [Transaction(TransactionMode.Manual)]
    public class Furo : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string msg, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            OpenFileDialog selecionaArquivo = new OpenFileDialog
            {
                Filter = "Text Files (*.txt)|*.txt",
                Title = "Selecione o arquivo que contém as coordenadas dos furos"
            };

            string filePath = string.Empty;
            if (selecionaArquivo.ShowDialog() == DialogResult.OK)
            {
                filePath = selecionaArquivo.FileName;
            }
            else
            {
                return Result.Cancelled;
            }

            double pe = 3.2808398950; // Conversão de metros para pés

            try
            {
                string[] furos = File.ReadAllLines(filePath);

                foreach (string furo in furos)
                {
                    // Separar o identificador das coordenadas
                    string[] furoInfo = furo.Split('/');

                    if (furoInfo.Length != 2)
                    {
                        throw new Exception("Formato de dados inválido: " + furo);
                    }

                    string tagFuro = furoInfo[0].Trim(); // Identificador do furo

                    string[] coordenadas = furoInfo[1].Split(';');
                    if (coordenadas.Length != 3)
                    {
                        throw new Exception("Formato de coordenadas inválido: " + furoInfo[1]);
                    }

                    double x = Convert.ToDouble(coordenadas[0], CultureInfo.InvariantCulture) * pe;
                    double y = Convert.ToDouble(coordenadas[1], CultureInfo.InvariantCulture) * pe;
                    double z = Convert.ToDouble(coordenadas[2], CultureInfo.InvariantCulture) * pe;

                    XYZ ponto = new XYZ(x, y, z);

                    Family family = null;
                    FamilySymbol familyType = null;
                    string familyName = "Furo Retangular";

                    FilteredElementCollector collector = new FilteredElementCollector(doc);
                    collector.OfClass(typeof(Family));

                    foreach (Family fam in collector)
                    {
                        if (fam.Name == familyName)
                        {
                            family = fam;
                            break;
                        }
                    }

                    if (family == null)
                    {
                        throw new Exception("Família não encontrada.");
                    }

                    // Seleciona o tipo da família (por default, é o primeiro)
                    IList<ElementId> familyTypeIds = family.GetFamilySymbolIds().ToList();
                    if (familyTypeIds.Count > 0)
                    {
                        familyType = doc.GetElement(familyTypeIds[0]) as FamilySymbol;
                    }

                    if (familyType == null)
                    {
                        throw new Exception("Tipo de família não encontrado.");
                    }

                    ViewPlan viewPlan = doc.ActiveView as ViewPlan;

                    if (viewPlan != null)
                    {
                        Level level = viewPlan.GenLevel;
                        double elevacaoNivel = level.Elevation; 

                        using (Transaction trans = new Transaction(doc, "Inserir Instâncias da Família"))
                        {
                            trans.Start();

                            if (!familyType.IsActive)
                            {
                                familyType.Activate();
                                doc.Regenerate();
                            }

                            FamilyInstance instance = doc.Create.NewFamilyInstance(ponto, familyType, StructuralType.NonStructural);

                            Parameter tagFuroPar = instance.LookupParameter("TAG do Furo");
                            tagFuroPar.Set(tagFuro);

                            Parameter hFuro = instance.LookupParameter("H do Furo (Centro)");
                            double zPositivo = Math.Abs(ponto.Z);
                            hFuro.Set(zPositivo);

                            Parameter deslocamentoParam = instance.get_Parameter(BuiltInParameter.INSTANCE_FREE_HOST_OFFSET_PARAM);
                            if (deslocamentoParam != null && deslocamentoParam.StorageType == StorageType.Double)
                            {
                                deslocamentoParam.Set(0);
                            }
                            trans.Commit();
                        }
                    }
                }
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return Result.Failed;
            }
        }
    }
}
