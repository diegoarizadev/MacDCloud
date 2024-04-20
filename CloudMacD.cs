using System;
using ATAS.Indicators;
using ATAS.Indicators.Technical;
using System.ComponentModel;
using System.Windows.Media;


namespace MacDCloud
{
    [Category("n0rf3n")]
    [DisplayName("MacdCloud")]
    public class CloudMacD : Indicator
    {

        #region Fields

        private readonly SMMA _fastSmma = new ();
        private readonly SMMA _slowSmma = new ();

        private readonly ValueDataSeries _fastSmaSeries = new("Fast SMMA")
        {
            VisualType = VisualMode.Line,
            IgnoredByAlerts = true
        };
        private readonly ValueDataSeries _slowSmaSeries = new("Slow SMMA")
        {
            VisualType = VisualMode.Line,
            IgnoredByAlerts = true
        };

        private readonly RangeDataSeries _cloudSeriesGreen = new("Background Up") 
        { 
            RangeColor = Color.FromArgb(90, 0, 255, 0) 
        };

        private readonly RangeDataSeries _cloudSeriesRed = new("Background Down") 
        { 
            RangeColor = Color.FromArgb(90, 255, 0, 0)
        };

        private bool _lastFastAboveSlow = false; // Estado inicial arbitrario
        private int _lastCrossover = -1;

        #endregion


        public int FastPeriod
        {
            get { return _fastSmma.Period; }
            set
            {
                if (value <= 0)
                    return;

                _fastSmma.Period = value;
                RecalculateValues();
            }
        }
        public int SlowPeriod
        {
            get { return _slowSmma.Period; }
            set
            {
                if (value <= 0)
                    return;

                _slowSmma.Period = value;
                RecalculateValues();
            }
        }

        public CloudMacD()
        {

            _cloudSeriesGreen.RangeColor = Color.FromArgb(90, 0, 255, 0);
            _cloudSeriesRed.RangeColor = Color.FromArgb(90, 255, 0, 0);
            _cloudSeriesGreen.IsHidden = true;
            _cloudSeriesRed.IsHidden = true;


            DataSeries.Add(_fastSmaSeries);
            DataSeries.Add(_slowSmaSeries);
            DataSeries.Add(_cloudSeriesGreen);
            DataSeries.Add(_cloudSeriesRed);

        }

        protected override void OnCalculate(int bar, decimal value)
        {
            var fastSma = _fastSmma.Calculate(bar, value);
            var slowSma = _slowSmma.Calculate(bar, value);

            _fastSmaSeries[bar] = fastSma;
            _slowSmaSeries[bar] = slowSma;

            if (bar >= Math.Max(FastPeriod, SlowPeriod) - 1)
            {
                bool fastAboveSlow = fastSma > slowSma;

                if (fastAboveSlow != _lastFastAboveSlow || bar == 0)
                {
                    _lastFastAboveSlow = fastAboveSlow;
                    _lastCrossover = bar;
                }

                if (_lastCrossover == bar)
                {
                    // Cambiar la visibilidad de las series según el cruce actual
                    _cloudSeriesGreen.IsHidden = !fastAboveSlow;
                    _cloudSeriesRed.IsHidden = fastAboveSlow;
                }

                if (!_cloudSeriesGreen.IsHidden || !_cloudSeriesRed.IsHidden)
                {
                    var activeSeries = fastAboveSlow ? _cloudSeriesGreen : _cloudSeriesRed;
                    activeSeries[bar].Upper = Math.Max(fastSma, slowSma);
                    activeSeries[bar].Lower = Math.Min(fastSma, slowSma);
                }
            }
        }
    }
}
