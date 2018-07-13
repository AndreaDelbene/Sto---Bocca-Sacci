#include "stdafx.h"
#include "BikeDBConnection.h"


BikeDBConnection::BikeDBConnection()
{
}


BikeDBConnection::~BikeDBConnection()
{
}

void BikeDBConnection::createConnection()
{
	#define SQL_RESULT_LEN 240
	#define SQL_RETURN_CODE_LEN 1000
		//define handles and variables
		SQLHANDLE sqlConnHandle;
		SQLHANDLE sqlStmtHandle;
		SQLHANDLE sqlEnvHandle;
		SQLWCHAR retconstring[SQL_RETURN_CODE_LEN];
		//initializations
		sqlConnHandle = NULL;
		sqlStmtHandle = NULL;
		//allocations
		if (SQL_SUCCESS != SQLAllocHandle(SQL_HANDLE_ENV, SQL_NULL_HANDLE, &sqlEnvHandle))
			goto COMPLETED;
		if (SQL_SUCCESS != SQLSetEnvAttr(sqlEnvHandle, SQL_ATTR_ODBC_VERSION, (SQLPOINTER)SQL_OV_ODBC3, 0))
			goto COMPLETED;
		if (SQL_SUCCESS != SQLAllocHandle(SQL_HANDLE_DBC, sqlEnvHandle, &sqlConnHandle))
			goto COMPLETED;
		//output
		cout << "Attempting connection to SQL Server...";
		cout << "\n";
		//connect to SQL Server	
		//I am using a trusted connection and port 1433
		switch (SQLDriverConnect(sqlConnHandle,
			NULL,
			//(SQLWCHAR*)L"DRIVER={SQL Server};SERVER=localhost, 1433;DATABASE=master;UID=username;PWD=password;",
			(SQLWCHAR*)L"DRIVER={SQL Server};SERVER=LAPTOP-DT8KB2TQ\STODB, 1433;DATABASE=stodb;Trusted=true;",
			SQL_NTS,
			retconstring,
			1024,
			NULL,
			SQL_DRIVER_NOPROMPT)) {
		case SQL_SUCCESS:
			cout << "Successfully connected to SQL Server";
			cout << "\n";
			break;
		case SQL_SUCCESS_WITH_INFO:
			cout << "Successfully connected to SQL Server";
			cout << "\n";
			break;
		case SQL_INVALID_HANDLE:
			cout << "Could not connect to SQL Server";
			cout << "\n";
			goto COMPLETED;
		case SQL_ERROR:
			cout << "Could not connect to SQL Server";
			cout << "\n";
			goto COMPLETED;
		default:
			break;
		}
		//if there is a problem connecting then exit application
		/*if (SQL_SUCCESS != SQLAllocHandle(SQL_HANDLE_STMT, sqlConnHandle, &sqlStmtHandle))
			goto COMPLETED;
		//output
		cout << "\n";
		cout << "Executing T-SQL query...";
		cout << "\n";
		//if there is a problem executing the query then exit application
		//else display query result
		if (SQL_SUCCESS != SQLExecDirect(sqlStmtHandle, (SQLWCHAR*)L"SELECT @@VERSION", SQL_NTS)) {
			cout << "Error querying SQL Server";
			cout << "\n";
			goto COMPLETED;
		}
		else {
			//declare output variable and pointer
			SQLCHAR sqlVersion[SQL_RESULT_LEN];
			SQLINTEGER ptrSqlVersion;
			while (SQLFetch(sqlStmtHandle) == SQL_SUCCESS) {
				SQLGetData(sqlStmtHandle, 1, SQL_CHAR, sqlVersion, SQL_RESULT_LEN, &ptrSqlVersion);
				//display query result
				cout << "\nQuery Result:\n\n";
				cout << sqlVersion << endl;
			}
		}*/
		//close connection and free resources
	COMPLETED:
		SQLFreeHandle(SQL_HANDLE_STMT, sqlStmtHandle);
		SQLDisconnect(sqlConnHandle);
		SQLFreeHandle(SQL_HANDLE_DBC, sqlConnHandle);
		SQLFreeHandle(SQL_HANDLE_ENV, sqlEnvHandle);
		cout << "";
		getchar();
}
