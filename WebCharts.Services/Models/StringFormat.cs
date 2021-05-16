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

        public StringFormat(StringFormat sf)
        {
            Alignment = sf.Alignment;
            LineAlignment = sf.LineAlignment;
            FormatFlags = sf.FormatFlags;
            DigitSubstitutionLanguage = sf.DigitSubstitutionLanguage;
            DigitSubstitutionMethod = sf.DigitSubstitutionMethod;
        }

        //public StringFormat(StringFormatFlags)

        //public StringFormat(StringFormatFlags, Int32)

        public StringAlignment Alignment { get; set; }
        public int DigitSubstitutionLanguage { get; private set; }
        public StringAlignment LineAlignment { get; set; }
        public StringFormatFlags FormatFlags { get; set; }
        public StringDigitSubstitute DigitSubstitutionMethod { get; set; }
        public StringTrimming Trimming { get; set; }

        public static StringFormat GenericTypographic
        {
            get
            {
                return new StringFormat()
                {
                    FormatFlags = (StringFormatFlags)24580,
                    Alignment = StringAlignment.Near,
                    LineAlignment = StringAlignment.Near,
                    Trimming = StringTrimming.None,
                    DigitSubstitutionMethod = StringDigitSubstitute.User,
                    DigitSubstitutionLanguage = 0
                };
            }
        }

        public static StringFormat GenericDefault
        {
            get
            {
                return new StringFormat
                {
                    FormatFlags = 0,
                    Alignment = StringAlignment.Near,
                    LineAlignment = StringAlignment.Near,
                    Trimming = StringTrimming.Character,
                    DigitSubstitutionMethod = StringDigitSubstitute.User,
                    DigitSubstitutionLanguage = 0
                };
            }
        }

        public object Clone()
        {
            return new StringFormat()
            {
                Alignment = Alignment,
                LineAlignment = LineAlignment,
                FormatFlags = FormatFlags,
                DigitSubstitutionLanguage = DigitSubstitutionLanguage,
                DigitSubstitutionMethod = DigitSubstitutionMethod
            };
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Cleanup
        }
    }
}
