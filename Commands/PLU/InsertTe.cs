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

namespace Solutia.Commands.PLU
{
    [Transaction(TransactionMode.Manual)]
    public class InsertTe : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string msg, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            // Abre a caixa de diálogo para selecionar o arquivo
            OpenFileDialog selecionaArquivo = new OpenFileDialog
            {
                Filter = "Text Files (*.txt)|*.txt",
                Title = "Selecione o arquivo que contém as coordenadas para inserir os Tês"
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

            double pe = 3.2808398950; // Conversão de metros para pés (Revit utiliza pés)
            List<XYZ> pontosXYZ = new List<XYZ>();

            try
            {
                // Lê e processa as coordenadas do arquivo
                string[] linhas = File.ReadAllLines(filePath);
                foreach (string linha in linhas)
                {
                    string[] coordenadas = linha.Split(' ');
                    if (coordenadas.Length == 3)
                    {
                        double x = Convert.ToDouble(coordenadas[0].Replace(',', '.'), CultureInfo.InvariantCulture) * pe;
                        double y = Convert.ToDouble(coordenadas[1].Replace(',', '.'), CultureInfo.InvariantCulture) * pe;
                        double z = Convert.ToDouble(coordenadas[2].Replace(',', '.'), CultureInfo.InvariantCulture) * pe;

                        pontosXYZ.Add(new XYZ(x, y, z));
                    }
                }

                using (Transaction trans = new Transaction(doc, "Inserir Tês nas Tubulações"))
                {
                    trans.Start();

                    foreach (XYZ ponto in pontosXYZ)
                    {
                        // Localiza as tubulações próximas ao ponto
                        Pipe pipe = FindNearestPipe(doc, ponto);
                        if (pipe != null)
                        {
                            // Insere o Tê na tubulação
                            InsertTee(doc, pipe, ponto);
                        }
                    }

                    trans.Commit();
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Erro", ex.Message);
                return Result.Failed;
            }

            return Result.Succeeded;
        }

        private Pipe FindNearestPipe(Document doc, XYZ point)
        {
            // Filtra tubulações no modelo
            FilteredElementCollector collector = new FilteredElementCollector(doc)
                .OfClass(typeof(Pipe));

            double tolerance = 1.0; // Tolerância em pés para encontrar tubulações próximas
            Pipe nearestPipe = null;
            double minDistance = double.MaxValue;

            foreach (Element element in collector)
            {
                Pipe pipe = element as Pipe;
                if (pipe != null)
                {
                    LocationCurve locationCurve = pipe.Location as LocationCurve;
                    if (locationCurve != null)
                    {
                        double distance = locationCurve.Curve.Distance(point);
                        if (distance < minDistance && distance <= tolerance)
                        {
                            nearestPipe = pipe;
                            minDistance = distance;
                        }
                    }
                }
            }

            return nearestPipe;
        

    }

    private void InsertTee(Document doc, Pipe pipe, XYZ point)
        {
            // Obtém o nível da tubulação
            Level pipeLevel = doc.GetElement(pipe.get_Parameter(BuiltInParameter.RBS_START_LEVEL_PARAM).AsElementId()) as Level;

            // Cria o Tê
            FamilySymbol teeSymbol = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol))
                .OfCategory(BuiltInCategory.OST_PipeFitting)
                .FirstOrDefault(f => f.Name == "Tê - Soldado - CU") as FamilySymbol; // Ajuste para buscar seu tipo de Tê específico

            if (teeSymbol != null && !teeSymbol.IsActive)
            {
                teeSymbol.Activate();
            }

            FamilyInstance teeInstance = doc.Create.NewFamilyInstance(point, teeSymbol, pipeLevel, StructuralType.NonStructural);

            // Conecta o Tê à tubulação
            ConnectorSet connectors = teeInstance.MEPModel.ConnectorManager.Connectors;
            Connector teeConnector = connectors.Cast<Connector>().OrderBy(c => c.Origin.DistanceTo(point)).FirstOrDefault();

            if (teeConnector != null)
            {
                foreach (Connector pipeConnector in pipe.ConnectorManager.Connectors)
                {
                    if (pipeConnector.Origin.DistanceTo(point) <= 5.0) // Tolerância para conexão
                    {
                        teeConnector.ConnectTo(pipeConnector);
                        break;
                    }
                }
            }
        }
    }
}

