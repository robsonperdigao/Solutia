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
    public class AutoPrumHID : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            MessageBox.Show("o botão AutoPrumHID está funcionando");

            return Result.Succeeded;
        }
    }
    

}
