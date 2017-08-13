using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NASClientTCP
{
    class SimplePerformanceCounter
    {
        public SimplePerformanceCounter()
        {

        }

        public static PerformanceCounter CreateSimpleCounter(string counterName,
            string counterHelp, PerformanceCounterType counterType, string
            categoryName, string categoryHelp)
        {
            CounterCreationDataCollection counterCollection =
                new CounterCreationDataCollection();

            CounterCreationData counter =
                new CounterCreationData(counterName, counterHelp, counterType);

            counterCollection.Add(counter);

            if (PerformanceCounterCategory.Exists(categoryName))
            {
                PerformanceCounterCategory.Delete(categoryName);
            }
            PerformanceCounterCategory appCategory =
                PerformanceCounterCategory.Create(categoryName, categoryHelp,
                PerformanceCounterCategoryType.SingleInstance, counterCollection);

            PerformanceCounter appCounter =
                new PerformanceCounter(categoryName, counterName, false);

            appCounter.RawValue = 0;
            return appCounter;
        }
    }
}
