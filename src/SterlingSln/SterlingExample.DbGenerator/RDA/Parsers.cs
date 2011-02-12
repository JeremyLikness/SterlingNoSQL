using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using SterlingExample.Model;

namespace SterlingExample.DbGenerator.RDA
{
    /// <summary>
    ///     Parsers for the food database
    /// </summary>
    public static class Parsers
    {
        private const string FOOD_GROUP = "FD_GROUP.txt";
        private const string FOOD_DESCRIPTION = "FOOD_DES.txt";
        private const string NUTRIENT_DEFINITION = "NUTR_DEF.txt";
        private const string NUTRIENT_DATA = "NUT_DATA.txt";

        /// <summary>
        ///     Parse a line from the record format
        /// </summary>
        /// <param name="line">The line</param>
        /// <returns>The columns</returns>
        private static List<string> _ParseLine(string line)
        {
            const char COLUMN_DELIMETER = '^';
            const string QUOTE = @"~";

            return line.Split(COLUMN_DELIMETER).Select(item => item.Replace(QUOTE, string.Empty)).ToList();
        }

        /// <summary>
        ///     Convert the source to the full resource stream
        /// </summary>
        /// <param name="src">The source file</param>
        /// <returns>The source as a stream</returns>
        private static Stream AsResourceStream(this string src)
        {
            return Application.GetResourceStream(new Uri(string.Format("RDA/{0}", src), UriKind.Relative)).Stream;
        }

        /// <summary>
        ///     Parses a stream to an entity list
        /// </summary>
        /// <typeparam name="T">The type to parse</typeparam>
        /// <param name="stream">The stream</param>
        /// <param name="parser">The parser for the stream</param>
        /// <returns>The iteration of items</returns>
        private static IEnumerable<T> Parse<T>(this Stream stream, Func<List<string>, T> parser)
        {
            using (TextReader tr = new StreamReader(stream))
            {
                var done = false;
                do
                {
                    var line = tr.ReadLine();
                    if (string.IsNullOrEmpty(line))
                    {
                        done = true;
                    }
                    else
                    {
                        yield return parser(_ParseLine(line));
                    }
                } while (!done);
            }
        }

        /// <summary>
        ///     Gets the list of food groups
        /// </summary>
        /// <returns>The list of food groups</returns>
        public static IEnumerable<FoodGroup> GetFoodGroups()
        {
            return
                FOOD_GROUP.AsResourceStream().Parse(list => new FoodGroup {Id = int.Parse(list[0]), GroupName = list[1]});
        }

        private static double _AsDouble(this string s)
        {
            double retVal;
            return double.TryParse(s, out retVal) ? retVal : 0;
        }

        /// <summary>
        ///     Get the nutrient data
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Tuple<int,int,double>> GetNutrientData()
        {
            return NUTRIENT_DATA.AsResourceStream().Parse(list => Tuple.Create(
                int.Parse(list[0]),
                int.Parse(list[1]),
                list[2]._AsDouble()));
        }

        /// <summary>
        ///     Gets the list of food groups
        /// </summary>
        /// <returns>The list of food groups</returns>
        public static IEnumerable<FoodDescription> GetFoodDescriptions()
        {            
            return
                FOOD_DESCRIPTION.AsResourceStream().Parse(list =>
                                                              {
                                                                  var id = int.Parse(list[0]);
                                                                  return new FoodDescription
                                                                             {
                                                                                 Id = id,
                                                                                 FoodGroupId = int.Parse(list[1]),
                                                                                 Description = list[2],
                                                                                 Abbreviated = list[3],
                                                                                 CommonName = list[4],
                                                                                 Manufacturer = list[5],
                                                                                 // skip 6th item (survey)
                                                                                 InedibleParts = list[7],
                                                                                 PctRefuse = list[8]._AsDouble(),
                                                                                 ScientificName = list[9],
                                                                                 NitrogenFactor = list[10]._AsDouble(),
                                                                                 ProteinCalories = list[11]._AsDouble(),
                                                                                 FatCalories = list[12]._AsDouble(),
                                                                                 CarbohydrateCalories =
                                                                                     list[13]._AsDouble()                                                                                 
                                                                             };
                                                              });
        }

        /// <summary>
        ///     Gets the list of nutrient definitions
        /// </summary>
        /// <returns>The list of nutrient definitions</returns>
        public static IEnumerable<NutrientDefinition> GetNutrientDefinitions()
        {
            return
                NUTRIENT_DEFINITION.AsResourceStream().Parse(list =>
                                                             new NutrientDefinition
                                                                 {
                                                                     Id = int.Parse(list[0]),
                                                                     UnitOfMeasure = list[1],
                                                                     Tag = list[2],
                                                                     Description = list[3],
                                                                     // skip 4th colum (decimal places for rounding)
                                                                     SortOrder = int.Parse(list[4])
                                                                 });
        }
    }
}