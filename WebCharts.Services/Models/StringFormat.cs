using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebCharts.Services.Enums;

namespace WebCharts.Services.Models
{
    public class StringFormat : ICloneable, IDisposable
    {
        public StringFormat() { }

        //public StringFormat(StringFormat)

        //public StringFormat(StringFormatFlags)

        //public StringFormat(StringFormatFlags, Int32)

        public StringAlignment Alignment { get; set; }
        public int DigitSubstitutionLanguage { get; }
        public StringAlignment LineAlignment { get; set; }
        public StringFormatFlags FormatFlags { get; set; }


    }
}
