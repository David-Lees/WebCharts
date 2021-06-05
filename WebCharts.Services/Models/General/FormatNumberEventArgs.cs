using System;
using WebCharts.Services;

namespace WebCharts.Services
{
    /// <summary>
    /// Event arguments of localized numbers formatting event.
    /// </summary>
    public class FormatNumberEventArgs : EventArgs
    {
        #region Fields

        // Private fields
        private readonly double _value;
        private readonly string _format;
        private readonly ChartValueType _valueType = ChartValueType.Auto;
        private readonly object _senderTag;
        private readonly ChartElementType _elementType = ChartElementType.Nothing;

        #endregion

        #region Properties

        /// <summary>
        /// Value to be formatted.
        /// </summary>
        public double Value
        {
            get { return _value; }
        }

        /// <summary>
        /// Localized text.
        /// </summary>
        public string LocalizedValue { get; set; }

        /// <summary>
        /// Format string.
        /// </summary>
        public string Format
        {
            get { return _format; }
        }

        /// <summary>
        /// Value type.
        /// </summary>
        public ChartValueType ValueType
        {
            get { return _valueType; }
        }

        /// <summary>
        /// The sender object of the event.
        /// </summary>
        public object SenderTag
        {
            get { return _senderTag; }
        }

        /// <summary>
        /// Chart element type.
        /// </summary>
        public ChartElementType ElementType
        {
            get { return _elementType; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Default constructor is not accessible
        /// </summary>
        private FormatNumberEventArgs()
        {
        }

        /// <summary>
        /// Object constructor.
        /// </summary>
        /// <param name="value">Value to be formatted.</param>
        /// <param name="format">Format string.</param>
        /// <param name="valueType">Value type..</param>
        /// <param name="localizedValue">Localized value.</param>
        /// <param name="senderTag">Chart element object tag.</param>
        /// <param name="elementType">Chart element type.</param>
        internal FormatNumberEventArgs(double value, string format, ChartValueType valueType, string localizedValue, object senderTag, ChartElementType elementType)
        {
            _value = value;
            _format = format;
            _valueType = valueType;
            LocalizedValue = localizedValue;
            _senderTag = senderTag;
            _elementType = elementType;
        }

        #endregion
    }
}
