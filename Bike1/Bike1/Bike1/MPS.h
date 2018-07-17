#include <sqlext.h>
#include <sqltypes.h>
#pragma once
class MPS
{
public:
	MPS();
	~MPS();
	void getMPS(SQLHANDLE sqlStmtHandle);
};

