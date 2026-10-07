namespace ChurchManagementMVC.ViewModels
{
    public class PdfExportVM
    {
        public string ChurchName { get; set; } = "ECWA GIGINYU HAUSA SECTION";

        public string ReportTitle { get; set; } = string.Empty;

        public string Subtitle { get; set; } = string.Empty;

        public List<PdfExportSectionVM> Sections { get; set; }
            = new List<PdfExportSectionVM>();
    }

    public class PdfExportSectionVM
    {
        public string Title { get; set; } = string.Empty;

        public List<string> Headers { get; set; }
            = new List<string>();

        public List<List<string>> Rows { get; set; }
            = new List<List<string>>();
    }
}