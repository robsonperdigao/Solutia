using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using System.Windows.Forms;

namespace Solutia.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class Lib : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            MessageBox.Show("O botão Lib está funcionando");

            return Result.Succeeded;
        }
    }


}
