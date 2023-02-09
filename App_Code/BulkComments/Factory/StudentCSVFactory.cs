using System.Collections.Generic;
using System.IO;
using CsvHelper;
using System.Globalization;

namespace BulkComments {

public class StudentCSVFactory {

public static StudentCSV ParseFile(CsvParser parser)
	{
	var result = new StudentCSV();

	result.Fields = new List<string>(parser.Read());
	result.Data = new List<StudentCSVRow>();

	string[] values;
	while ((values = parser.Read()) != null) 
		{
		var row = new Dictionary<string, string>();

		for(var i = 0; i < result.Fields.Count; i++)
			{
			if(values.Length > i)
				{
				row[result.Fields[i]] = values[i].Trim();
				}
			}

		result.Data.Add(new StudentCSVRow { Data=row });
		}

	return result;
	}

public static StudentCSV LoadStudentCSV(StreamReader reader)
	{
	using (var parser = new CsvParser(reader, CultureInfo.CreateSpecificCulture("en-US")))
		{
		return ParseFile(parser);
		}
	}

public static StudentCSV LoadStudentCSV(Stream stream)
	{
	using (var reader = new StreamReader(stream))
		{
		return LoadStudentCSV(reader);
		}
	}

public static StudentCSV LoadStudentCSV(string filename)
	{
	using (var reader = new StreamReader(filename))
		{
		return LoadStudentCSV(reader);
		}
	}
}
}
