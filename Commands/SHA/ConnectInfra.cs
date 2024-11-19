using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB.Plumbing;
using System.Linq;
using Autodesk.Revit.Attributes;

namespace Solutia.Commands.SHA
{
    [Transaction(TransactionMode.Manual)]
    public class ConnectInfra : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Obtém o documento atual
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            View view = uidoc.ActiveView;

            // Obtém a lista de todas as tubulações visíveis na vista atual
            List<Pipe> tubulacoes = InfraVistaAtual(doc, view);
            double margem = 1.0; // Margem padrão de 1 unidade

            List<Element> conexoes = new List<Element>();
            Dictionary<Connector, Connector> conDic = new Dictionary<Connector, Connector>();
            List<Connector> conList = new List<Connector>();

            // Coleta os conectores não conectados das tubulações
            foreach (var tubo in tubulacoes)
            {
                ConnectorSet conectores = tubo.ConnectorManager.Connectors;
                foreach (Connector conector in conectores)
                {
                    if (conector.IsConnected)
                        continue;
                    conDic[conector] = null;
                    conList.Add(conector);
                }
            }

            // Encontra os conectores mais próximos
            foreach (var k in conDic.Keys.ToList())
            {
                double mindist = 1000000;
                Connector prox = null;
                foreach (var conector in conList)
                {
                    if (conector.Owner.Id.Equals(k.Owner.Id))
                        continue;
                    double dist = k.Origin.DistanceTo(conector.Origin);
                    if (dist < mindist)
                    {
                        mindist = dist;
                        prox = conector;
                    }
                }
                if (mindist > margem)
                    continue;
                conDic[k] = prox;
                conList.Remove(prox);
                conDic.Remove(prox);
            }

            // Cria os conectores dentro de uma transação
            using (Transaction trans = new Transaction(doc, "Conecta Tubulações"))
            {
                trans.Start();
                foreach (var kvp in conDic)
                {
                    Connector k = kvp.Key;
                    Connector v = kvp.Value;
                    try
                    {
                        FamilyInstance conexao = doc.Create.NewElbowFitting(k, v);
                        conexoes.Add(conexao);
                    }
                    catch (Exception)
                    {
                 
                    }
                }
                trans.Commit();
            }

            // Retorna os conectores criados
            return Result.Succeeded;
        }

        private List<Pipe> InfraVistaAtual(Document doc, View view)
        {
            // Coleta todas as tubulações visíveis na vista atual
            FilteredElementCollector filtro = new FilteredElementCollector(doc, view.Id);
            ICollection<Element> elementos = filtro.OfClass(typeof(Pipe)).ToElements();
            List<Pipe> tubulacoes = elementos.Cast<Pipe>().ToList();
            return tubulacoes;
        }
    }
}

