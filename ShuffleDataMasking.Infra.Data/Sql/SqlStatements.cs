
using ShuffleDataMasking.Domain.Abstractions.Helpers;
using ShuffleDataMasking.Domain.Masking.Entities;
using ShuffleDataMasking.Domain.Masking.Models.Dtos;
using System;

namespace ShuffleDataMasking.Infra.Data.Sql
{
    internal static class SqlStatements
    {
        private static readonly string _createdBy = CreatorHelper.GetEntityCreatorIdentity();

        public static string SelectIntrospectionTable { get; } = $@"SELECT
                [introspection_table].[id] as {nameof(IntrospectionTable.Id)},
                [introspection_table].[table_name] as {nameof(IntrospectionTable.TableName)},
                [introspection_table].[database_id] as {nameof(IntrospectionTable.DatabaseId)}
            FROM [introspection_table] WITH(NOLOCK)
            INNER JOIN [introspection_database] ON [introspection_table].database_id = [introspection_database].id
            WHERE [introspection_database].database_name = @database";

        public static string AlterTableNoCheckConstraint(string _tableName)
        {
            return $"ALTER TABLE {_tableName} NOCHECK CONSTRAINT ALL";
        }

        public static string AlterTableCheckConstraint(string _tableName)
        {
            return $"ALTER TABLE {_tableName} WITH CHECK CHECK CONSTRAINT ALL";
        }

        public static string InsertProcessError { get; } = $@" INSERT INTO
                [PROCESS_ERROR]
                (TABLE_ID, ERROR_TYPE_ID, ERROR_DESCRIPTION, ORIGINAL_QUERY, QUEUE_PROCESS_ID, CREATED_AT_DT, CREATED_BY_DS)
            VALUES(@tableId, @errorTypeId, @errorDescription, @originalQuery, @queueProcessId, '{DateTime.Now}', '{_createdBy}');";

        public static string SelectIntrospectionColumn { get; } = $@"SELECT
                [introspection_column].[id] as {nameof(IntrospectionColumn.Id)},
                [introspection_column].[column_name] as {nameof(IntrospectionColumn.ColumnName)},
                [introspection_column].[type_of_mask] as {nameof(IntrospectionColumn.TypeOfMask)}
            FROM [introspection_column] WITH(NOLOCK)
            WHERE [introspection_column].[table_id] = @tableId AND [introspection_column].[type_of_mask] <> 0";

        public static string SelectOriginalData { get; } = $@"SELECT
                [DATA_COLLECTOR].[ORIGINAL_DATA] as {nameof(DataCollector.OriginalData)},
                [DATA_COLLECTOR].[MASKED_DATA] as {nameof(DataCollector.MaskedData)}
            FROM [DATA_COLLECTOR] WITH(NOLOCK)
            WHERE [DATA_COLLECTOR].[ORIGINAL_DATA] = @originalData AND [DATA_COLLECTOR].[MASK_TYPE_ID] = @typeOfMaskId";

        public static string SelectCountMaskedData { get; } = $@"SELECT 
                COUNT(1)
            FROM [DATA_COLLECTOR] WITH(NOLOCK)
            WHERE [DATA_COLLECTOR].[MASKED_DATA] = @maskedData";

        public static string UpdateConstraintDisabled { get; } = $@"UPDATE 
                [introspection_database]
            SET [introspection_database].[constraint_disabled] = @constraintDisabled
            WHERE [introspection_database].[database_name] = @database;";

        public static string UpdateProcessStarted { get; } = $@"UPDATE 
                [introspection_database]
            SET [introspection_database].[process_started] = @processStarted
            WHERE [introspection_database].[database_name] = @database;";

        public static string UpdateProcessedMasking { get; } = $@"UPDATE 
                [datamasking_queueprocess]
            SET [datamasking_queueprocess].[processed_masking] = @processedMasking
            WHERE [datamasking_queueprocess].[id] = @queueId";

        public static string UpdateQueueStartProcess { get; } = $@"UPDATE 
                [datamasking_queueprocess]
            SET [datamasking_queueprocess].[start_process] = @startProcess
            WHERE [datamasking_queueprocess].[id] = @queueId;";

        public static string UpdateQueueEndedProcess { get; } = $@"UPDATE 
                [datamasking_queueprocess]
            SET [datamasking_queueprocess].[ended_process] = @endedProcess
            WHERE [datamasking_queueprocess].[id] = @queueId;";

        public static string SelectProcessedMasking { get; } = $@"SELECT 
                [datamasking_queueprocess].[processed_masking]
            FROM [datamasking_queueprocess] WITH(NOLOCK)
            WHERE [datamasking_queueprocess].[id] = @queueId;";

        public static string RemoveErrorFromDatabase { get; } = $@"DELETE
            FROM [PROCESS_ERROR]
            WHERE [PROCESS_ERROR].[PROCESS_ERROR_ID] = @processErrorId";

        public static string SelectRelativeTable { get; } = $@"SELECT
                secondary_table.table_name AS {nameof(RelationDto.TableName)},
                secondary_column.column_name AS {nameof(RelationDto.ColumnName)},
		        principal_column.column_name AS {nameof(RelationDto.FatherColumnName)}
	        FROM introspection_relativetable
	        INNER JOIN introspection_table principal_table ON principal_table.id = introspection_relativetable.principal_table_id 
	        INNER JOIN introspection_table secondary_table ON secondary_table.id = introspection_relativetable.secondary_table_id 
	        INNER JOIN introspection_column principal_column ON principal_column.id = introspection_relativetable.principal_column_name_id 
	        INNER JOIN introspection_column secondary_column ON secondary_column.id = introspection_relativetable.secondary_column_name_id 
	        WHERE principal_table.table_name = @tableName AND principal_table.table_name <> @fatherTableName";
        
        public static string SelectColumnNameOfPrimaryKey { get; } = $@"SELECT
                COL_NAME(INDEX_COLUMNS.OBJECT_ID, INDEX_COLUMNS.COLUMN_ID)
            FROM SYS.INDEXES AS INDEXES
            INNER JOIN SYS.INDEX_COLUMNS AS INDEX_COLUMNS ON INDEXES.OBJECT_ID = INDEX_COLUMNS.OBJECT_ID AND INDEXES.INDEX_ID = INDEX_COLUMNS.INDEX_ID
            WHERE INDEXES.IS_PRIMARY_KEY = 1 AND OBJECT_NAME(INDEX_COLUMNS.OBJECT_ID) = @tableName";


        public static string InsertPrimaryKey { get; } = $@"INSERT INTO
                TEMPORAL_PRIMARY_KEY (ColumnName, PrimaryKey, FatherTableName)
            VALUES (@columnName, @primaryKey, @tableName)";

        public static string CreateTemporalTable { get; } = $@"
            IF OBJECT_ID(N'TEMPORAL_PRIMARY_KEY', N'U') IS NOT NULL  
                DROP TABLE TEMPORAL_PRIMARY_KEY;
             CREATE TABLE TEMPORAL_PRIMARY_KEY (
	            Id bigint IDENTITY(1,1) NOT NULL,
	            ColumnName nvarchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	            PrimaryKey nvarchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	            FatherTableName nvarchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	            CONSTRAINT PK__TEMPORAL_PRIMARY_KEY__Id PRIMARY KEY (Id)
            )
            CREATE NONCLUSTERED INDEX TEMPORAL_PRIMARY_KEY__TEMPORAL_INDEX ON TEMPORAL_PRIMARY_KEY(ColumnName, FatherTableName);";

        public static string DropTemporalTable { get; } = "DROP TABLE TEMPORAL_PRIMARY_KEY;";

        public static string InsertPrimaryKeyTopX { get; } = $@"INSERT INTO
            TEMPORAL_PRIMARY_KEY (
                ColumnName, 
                PrimaryKey,
                FatherTableName
            )
            SELECT TOP(@totalValue) '@columnName', @columnName, '@tableName' FROM @tableName
            WHERE @columnName NOT IN (
                SELECT PrimaryKey FROM TEMPORAL_PRIMARY_KEY WHERE ColumnName = '@columnName' AND FatherTableName = '@tableName'
            ) 
            ORDER BY NEWID()";
    }
}
