using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using Autodesk.Revit.Attributes;

namespace Solutia.Commands.SHA
{
    [Transaction(TransactionMode.Manual)]
    public class IntersectTub : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            // Call the method to evaluate intersections
            EvaluateIntersections(uiDoc);

            return Result.Succeeded;
        }

        public static XYZ FindIntersection(Line line1, Line line2)
        {
            IntersectionResultArray resultArray;
            SetComparisonResult result = line1.Intersect(line2, out resultArray);

            if (result == SetComparisonResult.Overlap && resultArray != null && resultArray.Size > 0)
            {
                return resultArray.get_Item(0).XYZPoint;
            }

            return null; // No intersection
        }

        public static void EvaluateIntersections(UIDocument uiDoc)
        {
            Document doc = uiDoc.Document;

            // Collect all pipes in the model
            FilteredElementCollector collector = new FilteredElementCollector(doc)
                .OfClass(typeof(Pipe));

            List<Line> pipeLines = new List<Line>();

            foreach (Element element in collector)
            {
                LocationCurve locCurve = element.Location as LocationCurve;
                if (locCurve != null && locCurve.Curve is Line line)
                {
                    pipeLines.Add(line);
                }
            }

            // Check intersections between pipe lines
            List<string> intersections = new List<string>();

            for (int i = 0; i < pipeLines.Count; i++)
            {
                for (int j = i + 1; j < pipeLines.Count; j++)
                {
                    XYZ intersection = FindIntersection(pipeLines[i], pipeLines[j]);
                    if (intersection != null)
                    {
                        intersections.Add($"Intersection found at: {intersection}");
                    }
                }
            }

            // Show results
            if (intersections.Count > 0)
            {
                TaskDialog.Show("Intersections Found", string.Join("\n", intersections));
            }
            else
            {
                TaskDialog.Show("No Intersections", "No intersections were found between pipes.");
            }
        }
    }
}
