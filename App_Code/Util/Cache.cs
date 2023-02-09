using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

namespace Wayfinder {

public class Cache<T> where T:new(){

private string table;
private string keyColumn;
private List<CachedAttribute> attributes;
private bool keyIsParameter;

public Cache(List<CachedAttribute> attributes, string table, string keyColumn)
	{
	this.attributes = attributes;
	this.table = table;
	this.keyColumn = keyColumn;

	keyIsParameter = attributes.Any(a=>a.DBColumn == keyColumn);
	}

private string ColumnList()
	{
	var columns = new List<string>(attributes.Select(a=>a.DBColumn));

	if(!keyIsParameter) columns.Add(keyColumn);

	return String.Join(", ", columns.ToArray());
	}

private string Parameters()
	{
	var parameters = new List<string>(attributes.Select(a=>a.Parameter));

	if(!keyIsParameter) parameters.Add("@" + keyColumn);

	return String.Join(", ", parameters.ToArray());
	}

public T RetrieveOne(string key)
	{
	T result = default(T);

	using (SqlConnection conn = new SqlConnection(Constants.DBDSN))
		{
		conn.Open();

		string query = String.Format(
			@"select {0} from {1} where {2} = @{2}",
			  ColumnList(),
			  table,
			  keyColumn);

		// Console.WriteLine(query);

		using (SqlCommand command = new SqlCommand(query, conn))
			{
			command.Parameters.AddWithValue("@" + keyColumn, key);

			SqlDataReader reader = command.ExecuteReader();
			if(reader.HasRows)
				{
				reader.Read();
				result = MapAttributes(reader);
				}

			}
		}

	return result;
	}

public List<T> Retrieve(IEnumerable<string> keys)
	{
	List<T> results = new List<T>();

	using (SqlConnection conn = new SqlConnection(Constants.DBDSN))
		{
		conn.Open();

		string[] paramNames = keys.Select((k, i)=>"@key" + i.ToString()).ToArray();
		string inClause = string.Join(", ", paramNames);
		
		string query = String.Format(
			@"select {0} from {1} where {2} in ({3})",
			ColumnList(),
			table,
			keyColumn,
			inClause);

		// Console.WriteLine(query);

		using (SqlCommand command = new SqlCommand(query, conn))
			{
			foreach(string key in keys)
				{
				command.Parameters.AddWithValue(paramNames[command.Parameters.Count], key);
				}

			using(SqlDataReader reader = command.ExecuteReader())
				{
				if(reader.HasRows)
					{
					while(reader.Read())
						{
						results.Add(MapAttributes(reader));
						}
					}
				}
			}
		}

	return results;
	}

public T MapAttributes(SqlDataReader reader)
	{
	T result = new T();
	Type type = result.GetType();

	foreach(CachedAttribute a in attributes)
		{
		PropertyInfo prop = type.GetProperty(a.Attribute);

		if(!(reader[a.DBColumn] is DBNull))
			prop.SetValue(result, reader[a.DBColumn], null);
		}

	return result;
	}

public bool Store(string key, T toCache)
	{
	StoreResult result = StoreWithResult(key, toCache);
	return result.success;
	}

public StoreResult StoreWithResult(string key, T toCache)
	{
	StoreResult result = new StoreResult { success = false };
	using (SqlConnection conn = new SqlConnection(Constants.DBDSN))
		{
		try {
			conn.Open();

			string query = String.Format(
				@"delete from {0} where {1} = @{1};
				insert into {0} ({2}) values ({3})",
				table, keyColumn,
				ColumnList(),
				Parameters());

			// Console.WriteLine(query);

			using (SqlCommand command = new SqlCommand(query, conn))
				{
				Type type = toCache.GetType();

				if(!keyIsParameter)
					command.Parameters.AddWithValue("@" + keyColumn, key);

				foreach(CachedAttribute a in attributes)
					{
					PropertyInfo prop = type.GetProperty(a.Attribute);
					Object valueToCache = prop.GetValue(toCache);
					if(valueToCache == null)
						valueToCache = DBNull.Value;

					command.Parameters.AddWithValue(a.Parameter, valueToCache);
					}

				result.affected = command.ExecuteNonQuery();
				}
			result.success = true;
			}
		catch(SqlException except)
			{
			result.success = false;
			result.exception = except;
			}
		}
	return result;
	}

}

public class StoreResult {

public SqlException exception {get; set;}
public bool success {get; set;}
public int affected {get; set;}

}

}
