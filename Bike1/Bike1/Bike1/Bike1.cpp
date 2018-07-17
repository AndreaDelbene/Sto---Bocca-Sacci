#include "stdafx.h"
#include "BikeDBConnection.h"
#include "MPS.h"
//include the below additional libraries
#include <iostream>
#include <windows.h>
#include <sqlext.h>
#include <sqltypes.h>
#include <sql.h>
#include <thread>
#include <chrono>
//use the std namespace
using namespace std;

void getMPSCaller(SQLHANDLE sqlStmtHandle);

int main()
{
	BikeDBConnection conn;
	SQLHANDLE sqlStmtHandle = conn.createConnection();
	thread mpsThread(getMPSCaller, sqlStmtHandle);
	//thread mpsThread(getMPSCaller,parameters);

	mpsThread.join();
	
}

void getMPSCaller(SQLHANDLE sqlStmtHandle)
{
	MPS mps;
	mps.getMPS(sqlStmtHandle);
}

