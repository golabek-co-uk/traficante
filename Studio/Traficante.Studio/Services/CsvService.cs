//using CsvHelper;
//using Metsys.Bson.Configuration;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using Traficante.TSQL;

//namespace Traficante.Studio.Services
//{
//    public class CsvService
//    {
//        public object Run(string sql)
//        {
//            Engine engine = new Engine();
//            engine.AddFunction("csv", (string pathToFile) =>
//            {
                
//                using (var reader = new StreamReader(pathToFile))
//                using (var csv = new CsvReader(reader))
//                {
//                    string[] header = null;
//                    Type returnType = null;
//                    List<object> rows = new List<object>();
//                    while (csv.Read())
//                    {
//                        if (header == null)
//                        {
//                            header = csv.Context.Record;
//                            List<(string name, Type type)> fields = header.Select(x => (x, typeof(string))).ToList();
//                            returnType = new Traficante.TSQL.Evaluator.Visitors.ExpressionHelper().CreateAnonymousType(fields);
//                            continue;
//                        }

//                        object row = Activator.CreateInstance(returnType);
//                        for (int i = 0; i < header.Length; i++)
//                        {

//                            var value = csv.Context.Record[i];
//                            returnType.GetField(header[i]).SetValue(row, value);
//                        }
//                        //var row = csv.GetRecord(returnType);
//                        rows.Add(row);
//                    }
//                    return rows;
//                }
//            }
//            //,getFields: (string pathToFile) =>
//            //{
//            //    return 
//            //}
//            );

//            return engine.Run(sql);
//        }
//    }
//}
