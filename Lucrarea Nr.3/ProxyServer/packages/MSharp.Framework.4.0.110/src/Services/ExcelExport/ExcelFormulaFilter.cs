using System;
using System.Collections.Generic;
using System.Text;
using static MSharp.Framework.Services.ExcelExporter<object>;

namespace MSharp.Framework.Services
{
    public class ExcelFormulaFilter : IExcelExporterDataFilter
    {
        public string Apply(ExcelColumn<object> column, string value)
        {
            var formulaChars = new char[] { '=', '+', '-', '@', '|' };

            return value.TrimStart(formulaChars);
        }
    }
}
