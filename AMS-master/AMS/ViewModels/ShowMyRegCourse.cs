namespace AMS.ViewModels
{
    public class ShowMyRegCourse
    {
        public string GraphName { get; set; }
        public string UId { get; set; }
        public Root Data { get; set; }
    }

    public class Root
    {
        public string type { get; set; }
        public string indexLabelFontFamily { get; set; }
        public int indexLabelFontSize { get; set; }
        public string indexLabel { get; set; }
        public int startAngle { get; set; }
        public bool showInLegend { get; set; }
        public string toolTipContent { get; set; }
        public List<DataPoint> dataPoints { get; set; }
    }

    public class DataPoint
    {
        public double y { get; set; }
        public string legendText { get; set; }
        public string label { get; set; }
    }

}
