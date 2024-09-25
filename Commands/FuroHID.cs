using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using System.Windows.Forms;

namespace Projetar.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class FuroHID : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            MessageBox.Show("o botão FuroHID está funcionando");

            return Result.Succeeded;
        }
    }
    

}
