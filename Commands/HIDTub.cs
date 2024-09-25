using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;

namespace Solutia.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class HIDTub : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string msg, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            OpenFileDialog selecionaArquivo = new OpenFileDialog
            {
                Filter = "Text Files (*.txt)|*.txt",
                Title = "Selecione o arquivo que contém as coordenadas de tubulação"
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

            double pe = 3.2808398950;

            try
            {
                string[] segmentosTubos = File.ReadAllLines(filePath);

                foreach (string segmentoTubo in segmentosTubos)
                {
                    string[] segmentoInfo = segmentoTubo.Split('/');
                    if (segmentoInfo.Length < 4)
                    {
                        throw new Exception("Formato de dados inválido: " + segmentoTubo);
                    }

                    string comentario = segmentoInfo[0].Trim();
                    double diametro = Convert.ToDouble(segmentoInfo[1].Trim());
                    string pavimento = segmentoInfo[2].Trim();

                    string[] coordenadasSegmento = segmentoInfo[3].Split(';');

                    List<XYZ> pontos = new List<XYZ>();

                    foreach (string segmento in coordenadasSegmento)
                    {
                        string[] coordenadas = segmento.Split(' ');
                        if (coordenadas.Length == 3)
                        {
                            double x = Convert.ToDouble(coordenadas[0].Replace(',', '.'), CultureInfo.InvariantCulture) * pe;
                            double y = Convert.ToDouble(coordenadas[1].Replace(',', '.'), CultureInfo.InvariantCulture) * pe;
                            double z = Convert.ToDouble(coordenadas[2].Replace(',', '.'), CultureInfo.InvariantCulture) * pe;

                            pontos.Add(new XYZ(x, y, z));
                        }
                    }

                    ViewPlan viewPlan = doc.ActiveView as ViewPlan;

                    if (viewPlan != null)
                    {
                        Level level = viewPlan.GenLevel;
                        double elevacaoNivel = level.Elevation;
                        PipeType tipoTubo = new FilteredElementCollector(doc).OfClass(typeof(PipeType)).FirstElement() as PipeType;
                        PipingSystemType tipoSistema = new FilteredElementCollector(doc).OfClass(typeof(PipingSystemType)).FirstElement() as PipingSystemType;

                        if (tipoTubo == null || level == null || tipoSistema == null)
                        {
                            throw new Exception("Não foi possível encontrar os tipos de tubo, nível ou sistema de tubulação.");
                        }

                        using (Transaction trans = new Transaction(doc, "Criar Tubulações"))
                        {
                            trans.Start();

                            for (int i = 0; i < pontos.Count - 1; i++)
                            {
                                if (pontos[i].DistanceTo(pontos[i + 1]) < 0.0833) // Valor mínimo que o Revit permite criar o tubo
                                {
                                    continue;
                                }
                                // Ajusta as coordenadas Z dos pontos para torná-las relativas ao nível
                                XYZ pontoInicio = new XYZ(pontos[i].X, pontos[i].Y, pontos[i].Z + elevacaoNivel);
                                XYZ pontoFim = new XYZ(pontos[i + 1].X, pontos[i + 1].Y, pontos[i + 1].Z + elevacaoNivel);

                                Line line = Line.CreateBound(pontoInicio, pontoFim);
                                Pipe pipe = Pipe.Create(doc, tipoSistema.Id, tipoTubo.Id, level.Id, line.GetEndPoint(0), line.GetEndPoint(1));

                                pipe.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM).Set(diametro / 304.8);

                                pipe.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).Set(comentario);

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
