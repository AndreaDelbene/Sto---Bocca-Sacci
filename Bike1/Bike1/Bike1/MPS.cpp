#include "stdafx.h"
#include "MPS.h"
#include <thread>
#include <chrono>
#include <vector>
#include <numeric>
#include <iostream>
#include <ctime>
#include <sqlext.h>
#include <sqltypes.h>
#include <string>

using namespace std;
using namespace std::chrono;

//Determining platform's tic period
const double microsPerClkTic{
	1.0E6 * system_clock::period::num / system_clock::period::den
};

//Processing period 
const milliseconds intervalPeriodMillis{ 1000 };

MPS::MPS()
{
}


MPS::~MPS()
{
}

void MPS::getMPS(SQLHANDLE sqlStmtHandle)
{
	//Let's define the start time and the next time the thread will wake up
	system_clock::time_point currentStartTime{ system_clock::now() };
	system_clock::time_point nextStartTime{ currentStartTime };

	while (true)
	{
		SQLRETURN retCode;
		//getting our "wakeup" time
		currentStartTime = system_clock::now();

		//determing the next time the thread will get executed
		nextStartTime = currentStartTime + intervalPeriodMillis;
		// Preparing the query to execute
		//SQLWCHAR query = (SQLWCHAR)"SELECT * FROM stodb.dbo.mps WHERE running == 0";
		cout << "dioprocodo";
		SQLPrepare(sqlStmtHandle, (SQLWCHAR*) "SELECT * FROM stodb.dbo.mps WHERE running == 0", SQL_NTS);
		// And executing the query itself
		if (SQL_SUCCESS != SQLExecute(sqlStmtHandle))
		{
			//getting the data returned
			retCode = SQLFetch(sqlStmtHandle);
			//if there is something (new MPS!!!), then i update the table and I fulfill the table "statoordini"
			if (retCode != SQL_NO_DATA)
			{
				SQLLEN numRows;
				SQLSMALLINT numCols;
				retCode = SQLRowCount(sqlStmtHandle, &numRows);
				retCode = SQLNumResultCols(sqlStmtHandle, &numCols);

				for (int i = 0; i < numRows; i++)
				{
					int idLotto[11], quantita[11], priorita[11];
					char tipoTelaio[100], colore[100], start[100], dueDate[100];
					int running[1];
					SQLINTEGER numBytes;


					retCode = SQLGetData(sqlStmtHandle, 1, SQL_INTEGER, idLotto, 10, &numBytes);
					retCode = SQLGetData(sqlStmtHandle, 2, SQL_DATETIME, start, 100, &numBytes);
					retCode = SQLGetData(sqlStmtHandle, 3, SQL_DATETIME, dueDate, 100, &numBytes);
					retCode = SQLGetData(sqlStmtHandle, 4, SQL_INTEGER, quantita, 10, &numBytes);
					retCode = SQLGetData(sqlStmtHandle, 5, SQL_VARCHAR, tipoTelaio, 100, &numBytes);
					retCode = SQLGetData(sqlStmtHandle, 6, SQL_VARCHAR, colore, 100, &numBytes);
					retCode = SQLGetData(sqlStmtHandle, 7, SQL_INTEGER, priorita, 11, &numBytes);
					retCode = SQLGetData(sqlStmtHandle, 8, SQL_TINYINT, running, 1, &numBytes);
					
					SQLFreeStmt(sqlStmtHandle, SQL_RESET_PARAMS);

					//SQLWCHAR query = (SQLWCHAR*)"INSERT INTO stodb.dbo.statoordini (idLotto,startPianificata,startEffettiva,dueDatePianificata,dueDateEffettiva,quantitaDesiderata,quantitaProdotta,tipoTelaio,stato,descrizione) VALUES(?,?,?,?,?,?,?,?,?,?)";
					
					SQLPrepare(sqlStmtHandle, (SQLWCHAR*)"INSERT INTO stodb.dbo.statoordini (idLotto,startPianificata,startEffettiva,dueDatePianificata,dueDateEffettiva,quantitaDesiderata,quantitaProdotta,tipoTelaio,stato,descrizione) VALUES(?,?,?,?,?,?,?,?,?,?)", SQL_NTS);
					SQLBindParameter(sqlStmtHandle, 2, SQL_PARAM_INPUT, SQL_INTEGER, SQL_INTEGER, 11, 0, &idLotto, 0, (SQLINTEGER*) 11);
					SQLBindParameter(sqlStmtHandle, 3, SQL_PARAM_INPUT, SQL_DATETIME, SQL_CHAR, 100, 0, &start, 0, (SQLINTEGER*) 100);
					SQLBindParameter(sqlStmtHandle, 4, SQL_PARAM_INPUT, SQL_DATETIME, SQL_CHAR, 100, 0, &start, 0, (SQLINTEGER*) 100);
					SQLBindParameter(sqlStmtHandle, 5, SQL_PARAM_INPUT, SQL_DATETIME, SQL_CHAR, 100, 0, &dueDate, 0, (SQLINTEGER*) 100);
					SQLBindParameter(sqlStmtHandle, 6, SQL_PARAM_INPUT, SQL_DATETIME, SQL_CHAR, 100, 0, &dueDate, 0, (SQLINTEGER*) 100);
					SQLBindParameter(sqlStmtHandle, 7, SQL_PARAM_INPUT, SQL_INTEGER, SQL_INTEGER, 11, 0, &quantita, 0, (SQLINTEGER*) 11);
					SQLBindParameter(sqlStmtHandle, 8, SQL_PARAM_INPUT, SQL_INTEGER, SQL_INTEGER, 11, 0, 0, 0, (SQLINTEGER*) 11);
					SQLBindParameter(sqlStmtHandle, 9, SQL_PARAM_INPUT, SQL_VARCHAR, SQL_CHAR, 100, 0, &tipoTelaio, 0, (SQLINTEGER*) 100);
					SQLBindParameter(sqlStmtHandle, 10, SQL_PARAM_INPUT, SQL_VARCHAR, SQL_CHAR, 100, 0, "", 0, (SQLINTEGER*) 500);

					if (SQL_SUCCESS != SQLExecute(sqlStmtHandle))
					{
						cout << "error in executing the query";
						this_thread::sleep_until(nextStartTime);
						return;
					}
					SQLFreeStmt(sqlStmtHandle, SQL_RESET_PARAMS);

					//SQLCHAR query1 = (SQLCHAR)"INSERT INTO stodb.dbo.mps (running) VALUES (1)";
					SQLPrepare(sqlStmtHandle,(SQLWCHAR*)"INSERT INTO stodb.dbo.mps (running) VALUES (1)", SQL_NTS);

					if (SQL_SUCCESS != SQLExecute(sqlStmtHandle))
					{
						cout << "error in executing the query";
						this_thread::sleep_until(nextStartTime);
						return;
					}


				}
			}
			else
			{
				//no data avaiable
			}
		}
		
		//Sleep till our next period start time
		this_thread::sleep_until(nextStartTime);
	}

}
