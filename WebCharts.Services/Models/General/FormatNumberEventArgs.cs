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
        private double _value;
        private string _format;
        private string _localizedValue;
        private ChartValueType _valueType = ChartValueType.Auto;
        private object _senderTag;
        private ChartElementType _elementType = ChartElementType.Nothing;

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
        public string LocalizedValue
        {
            get { return _localizedValue; }
            set { _localizedValue = value; }
        }

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
            _localizedValue = localizedValue;
            _senderTag = senderTag;
            _elementType = elementType;
        }

        #endregion
    }
}
