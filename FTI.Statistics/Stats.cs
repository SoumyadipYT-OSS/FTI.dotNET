﻿namespace FTI.Statistics
{
    public class Stats
    {
        /// <summary>
        /// Returns the sample arithmetic mean of the data.
        /// The arithmetic mean is the sum of the data divided by the number of data points.
        /// It is commonly called "the average", although it is only one of many different mathematical averages.
        /// It is a measure of the central location of the data.
        /// If data is empty, a <see cref="StatisticsError"/> will be raised.
        /// </summary>
        /// <param name="data">A sequence or iterable of numeric data.</param>
        /// <returns>The arithmetic mean of the data.</returns>
        /// <exception cref="StatisticsError">Thrown when the data is empty.</exception>
        public static double Mean(IEnumerable<double> data)
        {
            if (data == null || !data.Any())
            {
                throw new StatisticsError("Data is empty.");
            }

            double sum = 0;
            int count = 0;

            foreach (var item in data)
            {
                sum += item;
                count++;
            }

            return sum / count;
        }




        /// <summary>
        /// Converts data to floats and computes the arithmetic mean.
        /// This function runs faster than the Mean function and always returns a float.
        /// If the input dataset is empty, a <see cref="StatisticsError"/> will be raised.
        /// </summary>
        /// <param name="data">A sequence or iterable of numeric data.</param>
        /// <param name="weights">Optional weights for the data. Must be the same length as the data if provided.</param>
        /// <returns>The arithmetic mean of the data.</returns>
        /// <exception cref="StatisticsError">Thrown when the data is empty.</exception>
        /// <exception cref="ArgumentException">Thrown when data and weights are not of the same length.</exception>
        public static double FMean(IEnumerable<double> data, IEnumerable<double>? weights = null)
        {
            var dataList = data as IList<double> ?? [.. data];

            if (!dataList.Any())
            {
                throw new StatisticsError("Data is empty.");
            }

            if (weights == null)
            {
                double sum = 0;
                int count = dataList.Count;

                foreach (var item in dataList)
                {
                    sum += item;
                }

                return sum / count;
            }
            else
            {
                var weightList = weights as IList<double> ?? [.. weights];

                if (dataList.Count != weightList.Count)
                {
                    throw new ArgumentException("Data and weights must be of the same length.");
                }

                double weightedSum = 0;
                double weightSum = 0;

                for (int i = 0; i < dataList.Count; i++)
                {
                    weightedSum += dataList[i] * weightList[i];
                    weightSum += weightList[i];
                }

                return weightedSum / weightSum;
            }
        }





        /// <summary>
        /// Converts data to floats and computes the geometric mean.
        /// The geometric mean indicates the central tendency or typical value of the data using the product of the values.
        /// Raises a <see cref="StatisticsError"/> if the input dataset is empty, contains a zero, or a negative value.
        /// </summary>
        /// <param name="data">A sequence or iterable of numeric data.</param>
        /// <returns>The geometric mean of the data.</returns>
        /// <exception cref="StatisticsError">Thrown when the data is empty, contains a zero, or contains a negative value.</exception>
        public static double GeometricMean(IEnumerable<double> data)
        {
            var dataList = data as IList<double> ?? [.. data];

            if (!dataList.Any())
            {
                throw new StatisticsError("Data is empty.");
            }

            if (dataList.Any(x => x <= 0))
            {
                throw new StatisticsError("Data contains zero or negative value.");
            }

            double product = 1.0;
            int count = dataList.Count;

            foreach (var item in dataList)
            {
                product *= item;
            }

            return Math.Pow(product, 1.0 / count);
        }




        /// <summary>
        /// Returns the harmonic mean of data, a sequence or iterable of real-valued numbers.
        /// If weights is omitted or None, then equal weighting is assumed.
        /// The harmonic mean is the reciprocal of the arithmetic mean of the reciprocals of the data.
        /// For example, the harmonic mean of three values a, b, and c will be equivalent to 3 / (1/a + 1/b + 1/c).
        /// If one of the values is zero, the result will be zero.
        /// The harmonic mean is often appropriate when averaging ratios or rates, such as speeds.
        /// </summary>
        /// <param name="data">A sequence or iterable of numeric data.</param>
        /// <param name="weights">Optional weights for the data. Must be the same length as the data if provided.</param>
        /// <returns>The harmonic mean of the data.</returns>
        /// <exception cref="StatisticsError">Thrown when the data is empty or contains negative values.</exception>
        /// <exception cref="ArgumentException">Thrown when data and weights are not of the same length.</exception>
        public static double HarmonicMean(IEnumerable<double> data, IEnumerable<double>? weights = null)
        {
            var dataList = data as IList<double> ?? [.. data];

            if (!dataList.Any())
            {
                throw new StatisticsError("Data is empty.");
            }

            if (dataList.Any(x => x < 0))
            {
                throw new StatisticsError("Data contains a negative value.");
            }

            if (weights == null)
            {
                double reciprocalSum = 0;
                int count = dataList.Count;

                foreach (var item in dataList)
                {
                    if (item == 0)
                    {
                        return 0;
                    }
                    reciprocalSum += 1.0 / item;
                }

                return count / reciprocalSum;
            }
            else
            {
                var weightList = weights as IList<double> ?? weights.ToList();

                if (dataList.Count != weightList.Count)
                {
                    throw new ArgumentException("Data and weights must be of the same length.");
                }

                double weightedReciprocalSum = 0;
                double weightSum = 0;

                for (int i = 0; i < dataList.Count; i++)
                {
                    if (dataList[i] == 0)
                    {
                        return 0;
                    }
                    weightedReciprocalSum += weightList[i] / dataList[i];
                    weightSum += weightList[i];
                }

                if (weightSum <= 0)
                {
                    throw new StatisticsError("Weighted sum is not positive.");
                }

                return weightSum / weightedReciprocalSum;
            }
        }




        public const double ToleranceKDE = 1e-8;

        // Define the delegate type
        public delegate double[] KdeFunction(double[] data, double[] evalPoints, double h, string kernel = "normal", bool cumulative = false);

        /// <summary>
        /// Calculates Kernel Density Estimation.
        /// </summary>
        /// <param name="data">The input data sample.</param>
        /// <param name="evalPoints">Points at which to evaluate the density estimation.</param>
        /// <param name="h">The bandwidth parameter controlling the degree of smoothing.</param>
        /// <param name="kernel">The kernel function to use for smoothing (default: normal/Gaussian).</param>
        /// <param name="cumulative">Whether to return a cumulative distribution function.</param>
        /// <returns>Estimated density or cumulative distribution values at the evaluation points.</returns>
        /// <exception cref="ArgumentException">Thrown if input parameters are invalid.</exception>
        public static double[] Kde(double[] data, double[] evalPoints, double h, string kernel = "normal", bool cumulative = false)
        {
            if (data == null || data.Length == 0 || evalPoints == null || evalPoints.Length == 0 || h <= 0)
            {
                throw new ArgumentException("Data and evalPoints sequences cannot be empty, and bandwidth must be positive.");
            }

            int n = data.Length;
            int m = evalPoints.Length;
            double[] kdeEstimates = new double[m];
            double invH = 1.0 / h; // Pre-calculate inverse bandwidth for efficiency

            for (int j = 0; j < m; j++)
            {
                double x = evalPoints[j]; // Current evaluation point
                double sum = 0;

                for (int i = 0; i < n; i++)
                {
                    double u = (x - data[i]) * invH; // Calculate scaled distance
                    sum += KernelFunction(u, kernel); // Apply kernel function
                }

                kdeEstimates[j] = sum * invH / n; // Calculate KDE estimate

                if (cumulative)
                {
                    kdeEstimates[j] = CumulativeDistribution(kdeEstimates, evalPoints, j); // Accurate CDF
                }
            }

            return kdeEstimates;
        }




        /// <summary>
        /// Returns the median (middle value) of numeric data, using the common "mean of middle two" method.
        /// If data is empty, StatisticsError is raised. Data can be a sequence or iterable.
        /// 
        /// The median is a robust measure of central location and is less affected by the presence of outliers.
        /// 
        /// When the number of data points is odd, the middle data point is returned:
        /// <example>
        /// <code>
        /// median(new List<int> {1, 3, 5})
        /// // Output: 3
        /// </code>
        /// </example>
        /// 
        /// When the number of data points is even, the median is interpolated by taking the average of the two middle values:
        /// <example>
        /// <code>
        /// median(new List<int> {1, 3, 5, 7})
        /// // Output: 4.0
        /// </code>
        /// </example>
        /// 
        /// This is suited for when your data is discrete, and you don't mind that the median may not be an actual data point.
        /// 
        /// If the data is ordinal (supports order operations) but not numeric (doesn't support addition), 
        /// consider using median_low() or median_high() instead.
        /// </summary>
        /// <param name="data">The input data sequence.</param>
        /// <returns>The median value.</returns>
        /// <exception cref="StatisticsError">Thrown if the data sequence is empty.</exception>
        public static double Median(IEnumerable<double> data)
        {
            if (data == null || !data.Any())
            {
                throw new StatisticsError("Data sequence cannot be empty.");
            }

            var sortedData = data.OrderBy(d => d).ToList();
            int n = sortedData.Count;
            int mid = n / 2;

            if (n % 2 == 1)
            {
                return sortedData[mid];
            }
            else
            {
                return (sortedData[mid - 1] + sortedData[mid]) / 2.0;
            }
        }




        /// <summary>
        /// Returns the low median of numeric data.
        /// If data is empty, StatisticsError is raised. Data can be a sequence or iterable.
        /// 
        /// The low median is always a member of the data set.
        /// 
        /// When the number of data points is odd, the middle value is returned:
        /// <example>
        /// <code>
        /// median_low(new List<int> {1, 3, 5})
        /// // Output: 3
        /// </code>
        /// </example>
        /// 
        /// When the number of data points is even, the smaller of the two middle values is returned:
        /// <example>
        /// <code>
        /// median_low(new List<int> {1, 3, 5, 7})
        /// // Output: 3
        /// </code>
        /// </example>
        /// 
        /// Use the low median when your data are discrete and you prefer the median to be an actual data point rather than interpolated.
        /// </summary>
        /// <param name="data">The input data sequence.</param>
        /// <returns>The low median value.</returns>
        /// <exception cref="StatisticsError">Thrown if the data sequence is empty.</exception>
        public static double MedianLow(IEnumerable<double> data)
        {
            if (data == null || !data.Any())
            {
                throw new StatisticsError("Data sequence cannot be empty.");
            }

            var sortedData = data.OrderBy(d => d).ToList();
            int n = sortedData.Count;
            int mid = n / 2;

            if (n % 2 == 1)
            {
                return sortedData[mid];
            }
            else
            {
                return sortedData[mid - 1];
            }
        }




        /// <summary>
        /// Returns the high median of numeric data.
        /// If data is empty, StatisticsError is raised. Data can be a sequence or iterable.
        /// 
        /// The high median is always a member of the data set.
        /// 
        /// When the number of data points is odd, the middle value is returned:
        /// <example>
        /// <code>
        /// median_high(new List<int> {1, 3, 5})
        /// // Output: 3
        /// </code>
        /// </example>
        /// 
        /// When the number of data points is even, the larger of the two middle values is returned:
        /// <example>
        /// <code>
        /// median_high(new List<int> {1, 3, 5, 7})
        /// // Output: 5
        /// </code>
        /// </example>
        /// 
        /// Use the high median when your data are discrete and you prefer the median to be an actual data point rather than interpolated.
        /// </summary>
        /// <param name="data">The input data sequence.</param>
        /// <returns>The high median value.</returns>
        /// <exception cref="StatisticsError">Thrown if the data sequence is empty.</exception>
        public static double MedianHigh(IEnumerable<double> data)
        {
            if (data == null || !data.Any())
            {
                throw new StatisticsError("Data sequence cannot be empty.");
            }

            var sortedData = data.OrderBy(d => d).ToList();
            int n = sortedData.Count;
            int mid = n / 2;

            if (n % 2 == 1)
            {
                return sortedData[mid];
            }
            else
            {
                return sortedData[mid];
            }
        }




        /// <summary>
        /// Estimates the median for numeric data that has been grouped or binned around the midpoints of consecutive, fixed-width intervals.
        /// The data can be any iterable of numeric data with each value being exactly the midpoint of a bin. At least one value must be present.
        /// The interval is the width of each bin.
        /// 
        /// For example, demographic information may have been summarized into consecutive ten-year age groups with each group being represented by the 5-year midpoints of the intervals:
        /// <example>
        /// <code>
        /// var demographics = new Dictionary<int, int> {
        ///    {25, 172},   // 20 to 30 years old
        ///    {35, 484},   // 30 to 40 years old
        ///    {45, 387},   // 40 to 50 years old
        ///    {55,  22},   // 50 to 60 years old
        ///    {65,   6}    // 60 to 70 years old
        /// };
        /// var data = demographics.SelectMany(d => Enumerable.Repeat(d.Key, d.Value)).ToList();
        /// median(data); // Output: 35
        /// Math.Round(median_grouped(data, 10), 1); // Output: 37.5
        /// </code>
        /// </example>
        /// 
        /// The caller is responsible for making sure the data points are separated by exact multiples of interval. This is essential for getting a correct result. The function does not check this precondition.
        /// Inputs may be any numeric type that can be coerced to a float during the interpolation step.
        /// </summary>
        /// <param name="data">The input data sequence.</param>
        /// <param name="interval">The width of each bin (default is 1.0).</param>
        /// <returns>The estimated median value.</returns>
        /// <exception cref="StatisticsError">Thrown if the data sequence is empty.</exception>
        public static double MedianGrouped(IEnumerable<double> data, double interval = 1.0)
        {
            if (data == null || !data.Any())
            {
                throw new StatisticsError("Data sequence cannot be empty.");
            }

            var sortedData = data.OrderBy(d => d).ToList();
            int n = sortedData.Count;
            int mid = n / 2;

            double L = sortedData[mid - 1]; // Lower class boundary of the median class
            double F = sortedData.Count(d => d < L); // Cumulative frequency up to the median class
            double f = sortedData.Count(d => d == L); // Frequency of the median class

            return L + (n / 2.0 - F) / f * interval;
        }




        /// <summary>
        /// Returns the single most common data point from discrete or nominal data.
        /// The mode (when it exists) is the most typical value and serves as a measure of central location.
        /// If there are multiple modes with the same frequency, returns the first one encountered in the data.
        /// If the smallest or largest of those is desired instead, use min(multimode(data)) or max(multimode(data)).
        /// If the input data is empty, StatisticsError is raised.
        /// 
        /// mode assumes discrete data and returns a single value. This is the standard treatment of the mode as commonly taught in schools:
        /// <example>
        /// <code>
        /// mode(new List<int> {1, 1, 2, 3, 3, 3, 3, 4})
        /// // Output: 3
        /// </code>
        /// </example>
        /// 
        /// The mode is unique in that it is the only statistic in this package that also applies to nominal (non-numeric) data:
        /// <example>
        /// <code>
        /// mode(new List<string> {"red", "blue", "blue", "red", "green", "red", "red"})
        /// // Output: "red"
        /// </code>
        /// </example>
        /// 
        /// Only hashable inputs are supported. To handle type set, consider casting to frozenset. To handle type list, consider casting to tuple.
        /// For mixed or nested inputs, consider using this slower quadratic algorithm that only depends on equality tests: max(data, key=data.count).
        /// </summary>
        /// <param name="data">The input data sequence.</param>
        /// <returns>The mode value.</returns>
        /// <exception cref="StatisticsError">Thrown if the data sequence is empty or if no mode exists.</exception>
        public static T Mode<T>(IEnumerable<T> data) where T : notnull
        {
            if (data == null || !data.Any())
            {
                throw new StatisticsError("Data sequence cannot be empty.");
            }

            var frequency = new Dictionary<T, int>();
            foreach (var item in data)
            {
                if (frequency.ContainsKey(item))
                {
                    frequency[item]++;
                }
                else
                {
                    frequency[item] = 1;
                }
            }

            int maxCount = frequency.Values.Max();
            T mode = frequency.First(f => f.Value == maxCount).Key;

            return mode;
        }




        /// <summary>
        /// Returns a list of the most frequently occurring values in the order they were first encountered in the data.
        /// Will return more than one result if there are multiple modes or an empty list if the data is empty.
        /// <example>
        /// <code>
        /// multimode("aabbbbccddddeeffffgg")
        /// // Output: ['b', 'd', 'f']
        /// </code>
        /// <code>
        /// multimode("")
        /// // Output: []
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="data">The input data sequence.</param>
        /// <returns>A list of the most frequently occurring values.</returns>
        /// <exception cref="StatisticsError">Thrown if the data sequence is empty.</exception>
        public static List<T> Multimode<T>(IEnumerable<T> data) where T : notnull
        {
            if (data == null || !data.Any())
            {
                throw new StatisticsError("Data sequence cannot be empty.");
            }

            var frequency = new Dictionary<T, int>();
            foreach (var item in data)
            {
                if (frequency.ContainsKey(item))
                {
                    frequency[item]++;
                }
                else
                {
                    frequency[item] = 1;
                }
            }

            int maxCount = frequency.Values.Max();
            return frequency.Where(f => f.Value == maxCount).Select(f => f.Key).ToList();
        }




        /// <summary>
        /// Divides data into n continuous intervals with equal probability. Returns a list of n - 1 cut points separating the intervals.
        /// 
        /// Set n to 4 for quartiles (the default). Set n to 10 for deciles. Set n to 100 for percentiles which gives the 99 cut points that separate data into 100 equal-sized groups. Raises StatisticsError if n is less than 1.
        /// 
        /// The data can be any iterable containing sample data. For meaningful results, the number of data points in data should be larger than n. Raises StatisticsError if there is not at least one data point.
        /// 
        /// The cut points are linearly interpolated from the two nearest data points. For example, if a cut point falls one-third of the distance between two sample values, 100 and 112, the cut-point will evaluate to 104.
        /// 
        /// The method for computing quantiles can be varied depending on whether the data includes or excludes the lowest and highest possible values from the population.
        /// 
        /// The default method is “exclusive” and is used for data sampled from a population that can have more extreme values than found in the samples. The portion of the population falling below the i-th of m sorted data points is computed as i / (m + 1). Given nine sample values, the method sorts them and assigns the following percentiles: 10%, 20%, 30%, 40%, 50%, 60%, 70%, 80%, 90%.
        /// 
        /// Setting the method to “inclusive” is used for describing population data or for samples that are known to include the most extreme values from the population. The minimum value in data is treated as the 0th percentile and the maximum value is treated as the 100th percentile. The portion of the population falling below the i-th of m sorted data points is computed as (i - 1) / (m - 1). Given 11 sample values, the method sorts them and assigns the following percentiles: 0%, 10%, 20%, 30%, 40%, 50%, 60%, 70%, 80%, 90%, 100%.
        /// <example>
        /// <code>
        /// var data = new List<double> {105, 129, 87, 86, 111, 111, 89, 81, 108, 92, 110, 100, 75, 105, 103, 109, 76, 119, 99, 91, 103, 129, 106, 101, 84, 111, 74, 87, 86, 103, 103, 106, 86, 111, 75, 87, 102, 121, 111, 88, 89, 101, 106, 95, 103, 107, 101, 81, 109, 104};
        /// var quartiles = Quantiles(data, n: 4);
        /// Console.WriteLine(string.Join(", ", quartiles));
        /// // Output: [86.0, 103.0, 111.0]
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="data">The input data sequence.</param>
        /// <param name="n">The number of intervals (default is 4).</param>
        /// <param name="method">The method for computing quantiles (default is "exclusive").</param>
        /// <returns>A list of n - 1 cut points separating the intervals.</returns>
        /// <exception cref="StatisticsError">Thrown if n is less than 1 or if the data sequence is empty.</exception>
        public static List<double> Quantiles(IEnumerable<double> data, int n = 4, string method = "exclusive")
        {
            if (n < 1)
            {
                throw new StatisticsError("n must be at least 1.");
            }

            if (data == null || !data.Any())
            {
                throw new StatisticsError("Data sequence cannot be empty.");
            }

            var sortedData = data.OrderBy(d => d).ToList();
            int m = sortedData.Count;
            List<double> cutPoints = new List<double>();

            for (int i = 1; i < n; i++)
            {
                double p;
                if (method == "exclusive")
                {
                    p = i / (double)(n + 1);
                }
                else if (method == "inclusive")
                {
                    p = (i - 1) / (double)(n - 1);
                }
                else
                {
                    throw new ArgumentException("Invalid method specified. Use 'exclusive' or 'inclusive'.");
                }

                double index = p * (m - 1);
                int lower = (int)Math.Floor(index);
                int upper = (int)Math.Ceiling(index);
                double fraction = index - lower;

                double cutPoint = sortedData[lower] + fraction * (sortedData[upper] - sortedData[lower]);
                cutPoints.Add(cutPoint);
            }

            return cutPoints;
        }




        /// <summary>
        /// Returns the population standard deviation (the square root of the population variance).
        /// See pvariance() for arguments and other details.
        /// <example>
        /// <code>
        /// pstdev(new List<double> {1.5, 2.5, 2.5, 2.75, 3.25, 4.75})
        /// // Output: 0.986893273527251
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="data">The input data sequence.</param>
        /// <param name="mu">The population mean (optional). If not provided, it will be calculated from the data.</param>
        /// <returns>The population standard deviation.</returns>
        /// <exception cref="StatisticsError">Thrown if the data sequence is empty.</exception>
        public static double Pstdev(IEnumerable<double> data, double? mu = null)
        {
            if (data == null || !data.Any())
            {
                throw new StatisticsError("Data sequence cannot be empty.");
            }

            double mean = mu ?? data.Average();
            double variance = data.Select(x => Math.Pow(x - mean, 2)).Average();
            return Math.Sqrt(variance);
        }




        /// <summary>
        /// Returns the population variance of data, a non-empty sequence or iterable of real-valued numbers.
        /// Variance, or second moment about the mean, is a measure of the variability (spread or dispersion) of data.
        /// A large variance indicates that the data is spread out; a small variance indicates it is clustered closely around the mean.
        /// 
        /// If the optional second argument mu is given, it should be the population mean of the data.
        /// It can also be used to compute the second moment around a point that is not the mean.
        /// If it is missing or None (the default), the arithmetic mean is automatically calculated.
        /// 
        /// Use this function to calculate the variance from the entire population.
        /// To estimate the variance from a sample, the variance() function is usually a better choice.
        /// 
        /// Raises StatisticsError if data is empty.
        /// 
        /// Examples:
        /// <example>
        /// <code>
        /// var data = new List<double> {0.0, 0.25, 0.25, 1.25, 1.5, 1.75, 2.75, 3.25};
        /// Console.WriteLine(pvariance(data));
        /// // Output: 1.25
        /// 
        /// var mu = data.Average();
        /// Console.WriteLine(pvariance(data, mu));
        /// // Output: 1.25
        /// 
        /// var decimalData = new List<decimal> {27.5m, 30.25m, 30.25m, 34.5m, 41.75m};
        /// Console.WriteLine(pvariance(decimalData));
        /// // Output: 24.815
        /// 
        /// var fractionData = new List<Fraction> {new Fraction(1, 4), new Fraction(5, 4), new Fraction(1, 2)};
        /// Console.WriteLine(pvariance(fractionData));
        /// // Output: new Fraction(13, 72)
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="data">The input data sequence.</param>
        /// <param name="mu">The population mean (optional). If not provided, it will be calculated from the data.</param>
        /// <returns>The population variance.</returns>
        /// <exception cref="StatisticsError">Thrown if the data sequence is empty.</exception>
        public static double Pvariance(IEnumerable<double> data, double? mu = null)
        {
            if (data == null || !data.Any())
            {
                throw new StatisticsError("Data sequence cannot be empty.");
            }

            double mean = mu ?? data.Average();
            double variance = data.Select(x => Math.Pow(x - mean, 2)).Average();
            return variance;
        }




        /// <summary>
        /// Returns the sample standard deviation (the square root of the sample variance).
        /// See variance() for arguments and other details.
        /// <example>
        /// <code>
        /// stdev(new List<double> {1.5, 2.5, 2.5, 2.75, 3.25, 4.75})
        /// // Output: 1.0810874155219827
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="data">The input data sequence.</param>
        /// <param name="xbar">The sample mean (optional). If not provided, it will be calculated from the data.</param>
        /// <returns>The sample standard deviation.</returns>
        /// <exception cref="StatisticsError">Thrown if the data sequence is empty or has fewer than two values.</exception>
        public static double Stdev(IEnumerable<double> data, double? xbar = null)
        {
            if (data == null || !data.Any() || data.Count() < 2)
            {
                throw new StatisticsError("Data sequence must have at least two values.");
            }

            double mean = xbar ?? data.Average();
            double variance = data.Select(x => Math.Pow(x - mean, 2)).Sum() / (data.Count() - 1);
            return Math.Sqrt(variance);
        }




        /// <summary>
        /// Returns the sample variance of data, an iterable of at least two real-valued numbers.
        /// Variance, or second moment about the mean, is a measure of the variability (spread or dispersion) of data.
        /// A large variance indicates that the data is spread out; a small variance indicates it is clustered closely around the mean.
        /// 
        /// If the optional second argument xbar is given, it should be the sample mean of data.
        /// If it is missing or None (the default), the mean is automatically calculated.
        /// 
        /// Use this function when your data is a sample from a population.
        /// To calculate the variance from the entire population, see pvariance().
        /// 
        /// Raises StatisticsError if data has fewer than two values.
        /// <example>
        /// <code>
        /// var data = new List<double> {2.75, 1.75, 1.25, 0.25, 0.5, 1.25, 3.5};
        /// Console.WriteLine(variance(data));
        /// // Output: 1.3720238095238095
        /// 
        /// var m = data.Average();
        /// Console.WriteLine(variance(data, m));
        /// // Output: 1.3720238095238095
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="data">The input data sequence.</param>
        /// <param name="xbar">The sample mean (optional). If not provided, it will be calculated from the data.</param>
        /// <returns>The sample variance.</returns>
        /// <exception cref="StatisticsError">Thrown if the data sequence is empty or has fewer than two values.</exception>
        public static double Variance(IEnumerable<double> data, double? xbar = null)
        {
            if (data == null || !data.Any() || data.Count() < 2)
            {
                throw new StatisticsError("Data sequence must have at least two values.");
            }

            double mean = xbar ?? data.Average();
            return data.Select(x => Math.Pow(x - mean, 2)).Sum() / (data.Count() - 1);
        }




        /// <summary>
        /// Returns the sample covariance of two inputs x and y.
        /// Covariance is a measure of the joint variability of two inputs.
        /// Both inputs must be of the same length (no less than two), otherwise StatisticsError is raised.
        /// <example>
        /// <code>
        /// var x = new List<double> {1, 2, 3, 4, 5, 6, 7, 8, 9};
        /// var y = new List<double> {1, 2, 3, 1, 2, 3, 1, 2, 3};
        /// Console.WriteLine(covariance(x, y));
        /// // Output: 0.75
        /// 
        /// var z = new List<double> {9, 8, 7, 6, 5, 4, 3, 2, 1};
        /// Console.WriteLine(covariance(x, z));
        /// // Output: -7.5
        /// Console.WriteLine(covariance(z, x));
        /// // Output: -7.5
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="x">The first input data sequence.</param>
        /// <param name="y">The second input data sequence.</param>
        /// <returns>The sample covariance.</returns>
        /// <exception cref="StatisticsError">Thrown if the inputs are not of the same length or have fewer than two values.</exception>
        public static double Covariance(IEnumerable<double> x, IEnumerable<double> y)
        {
            if (x == null || y == null || x.Count() < 2 || y.Count() < 2 || x.Count() != y.Count())
            {
                throw new StatisticsError("Both inputs must be of the same length and have at least two values.");
            }

            double meanX = x.Average();
            double meanY = y.Average();

            double covariance = x.Zip(y, (xi, yi) => (xi - meanX) * (yi - meanY)).Sum() / (x.Count() - 1);

            return covariance;
        }




        /// <summary>
        /// Returns the Pearson's correlation coefficient for two inputs. Pearson's correlation coefficient r takes values between -1 and +1.
        /// It measures the strength and direction of a linear relationship.
        /// 
        /// If method is “ranked”, computes Spearman's rank correlation coefficient for two inputs.
        /// The data is replaced by ranks. Ties are averaged so that equal values receive the same rank.
        /// The resulting coefficient measures the strength of a monotonic relationship.
        /// 
        /// Spearman's correlation coefficient is appropriate for ordinal data or for continuous data that doesn't meet the linear proportion requirement for Pearson's correlation coefficient.
        /// 
        /// Both inputs must be of the same length (no less than two), and need not be constant, otherwise StatisticsError is raised.
        /// 
        /// Example with Aryabhata's planetary motion law:
        /// <example>
        /// <code>
        /// var orbital_period = new List<double> {88, 225, 365, 687, 4331, 10756, 30687, 60190}; // days
        /// var dist_from_sun = new List<double> {58, 108, 150, 228, 778, 1400, 2900, 4500}; // million km
        /// // Calculate Pearson's correlation coefficient
        /// Console.WriteLine(correlation(orbital_period, dist_from_sun));
        /// // Output: 0.9882
        /// 
        /// // Demonstrate Aryabhata's law: There is a linear correlation between the square of the orbital period and the cube of the distance from the sun.
        /// var period_squared = orbital_period.Select(p => p * p).ToList();
        /// var dist_cubed = dist_from_sun.Select(d => d * d * d).ToList();
        /// Console.WriteLine(correlation(period_squared, dist_cubed));
        /// // Output: 1.0
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="x">The first input data sequence.</param>
        /// <param name="y">The second input data sequence.</param>
        /// <param name="method">The method for computing the correlation ("linear" or "ranked"). Default is "linear".</param>
        /// <returns>The Pearson's or Spearman's correlation coefficient.</returns>
        /// <exception cref="StatisticsError">Thrown if the inputs are not of the same length or have fewer than two values.</exception>
        public static double Correlation(IEnumerable<double> x, IEnumerable<double> y, string method = "linear")
        {
            if (x == null || y == null || x.Count() < 2 || y.Count() < 2 || x.Count() != y.Count())
            {
                throw new StatisticsError("Both inputs must be of the same length and have at least two values.");
            }

            if (method == "ranked")
            {
                x = RankData(x);
                y = RankData(y);
            }

            double meanX = x.Average();
            double meanY = y.Average();

            double covariance = x.Zip(y, (xi, yi) => (xi - meanX) * (yi - meanY)).Sum() / (x.Count() - 1);
            double stdevX = Math.Sqrt(x.Select(xi => Math.Pow(xi - meanX, 2)).Sum() / (x.Count() - 1));
            double stdevY = Math.Sqrt(y.Select(yi => Math.Pow(yi - meanY, 2)).Sum() / (y.Count() - 1));

            return covariance / (stdevX * stdevY);
        }

        private static IEnumerable<double> RankData(IEnumerable<double> data)
        {
            var sorted = data.Select((value, index) => new { Value = value, Index = index })
                             .OrderBy(x => x.Value)
                             .ToList();

            double[] ranks = new double[data.Count()];
            int length = sorted.Count;

            for (int i = 0; i < length; i++)
            {
                if (i == length - 1 || sorted[i].Value != sorted[i + 1].Value)
                {
                    ranks[sorted[i].Index] = i + 1;
                }
                else
                {
                    int start = i;
                    while (i < length - 1 && sorted[i].Value == sorted[i + 1].Value)
                    {
                        i++;
                    }
                    double rank = (start + i + 2) / 2.0;
                    for (int j = start; j <= i; j++)
                    {
                        ranks[sorted[j].Index] = rank;
                    }
                }
            }

            return ranks;
        }




        /// <summary>
        /// Returns the slope and intercept of simple linear regression parameters estimated using ordinary least squares.
        /// Simple linear regression describes the relationship between an independent variable x and a dependent variable y in terms of this linear function:
        /// y = slope * x + intercept + noise
        /// where slope and intercept are the regression parameters that are estimated, and noise represents the variability of the data that was not explained by the linear regression (it is equal to the difference between predicted and actual values of the dependent variable).
        /// Both inputs must be of the same length (no less than two), and the independent variable x cannot be constant; otherwise a StatisticsError is raised.
        /// 
        /// Example with Monty Python films:
        /// <example>
        /// <code>
        /// var year = new List<double> {1971, 1975, 1979, 1982, 1983};
        /// var films_total = new List<double> {1, 2, 3, 4, 5};
        /// var (slope, intercept) = LinearRegression(year, films_total);
        /// Console.WriteLine(Math.Round(slope * 2019 + intercept)); // Output: 16
        /// </code>
        /// </example>
        /// 
        /// Example with Aryabhata's planetary motion law:
        /// <example>
        /// <code>
        /// var orbital_period = new List<double> {88, 225, 365, 687, 4331, 10756, 30687, 60190}; // days
        /// var dist_from_sun = new List<double> {58, 108, 150, 228, 778, 1400, 2900, 4500}; // million km
        /// // Calculate linear regression
        /// var (slope, intercept) = LinearRegression(orbital_period, dist_from_sun);
        /// Console.WriteLine(correlation(orbital_period, dist_from_sun)); // Output: 0.9882
        /// 
        /// var period_squared = orbital_period.Select(p => p * p).ToList();
        /// var dist_cubed = dist_from_sun.Select(d => d * d * d).ToList();
        /// var (slope, intercept) = LinearRegression(period_squared, dist_cubed, proportional: true);
        /// Console.WriteLine(slope); // Output: Proportional slope
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="x">The independent variable data sequence.</param>
        /// <param name="y">The dependent variable data sequence.</param>
        /// <param name="proportional">If true, the data is fit to a line passing through the origin (intercept is 0). Default is false.</param>
        /// <returns>A tuple containing the slope and intercept of the linear regression line.</returns>
        /// <exception cref="StatisticsError">Thrown if the inputs are not of the same length, have fewer than two values, or if x is constant.</exception>
        public static (double Slope, double Intercept) LinearRegression(IEnumerable<double> x, IEnumerable<double> y, bool proportional = false)
        {
            if (x == null || y == null || x.Count() < 2 || y.Count() < 2 || x.Count() != y.Count())
            {
                throw new StatisticsError("Both inputs must be of the same length and have at least two values.");
            }

            double meanX = x.Average();
            double meanY = y.Average();

            if (proportional)
            {
                double slope = x.Zip(y, (xi, yi) => xi * yi).Sum() / x.Select(xi => xi * xi).Sum();
                return (slope, 0);
            }
            else
            {
                double covariance = x.Zip(y, (xi, yi) => (xi - meanX) * (yi - meanY)).Sum();
                double varianceX = x.Select(xi => Math.Pow(xi - meanX, 2)).Sum();

                double slope = covariance / varianceX;
                double intercept = meanY - slope * meanX;

                return (slope, intercept);
            }
        }




        /// <summary>
        /// Returns a new NormalDist object where mu represents the arithmetic mean and sigma represents the standard deviation.
        /// If sigma is negative, raises StatisticsError.
        /// </summary>
        public class NormalDist
        {
            public double Mean { get; }
            public double StandardDeviation { get; }

            /// <summary>
            /// Initializes a new instance of the NormalDist class with specified mean and standard deviation.
            /// </summary>
            /// <param name="mu">The arithmetic mean (default is 0.0).</param>
            /// <param name="sigma">The standard deviation (default is 1.0).</param>
            /// <exception cref="StatisticsError">Thrown if sigma is negative.</exception>
            public NormalDist(double mu = 0.0, double sigma = 1.0)
            {
                if (sigma < 0)
                {
                    throw new StatisticsError("Standard deviation (sigma) cannot be negative.");
                }

                Mean = mu;
                StandardDeviation = sigma;
            }

            /// <summary>
            /// Returns the probability density function (pdf) value for the given x.
            /// </summary>
            /// <param name="x">The value for which to calculate the pdf.</param>
            /// <returns>The pdf value at x.</returns>
            public double Pdf(double x)
            {
                double expPart = Math.Exp(-0.5 * Math.Pow((x - Mean) / StandardDeviation, 2));
                double denominator = StandardDeviation * Math.Sqrt(2 * Math.PI);
                return expPart / denominator;
            }

            /// <summary>
            /// Returns the cumulative distribution function (cdf) value for the given x.
            /// </summary>
            /// <param name="x">The value for which to calculate the cdf.</param>
            /// <returns>The cdf value at x.</returns>
            public double Cdf(double x)
            {
                return 0.5 * (1 + Erf((x - Mean) / (StandardDeviation * Math.Sqrt(2))));
            }

            /// <summary>
            /// Calculates the error function value for the given z.
            /// </summary>
            /// <param name="z">The value for which to calculate the error function.</param>
            /// <returns>The error function value at z.</returns>
            private double Erf(double z)
            {
                // Approximation for the error function
                double t = 1.0 / (1.0 + 0.5 * Math.Abs(z));
                double erf = 1 - t * Math.Exp(-z * z - 1.26551223 +
                                     t * (1.00002368 +
                                     t * (0.37409196 +
                                     t * (0.09678418 +
                                     t * (-0.18628806 +
                                     t * (0.27886807 +
                                     t * (-1.13520398 +
                                     t * (1.48851587 +
                                     t * (-0.82215223 +
                                     t * 0.17087277)))))))));
                return z >= 0 ? erf : -erf;
            }
        }




        /// <summary>
        /// Returns the binomial coefficient, also known as "n choose k".
        /// The binomial coefficient is a measure of the number of ways to choose k items from n items without regard to the order.
        /// It is defined as:
        /// 
        /// C(n, k) = n! / (k! * (n - k)!)
        /// 
        /// <example>
        /// <code>
        /// Console.WriteLine(Binomial(5, 2)); // Output: 10
        /// Console.WriteLine(Binomial(10, 3)); // Output: 120
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="n">The total number of items.</param>
        /// <param name="k">The number of items to choose.</param>
        /// <returns>The binomial coefficient, "n choose k".</returns>
        /// <exception cref="ArgumentException">Thrown if n or k is negative, or if k is greater than n.</exception>
        public static long Binomial(int n, int k)
        {
            if (n < 0 || k < 0)
            {
                throw new ArgumentException("n and k must be non-negative.");
            }
            if (k > n)
            {
                throw new ArgumentException("k must not be greater than n.");
            }

            if (k == 0 || k == n)
            {
                return 1;
            }

            if (k > n - k)
            {
                k = n - k;
            }

            long result = 1;
            for (int i = 0; i < k; i++)
            {
                result *= n - i;
                result /= i + 1;
            }

            return result;
        }




        /// <summary>
        /// Returns the multinomial coefficient for given counts.
        /// The multinomial coefficient is a generalization of the binomial coefficient,
        /// representing the number of ways to partition a set of n items into r groups with sizes k1, k2, ..., kr.
        /// It is defined as:
        /// 
        /// C(n; k1, k2, ..., kr) = n! / (k1! * k2! * ... * kr!)
        /// 
        /// <example>
        /// <code>
        /// Console.WriteLine(Multinomial(6, new List<int> {2, 2, 2})); // Output: 90
        /// Console.WriteLine(Multinomial(10, new List<int> {2, 3, 5})); // Output: 2520
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="n">The total number of items.</param>
        /// <param name="counts">A list of integers representing the sizes of the groups.</param>
        /// <returns>The multinomial coefficient.</returns>
        /// <exception cref="ArgumentException">Thrown if n or any of the counts are negative, or if the sum of counts is not equal to n.</exception>
        public static long Multinomial(int n, List<int> counts)
        {
            if (n < 0 || counts.Any(k => k < 0) || counts.Sum() != n)
            {
                throw new ArgumentException("n and counts must be non-negative, and the sum of counts must equal n.");
            }

            long numerator = Factorial(n);
            long denominator = counts.Select(k => Factorial(k)).Aggregate((a, b) => a * b);

            return numerator / denominator;
        }




        /// <summary>
        /// Returns the value of the Beta function B(x, y).
        /// The Beta function is defined as:
        /// 
        /// B(x, y) = ∫(0 to 1) t^(x-1) * (1 - t)^(y-1) dt
        /// 
        /// It can also be computed using the Gamma function:
        /// 
        /// B(x, y) = Γ(x) * Γ(y) / Γ(x + y)
        /// 
        /// <example>
        /// <code>
        /// Console.WriteLine(Beta(0.5, 0.5)); // Output: 3.141592653589793
        /// Console.WriteLine(Beta(2, 3)); // Output: 0.08333333333333333
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="x">The first parameter of the Beta function.</param>
        /// <param name="y">The second parameter of the Beta function.</param>
        /// <returns>The value of the Beta function B(x, y).</returns>
        /// <exception cref="ArgumentException">Thrown if x or y is non-positive.</exception>
        public static double Beta(double x, double y)
        {
            if (x <= 0 || y <= 0)
            {
                throw new ArgumentException("x and y must be positive.");
            }
            return Gamma(x) * Gamma(y) / Gamma(x + y);
        }




        /// <summary>
        /// Returns the value of the polygamma function ψ^(n)(x), which is the (n+1)th derivative of the logarithm of the gamma function.
        /// The polygamma function is defined as:
        /// 
        /// ψ^(n)(x) = d^(n+1)/dx^(n+1) [ln(Γ(x))]
        /// 
        /// <example>
        /// <code>
        /// Console.WriteLine(Polygamma(0, 1)); // Output: -0.57721566490153286060 (Euler-Mascheroni constant)
        /// Console.WriteLine(Polygamma(1, 1)); // Output: π^2/6
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="n">The order of the polygamma function.</param>
        /// <param name="x">The value at which to evaluate the polygamma function.</param>
        /// <returns>The value of the polygamma function ψ^(n)(x).</returns>
        /// <exception cref="ArgumentException">Thrown if n is negative or if x is non-positive.</exception>
        public static double Polygamma(int n, double x)
        {
            if (n < 0)
            {
                throw new ArgumentException("n must be non-negative.");
            }
            if (x <= 0)
            {
                throw new ArgumentException("x must be positive.");
            }

            if (n == 0)
            {
                return Digamma(x);
            }

            // Using recursion to compute the polygamma function of order n
            double result = 0.0;
            for (int k = 0; k <= n; k++)
            {
                result += BinomialCoefficient(n, k) * StirlingNumberSecondKind(n + 1, k + 1) * (Math.Pow(-1, k) / Math.Pow(x, k + 1));
            }

            return result;
        }




        /// <summary>
        /// Returns the value of the Riemann zeta function ζ(x).
        /// The Riemann zeta function is defined as:
        /// 
        /// ζ(x) = Σ(1/n^x) for n=1 to ∞
        /// 
        /// <example>
        /// <code>
        /// Console.WriteLine(Zeta(2)); // Output: 1.6449340668482264 (π^2 / 6)
        /// Console.WriteLine(Zeta(3)); // Output: 1.2020569031595942
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="x">The input value for the Riemann zeta function.</param>
        /// <returns>The value of the Riemann zeta function ζ(x).</returns>
        /// <exception cref="ArgumentException">Thrown if x is less than or equal to 1.</exception>
        public static double Zeta(double x)
        {
            if (x <= 1)
            {
                throw new ArgumentException("x must be greater than 1.");
            }

            const int MAX_ITERATIONS = 1000000;
            double sum = 0.0;

            for (int n = 1; n <= MAX_ITERATIONS; n++)
            {
                sum += 1.0 / Math.Pow(n, x);
            }

            return sum;
        }




        /// <summary>
        /// Returns the Pearson's correlation coefficient for two inputs.
        /// Pearson's correlation coefficient r takes values between -1 and +1.
        /// It measures the strength and direction of a linear relationship.
        /// <example>
        /// <code>
        /// var x = new List<double> {1, 2, 3, 4, 5, 6, 7, 8, 9};
        /// var y = new List<double> {9, 8, 7, 6, 5, 4, 3, 2, 1};
        /// Console.WriteLine(PearsonCorrelation(x, y)); // Output: -1
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="x">The first input data sequence.</param>
        /// <param name="y">The second input data sequence.</param>
        /// <returns>The Pearson's correlation coefficient.</returns>
        /// <exception cref="StatisticsError">Thrown if the inputs are not of the same length or have fewer than two values.</exception>
        public static double PearsonCorrelation(IEnumerable<double> x, IEnumerable<double> y)
        {
            if (x == null || y == null || x.Count() < 2 || y.Count() < 2 || x.Count() != y.Count())
            {
                throw new StatisticsError("Both inputs must be of the same length and have at least two values.");
            }

            double meanX = x.Average();
            double meanY = y.Average();

            double covariance = x.Zip(y, (xi, yi) => (xi - meanX) * (yi - meanY)).Sum();
            double stdevX = Math.Sqrt(x.Select(xi => Math.Pow(xi - meanX, 2)).Sum());
            double stdevY = Math.Sqrt(y.Select(yi => Math.Pow(yi - meanY, 2)).Sum());

            return covariance / (stdevX * stdevY);
        }




        /// <summary>
        /// Returns the range of a data set.
        /// The range is the difference between the maximum and minimum values in the data set.
        /// <example>
        /// <code>
        /// var data = new List<double> {1, 2, 3, 4, 5, 6, 7, 8, 9};
        /// Console.WriteLine(Range(data)); // Output: 8
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="data">The input data sequence.</param>
        /// <returns>The range of the data set.</returns>
        /// <exception cref="StatisticsError">Thrown if the data sequence is empty.</exception>
        public static double Range(IEnumerable<double> data)
        {
            if (data == null || !data.Any())
            {
                throw new StatisticsError("Data sequence cannot be empty.");
            }

            double min = data.Min();
            double max = data.Max();

            return max - min;
        }




        /// <summary>
        /// Returns the root mean square (RMS) of a data set.
        /// The RMS is the square root of the average of the squares of a set of values.
        /// <example>
        /// <code>
        /// var data = new List<double> {1, 2, 3, 4, 5, 6, 7, 8, 9};
        /// Console.WriteLine(RootMeanSquare(data)); // Output: 5.744562646538029
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="data">The input data sequence.</param>
        /// <returns>The root mean square of the data set.</returns>
        /// <exception cref="StatisticsError">Thrown if the data sequence is empty.</exception>
        public static double RootMeanSquare(IEnumerable<double> data)
        {
            if (data == null || !data.Any())
            {
                throw new StatisticsError("Data sequence cannot be empty.");
            }

            double sumOfSquares = data.Select(x => x * x).Sum();
            double meanOfSquares = sumOfSquares / data.Count();
            return Math.Sqrt(meanOfSquares);
        }




        /// <summary>
        /// Returns the slope and intercept of the best-fitting line for a set of data points using the least squares method.
        /// The line is of the form y = slope * x + intercept.
        /// <example>
        /// <code>
        /// var x = new List<double> {1, 2, 3, 4, 5};
        /// var y = new List<double> {2, 3, 5, 7, 11};
        /// var (slope, intercept) = LeastSquaresLinearRegression(x, y);
        /// Console.WriteLine($"Slope: {slope}, Intercept: {intercept}");
        /// // Output: Slope: 2.2, Intercept: -0.4
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="x">The independent variable data sequence.</param>
        /// <param name="y">The dependent variable data sequence.</param>
        /// <returns>A tuple containing the slope and intercept of the best-fitting line.</returns>
        /// <exception cref="StatisticsError">Thrown if the inputs are not of the same length or have fewer than two values, or if x is constant.</exception>
        public static (double Slope, double Intercept) LeastSquaresLinearRegression(IEnumerable<double> x, IEnumerable<double> y)
        {
            if (x == null || y == null || x.Count() < 2 || y.Count() < 2 || x.Count() != y.Count())
            {
                throw new StatisticsError("Both inputs must be of the same length and have at least two values.");
            }

            double meanX = x.Average();
            double meanY = y.Average();

            double numerator = x.Zip(y, (xi, yi) => (xi - meanX) * (yi - meanY)).Sum();
            double denominator = x.Select(xi => Math.Pow(xi - meanX, 2)).Sum();

            if (denominator == 0)
            {
                throw new StatisticsError("The independent variable x cannot be constant.");
            }

            double slope = numerator / denominator;
            double intercept = meanY - slope * meanX;

            return (slope, intercept);
        }




        /// <summary>
        /// Returns a dictionary where the keys are the unique values from the data set and the values are the counts of each value.
        /// <example>
        /// <code>
        /// var data = new List<int> {1, 2, 2, 3, 3, 3, 4, 4, 4, 4};
        /// var counts = CountEach(data);
        /// foreach (var kvp in counts)
        /// {
        ///     Console.WriteLine($"Value: {kvp.Key}, Count: {kvp.Value}");
        /// }
        /// // Output:
        /// // Value: 1, Count: 1
        /// // Value: 2, Count: 2
        /// // Value: 3, Count: 3
        /// // Value: 4, Count: 4
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="data">The input data sequence.</param>
        /// <returns>A dictionary where keys are unique values and values are their counts.</returns>
        /// <exception cref="StatisticsError">Thrown if the data sequence is empty.</exception>
        public static Dictionary<T, int> CountEach<T>(IEnumerable<T> data) where T : notnull
        {
            if (data == null || !data.Any())
            {
                throw new StatisticsError("Data sequence cannot be empty.");
            }

            var counts = new Dictionary<T, int>();
            foreach (var item in data)
            {
                if (counts.TryGetValue(item, out int value))
                {
                    counts[item] = ++value;
                }
                else
                {
                    counts[item] = 1;
                }
            }

            return counts;
        }




        /// <summary>
        /// Creates a histogram by counting the frequency of each value in a data set and grouping them into bins.
        /// The function returns a dictionary where the keys are bin ranges and the values are the counts of items within those ranges.
        /// <example>
        /// <code>
        /// var data = new List<int> {1, 2, 2, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5, 5, 5};
        /// var histogram = Histogram(data, 5);
        /// foreach (var kvp in histogram)
        /// {
        ///     Console.WriteLine($"Range: {kvp.Key}, Count: {kvp.Value}");
        /// }
        /// // Output:
        /// // Range: 1-2, Count: 3
        /// // Range: 3-4, Count: 7
        /// // Range: 5-6, Count: 5
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="data">The input data sequence.</param>
        /// <param name="binCount">The number of bins.</param>
        /// <returns>A dictionary where keys are bin ranges and values are their counts.</returns>
        /// <exception cref="StatisticsError">Thrown if the data sequence is empty or bin count is less than 1.</exception>
        public static Dictionary<string, int> Histogram(IEnumerable<int> data, int binCount)
        {
            if (data == null || !data.Any())
            {
                throw new StatisticsError("Data sequence cannot be empty.");
            }

            if (binCount < 1)
            {
                throw new StatisticsError("Bin count must be at least 1.");
            }

            int min = data.Min();
            int max = data.Max();
            int binSize = (max - min + 1) / binCount;

            var histogram = new Dictionary<string, int>();

            for (int i = 0; i < binCount; i++)
            {
                int binStart = min + i * binSize;
                int binEnd = (i == binCount - 1) ? max : binStart + binSize - 1;
                string binRange = $"{binStart}-{binEnd}";
                histogram[binRange] = 0;
            }

            foreach (var value in data)
            {
                foreach (var binRange in histogram.Keys.ToList())
                {
                    var parts = binRange.Split('-');
                    int binStart = int.Parse(parts[0]);
                    int binEnd = int.Parse(parts[1]);

                    if (value >= binStart && value <= binEnd)
                    {
                        histogram[binRange]++;
                        break;
                    }
                }
            }

            return histogram;
        }






















        /// <summary>
        /// Calculates the Cumulative Distribution Function (CDF) using the trapezoidal rule.
        /// </summary>
        /// <param name="kdeEstimates">The KDE estimates (PDF values) at the evalPoints.</param>
        /// <param name="evalPoints">The evaluation points corresponding to the kdeEstimates.</param>
        /// <param name="j">The index up to which to calculate the cumulative probability.</param>
        /// <returns>The approximate CDF value at evalPoints[j].</returns>
        private static double CumulativeDistribution(double[] kdeEstimates, double[] evalPoints, int j)
        {
            double cumulativeSum = 0;
            for (int i = 1; i <= j; i++)
            {
                cumulativeSum += 0.5 * (kdeEstimates[i] + kdeEstimates[i - 1]) * (evalPoints[i] - evalPoints[i - 1]);
            }
            return cumulativeSum;
        }


        /// <summary>
        /// Kernel function.
        /// </summary>
        /// <param name="u">The input value.</param>
        /// <param name="kernel">The name of the kernel function.</param>
        /// <returns>The kernel value.</returns>
        /// <exception cref="ArgumentException">Thrown if an invalid kernel is specified.</exception>
        private static double KernelFunction(double u, string kernel)
        {
            return kernel.ToLower() switch
            {
                "normal" or "gaussian" => 1.0 / Math.Sqrt(2 * Math.PI) * Math.Exp(-u * u / 2), // Gaussian kernel
                "logistic" => 1 / (Math.Exp(u) + 2 + Math.Exp(-u)),
                "sigmoid" => 2 / Math.PI * (1 / (Math.Exp(u) + Math.Exp(-u))),
                "rectangular" => Math.Abs(u) <= 1 ? 0.5 : 0,
                "triangular" => Math.Abs(u) <= 1 ? (1 - Math.Abs(u)) : 0,
                "epanechnikov" => Math.Abs(u) <= 1 ? 3.0 / 4.0 * (1 - u * u) : 0,
                "quartic" => Math.Abs(u) <= 1 ? 15.0 / 16.0 * Math.Pow(1 - u * u, 2) : 0,
                "triweight" => Math.Abs(u) <= 1 ? 35.0 / 32.0 * Math.Pow(1 - u * u, 3) : 0,
                "cosine" => Math.Abs(u) <= 1 ? Math.PI / 4 * Math.Cos(Math.PI / 2 * u) : 0,
                _ => throw new ArgumentException("Invalid kernel specified."),
            };
        }

        /// <summary>
        /// Calculates the factorial of a non-negative integer.
        /// </summary>
        /// <param name="x">The non-negative integer.</param>
        /// <returns>The factorial of x.</returns>
        private static long Factorial(int x)
        {
            if (x < 0)
            {
                throw new ArgumentException("x must be non-negative.");
            }

            if (x == 0 || x == 1)
            {
                return 1;
            }

            long result = 1;
            for (int i = 2; i <= x; i++)
            {
                result *= i;
            }

            return result;
        }

        /// <summary>
        /// Calculates the Gamma function value for the given z using the Lanczos approximation.
        /// </summary>
        /// <param name="z">The value for which to calculate the Gamma function.</param>
        /// <returns>The Gamma function value at z.</returns>
        private static double Gamma(double x)
        {
            double[] p = {
                0.99999999999980993,
                676.52480940015306,
                -1259.1513357252162,
                773.1674073620051,
                -1385.710331296526,
                465.2362892704206,
                -75.6111845769078,
                0.5631938460520997,
                -0.000000000000005383
            };

            double z = x + 0.5;
            double sum = p[0];

            for (int i = 1; i < p.Length; i++)
            {
                sum += p[i] / z;
                z += 1;
            }

            double t = x + 7.5;
            return Math.Sqrt(2 * Math.PI) * Math.Pow(t, x + 0.5) * Math.Exp(-t) * sum;
        }

        /// <summary>
        /// Calculates the value of the digamma function ψ(x), which is the first derivative of the logarithm of the gamma function.
        /// </summary>
        /// <param name="x">The value at which to evaluate the digamma function.</param>
        /// <returns>The value of the digamma function ψ(x).</returns>
        private static double Digamma(double x)
        {
            const double EULER_MASCHERONI_CONSTANT = 0.57721566490153286060;

            if (x <= 0)
            {
                throw new ArgumentException("x must be positive.");
            }

            double result = -EULER_MASCHERONI_CONSTANT;
            for (int k = 0; k < 100; k++)
            {
                result += (1.0 / (k + 1)) - (1.0 / (x + k));
            }

            return result;
        }

        /// <summary>
        /// Calculates the value of the binomial coefficient C(n, k).
        /// </summary>
        /// <param name="n">The total number of items.</param>
        /// <param name="k">The number of items to choose.</param>
        /// <returns>The binomial coefficient C(n, k).</returns>
        private static long BinomialCoefficient(int n, int k)
        {
            if (k > n)
            {
                return 0;
            }

            long result = 1;
            for (int i = 1; i <= k; i++)
            {
                result = result * (n - i + 1) / i;
            }

            return result;
        }

        /// <summary>
        /// Calculates the value of the Stirling number of the second kind S(n, k).
        /// </summary>
        /// <param name="n">The total number of items.</param>
        /// <param name="k">The number of non-empty subsets.</param>
        /// <returns>The Stirling number of the second kind S(n, k).</returns>
        private static long StirlingNumberSecondKind(int n, int k)
        {
            if (n == k)
            {
                return 1;
            }
            if (k == 0 || k > n)
            {
                return 0;
            }

            return StirlingNumberSecondKind(n - 1, k - 1) + k * StirlingNumberSecondKind(n - 1, k);
        }

        public class StatisticsError(string message) : Exception(message)
        {
        }
    }
}
