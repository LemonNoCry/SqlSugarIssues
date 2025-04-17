using System.Data;
using SqlSugar;

namespace SqlSugarIssues;

public class SqlSugarSerializeService : ISerializeService, ISugarDataConverter
{
    private static readonly ISerializeService _instance = new SerializeService();

    #region ISerializeService

    public string SerializeObject(object value)
    {
        try
        {
            return _instance.SerializeObject(value);
        }
        catch
        {
            return default;
        }
    }

    public string SugarSerializeObject(object value)
    {
        try
        {
            return _instance.SugarSerializeObject(value);
        }
        catch
        {
            return default;
        }
    }

    public T DeserializeObject<T>(string value)
    {
        try
        {
            return _instance.DeserializeObject<T>(value);
        }
        catch
        {
            return default;
        }
    }

    #endregion

    #region ISugarDataConverter

    public SugarParameter ParameterConverter<T>(object columnValue, int columnIndex)
    {
        var name = "@myp" + columnIndex;
        var str = SerializeObject(columnValue);

        str = str.Replace("\\\"", "\"").Trim('"');
        return new SugarParameter(name, str);
    }

    public T QueryConverter<T>(IDataRecord dataRecord, int dataRecordIndex)
    {
        var str = dataRecord.GetValue(dataRecordIndex) + "";
        return DeserializeObject<T>(str);
    }

    #endregion
}