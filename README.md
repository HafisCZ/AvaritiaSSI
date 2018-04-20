# AvaritiaSSI

Function|Return|Parameters
------------|-|-----------
`SqlConnector.SqlConnector`|`SqlConnector`|`SqlConnectionString, Boolean`
`SqlConnector.Open`||
`SqlConnector.Close`||
`SqlConnector.Dispose`||
`SqlConnector.SetTransaction`||`IsolationLevel`
`SqlConnector.Log`||`String`
`SqlConnector.ExecuteQuerry`||`SqlConnector, SqlQuerryRequest, CommandBehaviour`
`SqlConnector.ExecuteNonQuerry`||`SqlConnector, SqlNonQuerryRequest`
`SqlConnector.Execute`||`SqlConnector, ISqlRequest`
`SqlMapper.SqlMapper`|`SqlMapper`|`SqlConnector, String`
`SqlMapper.GetTemplate`|`SqlDatabaseTemplate`|`SqlConnector, String`
`SqlMapper.Select`||`SqlMapper, List, String, String[]`
`SqlMapper.Select`||`SqlMapper, List, String, String, String[]`
`SqlMapper.Update`||`SqlMapper, ISqlUserRecord`
`SqlMapper.Insert`||`SqlMapper, IsqlUserRecord`
`SqlMapper.Select`||`SqlMapper, List, String, String[]`
`SqlMapper.Update`||`SqlMapper, SqlDynamicRecord`
`SqlMapper.Insert`||`SqlMapper, SqlDynamicRecord, String`
`SqlMapper.GetDebug`|`String`|`SqlMapper`
`IsqlUserRecord.GetTableLabel`|`String`|
`SqlDynamicRecord.SqlDynamicRecord`|`SqlDynamicRecord`|`String`
`SqlDynamicRecord.HasProperty`|`Boolean`|`String`
`SqlDynamicRecord.TrySetMember`|`Boolean`|`SetMemberBinder, Object`
`SqlDynamicRecord.TryGetMember`|`Boolean`|`GetMemberBinder, Object`
`SqlDynamicRecord.GetDynamicMemberNames`|`IEnumerable`|
`SqlNonQuerryRequest.MakeTableConstraintRequest`|`SqlQuerryRequest`|
`SqlNonQuerryRequest.MakeTableDefinitionRequest`|`SqlQuerryRequest`|`String`
`SqlNonQuerryRequest.MakeColumnDefinitionRequest`|`SqlQuerryRequest`|`String`
`SqlNonQuerryRequest.MakeSelectRequest`|`SqlQuerryRequest`|`String, String[], String`
`SqlTableTemplate.MakeHeaders`||`SqlTableTemplate`
`SqlDatabaseTemplate.MakeTables`||`SqlDatabaseTemplate`
`SqlConnectionString.SqlConnectionString`|`SqlConnectionString`|`String, String`
`SqlConnectionString.Login`|`SqlConnectionString`|`String, String`
`SqlConnectionString.SetTrusted`|`SqlConnectionString`|`Boolean``
`SqlConnectionString.Reset`||
`SqlConnectionString.ToString`|`String`|
